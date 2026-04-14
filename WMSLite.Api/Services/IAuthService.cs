using WMSLite.Api.DTOs;

namespace WMSLite.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> SignupAsync(SignupRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}
