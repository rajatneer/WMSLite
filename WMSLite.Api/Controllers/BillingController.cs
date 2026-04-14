using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSLite.Api.DTOs;
using WMSLite.Api.Services;

namespace WMSLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;
    private readonly ITenantContext _tenantContext;

    public BillingController(IBillingService billingService, ITenantContext tenantContext)
    {
        _billingService = billingService;
        _tenantContext = tenantContext;
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status()
        => Ok(await _billingService.GetStatusAsync(_tenantContext.TenantId));

    [HttpPost("subscribe")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
        => Ok(await _billingService.SubscribeAsync(_tenantContext.TenantId, request));
}
