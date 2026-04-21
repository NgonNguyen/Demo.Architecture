using Demo.Architecture.WebAPI.Consumer.Consumers.Products;
using Demo.Architecture.WebAPI.Consumer.Extensions;
using Demo.Architecture.WebAPI.Consumer.Fomatters;
using Demo.Architecture.WebAPI.Consumer.IntegrationEvents;
using Demo.Architecture.WebAPI.Consumer.Serialization;
using Demo.Architecture.WebAPI.Consumer.Service;
using MassTransit;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register RabbitMQ connection as a singleton
/*builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory()
    {
        HostName = "localhost",
        Port = 5672,
        UserName = "guest",
        Password = "guest",
        DispatchConsumersAsync = true
    };

    return factory.CreateConnection();
});

builder.Services.AddHostedService<RabbitMqConsumerService>();*/

// MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // 👇 Register ALL consumers (auto scan)
    x.AddConsumers(typeof(ProductCreatedConsumer).Assembly);

    x.SetEndpointNameFormatter(new QueueEndpointNameFormatter());

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ApplyEntityNamesFromAssembly(
            typeof(ProductCreatedIntegrationEvent).Assembly
        );

        cfg.ApplyEntityNamesFromAssembly(
            typeof(ProductUpdatedIntegrationEvent).Assembly
        );

        cfg.ConfigureEndpoints(context);

        cfg.UseRawJsonSerializer(); // 👈 for multi-language

        cfg.ConfigureJsonSerializerOptions(options =>
        {
            options.Converters.Add(new UlidJsonConverter());
            return options;
        });
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
