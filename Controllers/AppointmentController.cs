
using System.Security.Claims;
using AutoMapper;
using latest.Models;
using latest.Models.Dtos;
using latest.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace latest.Controllers;
[ApiController]
[Route("[controller]")]

public class AppointmentController : ControllerBase {
    private readonly UserManager<CareUser> _userManager;
    private readonly MContext _context;
    private readonly IMapper _mapper;
    public AppointmentController(UserManager<CareUser> userManager, MContext context, IMapper mapper)
    {
        _userManager = userManager;
        _context = context;
        _mapper = mapper;
    }
    /// do we need a careid or a appid incase of repeat the app id has the careid too
    /// so to write shorter code we only want the appid
    
    // [HttpDelete("{appid}")]
    // [Authorize(Roles = "careconsumer")]
    // public async Task<IActionResult> Delete(int appid) {
    //     Appointment app = await _context.Appointments.SingleAsync(a => a.Id == appid);
    //     _context.Appointments.Remove(app);
    //     return Ok();
    // }
    [Authorize(Roles = "careconsumer")]
    [HttpPost("{appid}/{forto}")]
    public async Task<IActionResult> Repeat(int appid, Guid forto, [FromBody] CareRequestOfferDto crod) {
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        string userId = ci!.FindFirst(ClaimTypes.Name)?.Value!; 
        Appointment appointment = await _context.Appointments.SingleAsync(a => a.Id == appid);
        // DateTime nTime = (DateTime) crod.Date!;
        // DateTime wTime = nTime.Add(new TimeSpan(Int32.Parse(crod.Time!.Split(':')[0]) + 2, Int32.Parse(crod.Time.Split(':')[1]), 0));
        ChatMessageButtonEnum cmbe = ChatMessageButtonEnum.Nothing;
        if (crod.Date != null) {
            DateTime toAddTime = (DateTime) crod.Date!;
            if (crod.Time != null) {
                DateTime withTime = toAddTime.Add(new TimeSpan(Int32.Parse(crod.Time!.Split(':')[0]) + 2, Int32.Parse(crod.Time.Split(':')[1]), 0)); 
                appointment.Date = withTime;
                appointment.Time = crod.Time;
            } else appointment.Date = toAddTime;
        } 
        if (crod.Compensation != null) {
            appointment.Compensation = crod.Compensation;
        }
        appointment.ShouldDelete = false;
        _context.Appointments.Update(appointment);
        /// we checked crod.date but actually we should check the appointment his value
        if (appointment.Date == null || appointment.Time == null || appointment.Compensation == null) cmbe = ChatMessageButtonEnum.ToOffer;
        else cmbe = ChatMessageButtonEnum.ToAccept;
        ChatMessage nChatMessage = new ChatMessage {
            CareRequestId = appointment.CareRequestId,
            SenderId = Guid.Parse(userId),
            RecieverId = forto,
            ChatMessageButtonEnum = cmbe
        };
        await _context.ChatMessages.AddAsync(nChatMessage);
        await _context.SaveChangesAsync();
        return Ok();
    }
    [Authorize]
    [HttpPost("prepare/{crid}/{cgid}")]
    public async Task<IActionResult> PrepareRepeat(int crid, Guid cgid) {
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        Guid userId = Guid.Parse(ci!.FindFirst(ClaimTypes.Name)?.Value!);
        List<Appointment> apps = await _context.Appointments.Where(
        a => a.CareGiverId == cgid && a.ShouldDelete == true && a.CareRequestId == crid).ToListAsync();
        _context.RemoveRange(apps);
        Appointment app = new Appointment {
            CareRequestId = crid,
            CareGiverId = cgid,
            ShouldDelete = true
        };
        await _context.Appointments.AddAsync(app);
        await _context.SaveChangesAsync();
        return Ok(app.Id);
    }

    [Authorize(Roles = "careconsumer")]
    [HttpGet("planned")]
    public async Task<IActionResult> Planned() {
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        string userId = ci!.FindFirst(ClaimTypes.Name)?.Value!; 
        CareUser? cu = await _userManager.FindByIdAsync(userId);
        List<Appointment> apps = 
        await _context.Appointments
        .Include(a => a.CareRequest)
        .Include(a => a.CareGiver)
        .OrderBy(a => a.Date)
        .Where(a => a.CareRequest.CareConsumerId == Guid.Parse(userId) && a.PaymentRequest != null && a.Date >= DateTime.Now).ToListAsync();
        List<AppointmentDto> appds = _mapper.Map<List<Appointment>, List<AppointmentDto>>(apps);
        return Ok(appds);
    }
    [Authorize(Roles = "careconsumer")]
    [HttpGet("passed")]
    public async Task<IActionResult> Passed() {
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        string userId = ci!.FindFirst(ClaimTypes.Name)?.Value!; 
        CareUser? cu = await _userManager.FindByIdAsync(userId);
        List<Appointment> apps = 
        await _context.Appointments
        .Include(a => a.CareRequest)
        .Include(a => a.CareGiver)
        .OrderBy(a => a.Date)
        .Where(a => a.CareRequest.CareConsumerId == Guid.Parse(userId) && a.PaymentRequest != null && a.Date < DateTime.Now).ToListAsync();
        List<AppointmentDto> appds = _mapper.Map<List<Appointment>, List<AppointmentDto>>(apps);
        return Ok(appds);
    }
}