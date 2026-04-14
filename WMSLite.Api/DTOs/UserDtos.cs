namespace WMSLite.Api.DTOs;

public record CreateUserRequest(string Email, string Password, string Role);
public record UserDto(Guid Id, Guid TenantId, string Email, string Role, DateTime CreatedAtUtc);
