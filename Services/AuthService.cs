using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WMSLite.DTOs;
using WMSLite.Models;
using WMSLite.Repositories;

namespace WMSLite.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IJsonRepository<Tenant> _tenantRepository;
    private readonly IJsonRepository<AppUser> _userRepository;
    private readonly IJsonRepository<Subscription> _subscriptionRepository;

    public AuthService(
        IConfiguration configuration,
        IJsonRepository<Tenant> tenantRepository,
        IJsonRepository<AppUser> userRepository,
        IJsonRepository<Subscription> subscriptionRepository)
    {
        _configuration = configuration;
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<AuthResponse> SignupAsync(SignupRequest request)
    {
        var tenantId = Guid.NewGuid().ToString("N");
        var tenant = new Tenant
        {
            Id = tenantId,
            TenantId = tenantId,
            Name = request.TenantName,
            CreatedAtUtc = DateTime.UtcNow
        };

        var user = new AppUser
        {
            Id = Guid.NewGuid().ToString("N"),
            TenantId = tenantId,
            Email = request.AdminEmail.ToLowerInvariant(),
            PasswordHash = HashPassword(request.Password),
            Role = "Admin",
            CreatedAtUtc = DateTime.UtcNow
        };

        var subscription = new Subscription
        {
            Id = Guid.NewGuid().ToString("N"),
            TenantId = tenantId,
            TrialEndsAtUtc = DateTime.UtcNow.AddDays(16),
            IsPaidActive = false,
            SeatsPurchased = 1,
            PaidEndsAtUtc = null
        };

        await _tenantRepository.InsertAsync(tenant);
        await _userRepository.InsertAsync(user);
        await _subscriptionRepository.InsertAsync(subscription);

        return new AuthResponse(GenerateToken(user), tenantId, user.Id, user.Role);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var users = await _userRepository.GetAllAsync();
        var user = users.FirstOrDefault(x => x.Email == request.Email.ToLowerInvariant());
        if (user is null || user.PasswordHash != HashPassword(request.Password))
        {
            return null;
        }

        return new AuthResponse(GenerateToken(user), user.TenantId, user.Id, user.Role);
    }

    private string GenerateToken(AppUser user)
    {
        var jwt = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"] ?? throw new InvalidOperationException("Missing JWT secret")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new("tenant_id", user.TenantId),
            new(ClaimTypes.Role, user.Role),
            new(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}
