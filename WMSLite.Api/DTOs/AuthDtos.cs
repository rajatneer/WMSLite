namespace WMSLite.Api.DTOs;

public record SignupRequest(string TenantName, string AdminEmail, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, Guid TenantId, Guid UserId, string Role, DateTime TrialEndsAtUtc);
