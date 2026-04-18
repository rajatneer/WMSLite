using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSLite.DTOs;
using WMSLite.Models;
using WMSLite.Repositories;

namespace WMSLite.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IJsonRepository<AppUser> _userRepository;
    private readonly IJsonRepository<Subscription> _subscriptionRepository;

    public UsersController(IJsonRepository<AppUser> userRepository, IJsonRepository<Subscription> subscriptionRepository)
    {
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    [HttpPost]
    public async Task<ActionResult> Add([FromBody] CreateUserRequest request)
    {
        var tenantId = HttpContext.Items["TenantId"] as string;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return Unauthorized();
        }

        var subscription = (await _subscriptionRepository.GetAllAsync()).First(x => x.TenantId == tenantId);
        var users = (await _userRepository.GetAllAsync()).Where(x => x.TenantId == tenantId).ToList();

        var trialEnded = DateTime.UtcNow > subscription.TrialEndsAtUtc;
        if (trialEnded && subscription.IsPaidActive && users.Count >= subscription.SeatsPurchased)
        {
            return Conflict(new { message = "Seats exceeded. Increase seats in billing." });
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid().ToString("N"),
            TenantId = tenantId,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Password))),
            Role = request.Role is "Admin" ? "Admin" : "User",
            CreatedAtUtc = DateTime.UtcNow
        };

        await _userRepository.InsertAsync(user);
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Remove(string id)
    {
        var tenantId = HttpContext.Items["TenantId"] as string;
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null || user.TenantId != tenantId)
        {
            return NotFound();
        }

        await _userRepository.DeleteAsync(id);
        return NoContent();
    }
}
