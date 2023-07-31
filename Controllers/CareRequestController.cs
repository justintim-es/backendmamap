using System.Globalization;
using System.Security.Claims;
using AutoMapper;
using latest.Models;
using latest.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using latest.Models.Enums;

namespace latest.Controllers;
[ApiController]
[Route("care-request")]
public class CareRequestController : ControllerBase {
    private readonly IMapper _mapper;
    private readonly UserManager<CareUser> _userManager;
    private readonly MContext _context;
    public CareRequestController(IMapper mapper, UserManager<CareUser> userManager, MContext context) {
        _mapper = mapper;
        _userManager = userManager;
        _context = context;
    }
    [HttpPost]
    [Authorize(Roles = "careconsumer")]
    public async Task<IActionResult> Create([FromBody] CareRequestDto crd) {
        CareRequest cr = _mapper.Map<CareRequestDto, CareRequest>(crd);
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        string userId = ci!.FindFirst(ClaimTypes.Name)?.Value!; 
        cr.CareConsumerId = Guid.Parse(userId);
        await _context.CareRequests.AddAsync(cr);
        await _context.SaveChangesAsync();
        return Ok(cr.Id);
    }
    [HttpPost("{id}")]
    [Authorize(Roles = "careconsumer")]
    /// if its the first chatmessage you  create you want it to be visible but the default is already set to true of `
    public async Task<IActionResult> Invite(int id, [FromBody] string[] ids) {
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        string userId = ci!.FindFirst(ClaimTypes.Name)?.Value!; 
        foreach (string cgid in ids) {
            Guid careGiverId = Guid.Parse(cgid);
            CareGiverCareRequest cgcr = new CareGiverCareRequest {
                CareGiverId = careGiverId,
                CareRequestId = id
            };
            await _context.CareGiverCareRequests.AddAsync(cgcr);
            ChatMessage cm = new ChatMessage {
                CareRequestId = id,
                SenderId = Guid.Parse(userId),
                RecieverId = careGiverId,
                ChatMessageButtonEnum = ChatMessageButtonEnum.ToOffer
            };
            await _context.ChatMessages.AddAsync(cm);
            Appointment a = new Appointment {
                CareRequestId = id,
                CareGiverId = careGiverId
            };
            await _context.Appointments.AddAsync(a);
        }
        await _context.SaveChangesAsync();
        return Ok();
    }
    [HttpGet]
    [Authorize(Roles = "caregiver")]
    public async Task<IActionResult> Invitations() {
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        string userId = ci!.FindFirst(ClaimTypes.Name)?.Value!;        
        List<CareGiverCareRequest> lcgcr = await _context.CareGiverCareRequests.Where(cgcr => cgcr.CareGiverId == Guid.Parse(userId)).Include(cgcr => cgcr.CareRequest).ToListAsync();
        return Ok(lcgcr);
    }
    [HttpGet("{id}/{appId}/{user}")]
    [Authorize]
    public async Task<IActionResult> Get(int id, int appId, Guid user) {
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        Guid userId = Guid.Parse(ci!.FindFirst(ClaimTypes.Name)?.Value!);
        CareRequest cr = 
        await _context.CareRequests
        .Include(cr => cr.CareGiverCareRequests)
        // .Include(cr => cr.Appointments.Where(a => a.CareGiverId == user || a.CareGiverId == userId))
        // .SingleAsync(cr => cr.Id == id && (cr.CareConsumerId == userId || cr.CareGiverCareRequests.Select(x => x.CareGiverId).Contains(userId)))
        .SingleAsync(cr => cr.Id == id && (cr.CareConsumerId == userId || cr.CareGiverCareRequests.Select(x => x.CareGiverId).Contains(userId)));
        Appointment app = await _context.Appointments.SingleAsync(a => a.Id == appId);
        CareRequestOneAppointmentDto crd = _mapper.Map<CareRequest, CareRequestOneAppointmentDto>(cr, opt => opt.AfterMap((src, dest) => {
            dest.Appointment = _mapper.Map<Appointment, AppointmentDto>(app);
        }));
        return Ok(crd);   
    }
    [HttpPut("{careid}/{appid}/{to}")]
    [Authorize]
    public async Task<IActionResult> Offer(int careid, int appid, Guid to, [FromBody] CareRequestOfferDto crod) {
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        Guid userId = Guid.Parse(ci!.FindFirst(ClaimTypes.Name)?.Value!);
        CareRequest cr = 
        await _context.CareRequests
        .Include(cr => cr.CareGiverCareRequests)
        .Include(cr => cr.Appointments)
        .SingleAsync(cr => cr.Id == careid &&  (cr.CareConsumerId == userId || cr.CareGiverCareRequests.Select(x => x.CareGiverId).Contains(userId)));
        Appointment appToAdjust = cr.Appointments.Single(a => a.Id == appid);
        if (crod.Date != null) {
            DateTime toAddTime = (DateTime) crod.Date!;
            if (crod.Time != null) {
                DateTime withTime = toAddTime.Add(new TimeSpan(Int32.Parse(crod.Time!.Split(':')[0]) + 2, Int32.Parse(crod.Time.Split(':')[1]), 0)); 
                appToAdjust.Date = withTime;
                appToAdjust.Time = crod.Time;
            } else appToAdjust.Date = toAddTime;
        } 
        if (crod.Compensation != null) {
            appToAdjust.Compensation = crod.Compensation;
        }
        _context.Appointments.Update(appToAdjust);
        /// would it crash here because of the null check
        /// could we do something in the frontend like disable time for as long that ther aint no date time only
        // if (crod.Time != null) {
            // appToAdjust.Compensation = crod.Compensation;
            // appToAdjust.Time = crod.Time;
            // _context.Appointments.Update(appToAdjust);
        // } else {
        //     /// aint important yet we found different issue
        // }
    
        /// should render Tile will be true but all previous msgs with the same carerequest id should be removed
        List<ChatMessage> previous = 
        await _context.ChatMessages
        .Where(cm => 
            cm.CareRequestId == cr.Id && 
            ((cm.SenderId == to && cm.RecieverId == userId) || (cm.SenderId == userId && cm.RecieverId == to))
        ).ToListAsync();
        foreach (ChatMessage pcm in previous) {
            //// shoudl render tile is already set to false
            pcm.ShouldRenderTile = false;
            /// ithinknit aint important
            // pcm.ChatMessageButtonEnum = ChatMessageButtonEnum.Nothing;
            _context.Update(pcm);
            /// does this function behaves properly both sides it can be the careconsumer or caregiver
        }
        // but whanever we do we have a new chat message so the message shoves over to the other to accept it
        // but again does accept work both sides i dont think
        // because somehow we rendered betaal on the caregiver instead of the careconsumer
        /// we only want to have a chatmessage with toaccept if all appointment data fields arent null or are their
        if (appToAdjust.Date == null || appToAdjust.Compensation == null || appToAdjust.Time == null) {
            ChatMessage icm = new ChatMessage {
                CareRequest = cr,
                SenderId = userId,
                RecieverId = to,
                ChatMessageButtonEnum = ChatMessageButtonEnum.ToOffer
            };
            await _context.ChatMessages.AddAsync(icm);
        } else {
            ChatMessage cm = new ChatMessage {
                CareRequest = cr,
                SenderId = userId,
                RecieverId = to,
                ChatMessageButtonEnum = ChatMessageButtonEnum.ToAccept
            };
            await _context.ChatMessages.AddAsync(cm);
        };
        await _context.SaveChangesAsync();
        return Ok();    
    }
    // [HttpPost("caregiver/{appId}")]
    // [Authorize(Roles = "caregiver")]
    // public async Task<IActionResult> CareGiverAccept(int appId) {
        
    //     return Ok();
    // }
    // [HttpPost("careconsumer/{appId}")]
    // [Authorize(Roles = "careconsumer")]
    // public async Task<IActionResult> CareConsumerAccept(int appId) {
    //     return Ok()

    // }

}