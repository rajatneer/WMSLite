using WMSLite.Api.DTOs;
using WMSLite.Api.Models;
using WMSLite.Api.Repositories;

namespace WMSLite.Api.Services;

public class UserService : IUserService
{
    private readonly IJsonRepository<User> _userRepo;
    private readonly IPasswordService _passwordService;
    private readonly IBillingService _billingService;

    public UserService(IJsonRepository<User> userRepo, IPasswordService passwordService, IBillingService billingService)
    {
        _userRepo = userRepo;
        _passwordService = passwordService;
        _billingService = billingService;
    }

    public async Task<UserDto> AddUserAsync(Guid tenantId, CreateUserRequest request)
    {
        if (!await _billingService.CanAddUserAsync(tenantId))
            throw new InvalidOperationException("Seat limit reached for active subscription.");

        var users = await _userRepo.GetAllAsync();
        if (users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Email already exists.");

        var user = new User
        {
            TenantId = tenantId,
            Email = request.Email,
            PasswordHash = _passwordService.Hash(request.Password),
            Role = request.Role
        };

        await _userRepo.InsertAsync(user);
        return new UserDto(user.Id, user.TenantId, user.Email, user.Role, user.CreatedAtUtc);
    }

    public async Task<List<UserDto>> GetUsersAsync(Guid tenantId)
    {
        return (await _userRepo.GetAllAsync())
            .Where(u => u.TenantId == tenantId)
            .Select(u => new UserDto(u.Id, u.TenantId, u.Email, u.Role, u.CreatedAtUtc))
            .ToList();
    }

    public async Task<bool> RemoveUserAsync(Guid tenantId, Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user is null || user.TenantId != tenantId) return false;
        return await _userRepo.DeleteAsync(userId);
    }
}
