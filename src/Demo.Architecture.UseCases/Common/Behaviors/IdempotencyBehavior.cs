using Demo.Architecture.Shared.Serialization;
using Demo.Architecture.UseCases.Common.Attributes;
using Demo.Architecture.UseCases.Common.Errors;
using Demo.Architecture.UseCases.Common.Factories;
using Demo.Architecture.UseCases.Common.Idempotency;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Demo.Architecture.UseCases.Common.Behaviors;

public class IdempotencyBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IIdempotencyService _service;
    private readonly IHttpContextAccessor _http;
    private readonly IdempotencyOptions _options;

    public IdempotencyBehavior(
        IIdempotencyService service,
        IHttpContextAccessor http,
        IOptions<IdempotencyOptions> options)
    {
        _service = service;
        _http = http;
        _options = options.Value;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var context = _http.HttpContext;
        var key = context?.Request.Headers["Idempotency-Key"].FirstOrDefault();

        // 🔍 Check if this request REQUIRES idempotency
        var requiresIdempotency = typeof(TRequest)
            .IsDefined(typeof(RequireIdempotencyAttribute), false);

        // ❗ Missing key but required
        if (string.IsNullOrWhiteSpace(key))
        {
            if (requiresIdempotency)
            {
                var errors = new[]
                {
                    new ValidationError
                    {
                        Identifier = "Idempotency-Key",
                        ErrorMessage = CommonErrors.IdempotencyKeyRequired.Message,
                        ErrorCode = CommonErrors.IdempotencyKeyRequired.Code
                    }
                };

                var result = ArdalisResultFactory.CreateInvalid(typeof(TResponse), errors);

                return (TResponse)result;
            }

            return await next();
        }

        var scopedKey = BuildScopedKey(key, context);
        var requestHash = ComputeHash(request);

        // 1. Check existing
        var existing = await _service.GetAsync(scopedKey);
        if (existing != null)
        {
            if (existing.RequestHash != requestHash)
            {
                var errors = new[]
                {
                    new ValidationError
                    {
                        Identifier = "Idempotency-Key",
                        ErrorMessage = CommonErrors.IdempotencyConflict.Message,
                        ErrorCode = CommonErrors.IdempotencyConflict.Code
                    }
                };

                var result = ArdalisResultFactory.CreateInvalid(typeof(TResponse), errors);

                return (TResponse)result;
            }

            return JsonSerializer.Deserialize<TResponse>(existing.ResponseJson, Shared.Serialization.JsonSerializerDefaults.Options)!;
        }

        // 2. Acquire distributed lock
        var locked = await _service.TryAcquireLockAsync(scopedKey, _options.LockTtl);
        if (!locked)
        {
            var errors = new[]
                {
                    new ValidationError
                    {
                        Identifier = "Idempotency-Key",
                        ErrorMessage = CommonErrors.IdempotencyInProgress.Message,
                        ErrorCode = CommonErrors.IdempotencyInProgress.Code
                    }
                };

            var result = ArdalisResultFactory.CreateInvalid(typeof(TResponse), errors);

            return (TResponse)result;
        }

        try
        {
            // 3. Execute handler
            var response = await next();

            // 4. Save result
            var record = new IdempotencyRecord
            {
                RequestHash = requestHash,
                ResponseJson = JsonSerializer.Serialize(response, Shared.Serialization.JsonSerializerDefaults.Options),
                StatusCode = 200
            };

            await _service.SaveAsync(scopedKey, record, _options.CacheTtl);

            return response;
        }
        finally
        {
            await _service.ReleaseLockAsync(scopedKey);
        }
    }

    private static string BuildScopedKey(string key, HttpContext context)
    {
        /*var userId = context.User?.Identity?.Name ?? "anonymous";
        var path = context.Request.Path;

        return $"{key}:{userId}:{path}";*/
        var path = context.Request.Path;
        return $"{key}:{path}";
    }

    private static string ComputeHash(TRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
    }
}
