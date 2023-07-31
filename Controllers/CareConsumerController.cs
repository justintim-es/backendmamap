
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using latest.Models;
using latest.Models.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using latest.Stripe;
using Stripe.Checkout;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using latest.Email;

namespace latest.Controllers;
[ApiController]
[Route("care-consumer")]
public class CareConsumerController : ControllerBase {
    private readonly UserManager<CareUser> _userManager;
    private readonly IMapper _mapper;
    private readonly IEmailSender _emailSender;

    private readonly IStripeAppService _stripeAppService;

    private readonly SignInManager<CareUser> _signInManager;

    public CareConsumerController(
        UserManager<CareUser> userManager, 
        IMapper mapper, 
        IEmailSender emailSender, 
        IStripeAppService stripeAppService,
        SignInManager<CareUser> signInManager) {
        _userManager = userManager;
        _mapper = mapper;
        _emailSender = emailSender;
        _stripeAppService = stripeAppService;
        _signInManager = signInManager;
    }
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] CareConsumerDto ccd) {
        CareConsumer cc = _mapper.Map<CareConsumerDto, CareConsumer>(ccd);
        IdentityResult ir = await _userManager.CreateAsync(cc, ccd.Password);
        if (ir.Succeeded) {
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(cc);
            _emailSender.SendEmail(
                new EmailMessage(new string[] {cc.Email!}, 
                "Bevestig uw E-mail", 
                string.Format("register-pay/{0}/{1}", cc.Id, WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token))))
            );
            return Ok();
        } else return StatusCode(422, new { error = ir.Errors.Select(x => x.Code).First() }); 
    }
    [HttpGet("{id}/{token}")]
    public async Task<IActionResult> PaymentLink(string id, string token) {
        CareConsumer? cc = await _userManager.FindByIdAsync(id) as CareConsumer;
        if (cc == null) return BadRequest();
        Session session = _stripeAppService.CreateAccountPayment(id, token); 
        cc.SessionId = session.Id;
        IdentityResult ir = await _userManager.UpdateAsync(cc);
        if (ir.Succeeded) return Ok(session.Url);
        return BadRequest(ir.Errors);
    }
    [HttpPost("{id}/{token}")]
    public async Task<IActionResult> Confirm(string id, string token) {
        CareConsumer? cc = await _userManager.FindByIdAsync(id) as CareConsumer;
        if (cc == null) return BadRequest();
        Session session = _stripeAppService.GetSession(cc.SessionId);
        Console.WriteLine(session.Status);
        if (session.Status == "complete") {
            IdentityResult ir = await _userManager.ConfirmEmailAsync(cc, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)));
            if (ir.Succeeded) return Ok();
            return StatusCode(422, ir.Errors);
        } 
        return BadRequest(new {
            Title = "U heeft nog niet betaald",
            Subtitle = "Klik op de knop hieronder om het opnieuw te proberen",
            Url = session.Url
        });
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto ld) {
        CareConsumer? cg = await _userManager.FindByEmailAsync(ld.Email) as CareConsumer;  
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
                    new Claim(ClaimTypes.Role, "careconsumer")
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
}