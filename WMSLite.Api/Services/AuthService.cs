using WMSLite.Api.DTOs;
using WMSLite.Api.Models;
using WMSLite.Api.Repositories;

namespace WMSLite.Api.Services;

public class AuthService : IAuthService
{
    private readonly IJsonRepository<Tenant> _tenantRepo;
    private readonly IJsonRepository<User> _userRepo;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

    public AuthService(
        IJsonRepository<Tenant> tenantRepo,
        IJsonRepository<User> userRepo,
        IPasswordService passwordService,
        IJwtService jwtService)
    {
        _tenantRepo = tenantRepo;
        _userRepo = userRepo;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> SignupAsync(SignupRequest request)
    {
        var existing = (await _userRepo.GetAllAsync()).Any(u => u.Email.Equals(request.AdminEmail, StringComparison.OrdinalIgnoreCase));
        if (existing)
        {
            throw new InvalidOperationException("Email already registered.");
        }

        var tenant = new Tenant
        {
            Name = request.TenantName,
            CreatedAtUtc = DateTime.UtcNow,
            TrialEndsAtUtc = DateTime.UtcNow.AddDays(16)
        };
        await _tenantRepo.InsertAsync(tenant);

        var admin = new User
        {
            TenantId = tenant.Id,
            Email = request.AdminEmail,
            PasswordHash = _passwordService.Hash(request.Password),
            Role = "Admin"
        };
        await _userRepo.InsertAsync(admin);

        var token = _jwtService.GenerateToken(admin);
        return new AuthResponse(token, tenant.Id, admin.Id, admin.Role, tenant.TrialEndsAtUtc);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = (await _userRepo.GetAllAsync())
            .FirstOrDefault(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));

        if (user is null || !_passwordService.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var tenant = await _tenantRepo.GetByIdAsync(user.TenantId);
        if (tenant is null) return null;

        var token = _jwtService.GenerateToken(user);
        return new AuthResponse(token, tenant.Id, user.Id, user.Role, tenant.TrialEndsAtUtc);
    }
}
