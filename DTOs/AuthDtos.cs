namespace WMSLite.DTOs;

public record SignupRequest(string TenantName, string AdminEmail, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string TenantId, string UserId, string Role);
