using WMSLite.Api.DTOs;

namespace WMSLite.Api.Services;

public interface IUserService
{
    Task<UserDto> AddUserAsync(Guid tenantId, CreateUserRequest request);
    Task<bool> RemoveUserAsync(Guid tenantId, Guid userId);
    Task<List<UserDto>> GetUsersAsync(Guid tenantId);
}
