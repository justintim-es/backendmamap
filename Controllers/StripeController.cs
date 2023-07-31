
using System.Security.Claims;
using latest.Stripe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace latest.Models;
[ApiController]
[Route("stripe")]

[Authorize(Roles = "caregiver")]
public class StripeController : ControllerBase {
    private readonly IStripeAppService _stripeAppService;
    private readonly UserManager<CareUser> _userManager;
    public StripeController(IStripeAppService stripeAppService, UserManager<CareUser> userManager) {
        _stripeAppService = stripeAppService;
        _userManager = userManager;
    }
    // [HttpGet]
    // public async Task<IActionResult> GetConnected() {
    //     StripeList<Account> accounts = _stripeAppService.GetConnectedAccounts();
    //     return Ok(accounts);
    // }
}