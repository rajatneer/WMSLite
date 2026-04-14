using WMSLite.Api.Models;

namespace WMSLite.Api.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}
