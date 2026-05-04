namespace Demo.Architecture.UseCases.Common.Identity;

public record UserInfo(
    string UserId,
    string? Email,
    string[] Roles
);
