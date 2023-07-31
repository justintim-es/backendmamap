
using System.Security.Claims;
using latest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace latest.Controllers;

[ApiController]
[Route("profile-image")]
public class ProfileImageController : ControllerBase {
    private readonly UserManager<CareUser> _userManager;
    public ProfileImageController(UserManager<CareUser> userManager) {
        _userManager = userManager;
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Upload(IFormFile file) {
        Console.WriteLine("gothere");
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        string userId = ci!.FindFirst(ClaimTypes.Name)?.Value!; 
        CareUser? cu = await _userManager.FindByIdAsync(userId);
        if (cu == null) return BadRequest();
        if (file.Length > 0) {
            using (var ms = new MemoryStream()) {
                file.CopyTo(ms);
                cu.ProfileImage = ms.ToArray();
                await _userManager.UpdateAsync(cu);
                return Ok();
            }
        }
        return BadRequest();
    }
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> HasImage() {
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        string userId = ci!.FindFirst(ClaimTypes.Name)?.Value!; 
        CareUser? cu = await _userManager.FindByIdAsync(userId) as CareUser;
        if (cu == null) return Ok(false);
        if (cu.ProfileImage == null) return Ok(false);
        return Ok(true);
    }
    [HttpGet("has-opponent-image/{id}")]
    [Authorize]
    public async Task<IActionResult> HasOpponentImage(string id) {
        CareUser? cu = await _userManager.FindByIdAsync(id) as CareUser;
        if (cu == null) return Ok(false);
        if (cu.ProfileImage == null) return Ok(false);
        return Ok(true);
    }
    [HttpGet("img/{random}/{id}")]
    public async Task<IActionResult> Display(string random, string id) {
        CareUser? cu = await _userManager.FindByIdAsync(id);
        if (cu == null) return BadRequest();
        return File(cu.ProfileImage!, "image/jpg");
    }
}