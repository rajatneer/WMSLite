using WMSLite.DTOs;

namespace WMSLite.Services;

public interface IAuthService
{
    Task<AuthResponse> SignupAsync(SignupRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}
