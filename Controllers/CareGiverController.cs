using System.Text;
using AutoMapper;
using latest.Models;
using latest.Models.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Stripe;
using latest.Stripe;
using latest.Email;
using Microsoft.EntityFrameworkCore;
using latest.Models.Enums;

namespace latest.Controllers;

[ApiController]
[Route("care-giver")]
public class CareGiverController : ControllerBase {
    /// is het beter om na onboarden expired token terug te gaan naar getOnboardLink of naar /onboard ipv /confirm-caregiver in de mail moet /confirm-caregiver staan of hij moet opniew met stripe koppelen omdat de link niet kwam
    /// als het naar /onboard zou gaan moet je opniew koppelen met stripe omdat er een nieuwe account wordt aangemaakt
    private readonly UserManager<CareUser> _userManager;
    private readonly SignInManager<CareUser> _signInManager;
    private readonly IMapper _mapper;
    private readonly IEmailSender _emailSender;
    
    private readonly IStripeAppService _stripeAppService;

    private readonly MContext _context;

    public CareGiverController(
        UserManager<CareUser> userManager, 
        SignInManager<CareUser> signInManager, 
        IMapper  mapper, 
        IEmailSender emailSender, 
        IStripeAppService stripeAppService,
        MContext context) {
        _userManager =  userManager;
        _signInManager = signInManager;
        _mapper = mapper;
        _emailSender = emailSender;
        _stripeAppService = stripeAppService;
        _context = context;
    }
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] CareGiverDto cgd) {
        CareGiver cg = _mapper.Map<CareGiverDto, CareGiver>(cgd);
        IdentityResult ir = await _userManager.CreateAsync(cg, cgd.Password);
        if (ir.Succeeded) {
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(cg);
            _emailSender.SendEmail(
                new EmailMessage(new string[] {cg.Email!}, 
                "Bevestig uw E-mail", 
                string.Format("onboard/{0}/{1}", cg.Id, WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token))))
            );
            return Ok();
        } else return StatusCode(422, new { error = ir.Errors.Select(x => x.Code).First() });
        // } else return StatusCode(422, ir.Errors);
    }
    [HttpGet("{id}/{token}")]
    public async Task<IActionResult> GetOnboardLink(string id, string token) {
        CareGiver? cg = await _userManager.FindByIdAsync(id) as CareGiver;
        if (cg == null) return BadRequest();
        Account account = _stripeAppService.CreateExpressAccount(cg.Email!);
        cg.StripeId = account.Id;
        AccountLink accountLink = _stripeAppService.CreateAccountLink(account.Id, id, token);
        IdentityResult uir = await _userManager.UpdateAsync(cg);
        if (uir.Succeeded)  return Ok(accountLink.Url);
        return BadRequest(uir.Errors);
    }
    [HttpPost("{id}/{token}")]
    public async Task<IActionResult> Confirm(string id, string token) {
        CareGiver? cg = await _userManager.FindByIdAsync(id) as CareGiver;
        if (cg == null) return BadRequest();
        if (cg.StripeId == null) return BadRequest(new {
            Code = 0,
            Title = "Stripe gegevens zijn onbekend",
            Subtitle = "Probeer alstublieft opnieuw met stripe te koppelen door op de knop in uw bevestingsmail te klikken"
        });
        Account stripeAccount = _stripeAppService.GetAccount(cg.StripeId);
        if (!stripeAccount.PayoutsEnabled) {
            AccountLink accountLink = _stripeAppService.CreateAccountLink(stripeAccount.Id, id, token);
            return BadRequest(new {
                Code = 1,
                Title = "Stripe gegevens zijn incompleet",
                Subtitle = "Probeer alstublieft opnieuw met stripe te koppelen door op de knop hieronder te klikken",
                Url = accountLink.Url
            });   
        } 
        IdentityResult ir = await _userManager.ConfirmEmailAsync(cg, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)));
        if (ir.Succeeded) return Ok();
        return StatusCode(422, ir.Errors);
    }
    /// we need to recreate a confirmation token and send an email
    /// maby we need to know
    /// hij checked het token pas of ie confirmed is nadat stripe is gekoppeld
    /// dus daarom struren wij nu altijd een mail die gaat naar confirm-caregiver ipv onboard omdat wij anders het formulier overbodig vaak moeten invullen 
    /// somehow this endpoint could be spammed
    [HttpPost("resend/{id}")]
    public async Task<IActionResult> ResendConfirmationToḱen(string id) {
        CareGiver? cg = await _userManager.FindByIdAsync(id) as CareGiver;
        if (cg == null) return BadRequest();
        string token = await _userManager.GenerateEmailConfirmationTokenAsync(cg);
        _emailSender.SendEmail(
                new EmailMessage(new string[] {cg.Email!}, 
                "Bevestig uw E-mail", 
                string.Format("confirm-caregiver/{0}/{1}", cg.Id, WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token))))
            );
        return Ok();
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto ld) {
        CareGiver? cg = await _userManager.FindByEmailAsync(ld.Email) as CareGiver;  
        if (cg == null) return BadRequest("Ongeldige login gegevens of uw e-mail is niet bevestigd");
        Microsoft.AspNetCore.Identity.SignInResult sir = await _signInManager.PasswordSignInAsync(cg, ld.Password, false, true);
        if (sir.Succeeded) {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("SOME MAGIC UNICORNS GENERATE THIS SECRET");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, cg.Id.ToString()),
                    new Claim(ClaimTypes.Role, "caregiver")
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encryptedtoken = tokenHandler.WriteToken(token);
            return Ok(new {
                token = encryptedtoken,
                id = cg.Id
            });
        }
        return StatusCode(422, sir);
    }
    [HttpGet("profile")]
    [Authorize(Roles = "caregiver")]
    public async Task<IActionResult> GetProfile() {
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        string userId = ci!.FindFirst(ClaimTypes.Name)?.Value!; 
        CareGiver? cg = await _userManager.FindByIdAsync(userId) as CareGiver;
        if (cg == null) return Unauthorized();
        CareGiverProfileDto cgpd = _mapper.Map<CareGiver, CareGiverProfileDto>(cg);
        return Ok(cgpd);
    }
    [HttpPut]
    [Authorize(Roles = "caregiver")]
    public async Task<IActionResult> UpdateBio([FromBody] CareGiverBioDto cgbd) {
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        string userId = ci!.FindFirst(ClaimTypes.Name)?.Value!; 
        CareGiver? cg = await _userManager.FindByIdAsync(userId) as CareGiver;
        if (cg == null) return Unauthorized();
        cg.Biography = cgbd.Biography;
        IdentityResult ir = await _userManager.UpdateAsync(cg);
        if (ir.Succeeded) return Ok();
        return BadRequest(ir.Errors);
    }
    [HttpGet("choose-degree/{course}")]
    [Authorize(Roles = "careconsumer")]
    public async Task<IActionResult> GetAll(bool course) {
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        string userId = ci!.FindFirst(ClaimTypes.Name)?.Value!; 
        CareConsumer? cc = await _userManager.FindByIdAsync(userId) as CareConsumer;
        if (cc == null) return Unauthorized();
        List<CareGiver> lcg = 
        await _context.CareGivers
        .Include(cg => cg.Course)
        .Where(c => c.City.ToLower() == cc.City.ToLower() && course ? c.Course != null : c.Course == null)
        .Include(c => c.Course)
        .Include(cr => cr.Appointments.Where(a => a.PaymentRequest != null && a.Date < DateTime.Now).OrderBy(a => a.Date))
        .ThenInclude(a => a.Review)
        .Include(cr => cr.Appointments.Where(a => a.PaymentRequest != null && a.Date < DateTime.Now).OrderBy(a => a.Date))
        .ThenInclude(a => a.CareRequest)
        .ToListAsync();
        ChooseDto cd = new ChooseDto {
            City = cc.City,
            CareGivers = _mapper.Map<List<CareGiver>, List<CareGiverChooseDto>>(lcg, opt => opt.AfterMap((src, dest) => {
                for (int i = 0; i < src.Count; i++) {
                    List<int> ratings = new List<int>();
                    foreach (Appointment app in src[i].Appointments) {
                        if (app.Review != null) ratings.Add((int) app.Review.Stars);
                    }
                    int totalRating = 0;
                    foreach (int rating in ratings) {
                        totalRating += rating;
                    }
                    dest[i].AverageStars = (totalRating > 0) ? totalRating / ratings.Count : 0;
                }
            }))
        };
        return Ok(cd);
    }
    [HttpGet("choose-no-degree")]

    [HttpGet("fname/{id}")]
    [Authorize(Roles = "careconsumer")]
    public async Task<IActionResult> FName(string id) {
        CareGiver? cg = await _userManager.FindByIdAsync(id) as CareGiver;
        if (cg == null) return BadRequest();
        return Ok(cg.FirstName);
    }
}