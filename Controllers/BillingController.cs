using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSLite.DTOs;
using WMSLite.Services;

namespace WMSLite.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;

    public BillingController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpGet("status")]
    public async Task<ActionResult> Status()
    {
        var tenantId = HttpContext.Items["TenantId"] as string;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return Unauthorized();
        }

        var sub = await _billingService.GetByTenantIdAsync(tenantId);
        if (sub is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            sub.TenantId,
            sub.TrialEndsAtUtc,
            sub.IsPaidActive,
            sub.SeatsPurchased,
            sub.PaidEndsAtUtc,
            WriteAllowed = sub.IsWriteAccessAllowed(DateTime.UtcNow)
        });
    }

    [HttpPost("subscribe")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Subscribe([FromBody] SubscribeRequest request)
    {
        var tenantId = HttpContext.Items["TenantId"] as string;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return Unauthorized();
        }

        if (request.Seats < 1 || request.DurationDays < 1)
        {
            return BadRequest(new { message = "Seats and duration must be positive." });
        }

        var sub = await _billingService.SubscribeAsync(tenantId, request.Seats, request.DurationDays);
        return Ok(sub);
    }
}
