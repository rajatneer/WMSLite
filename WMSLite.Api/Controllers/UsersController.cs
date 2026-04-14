using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSLite.Api.DTOs;
using WMSLite.Api.Services;

namespace WMSLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITenantContext _tenantContext;

    public UsersController(IUserService userService, ITenantContext tenantContext)
    {
        _userService = userService;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> List()
        => Ok(await _userService.GetUsersAsync(_tenantContext.TenantId));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Add([FromBody] CreateUserRequest request)
    {
        try
        {
            return Ok(await _userService.AddUserAsync(_tenantContext.TenantId, request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Remove(Guid userId)
        => await _userService.RemoveUserAsync(_tenantContext.TenantId, userId) ? NoContent() : NotFound();
}
