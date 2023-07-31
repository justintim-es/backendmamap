
using System.Security.Claims;
using latest.Models;
using latest.Stripe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using latest.Services;
using latest.Models.Enums;

namespace latest.Controllers;
[ApiController]
[Route("payment-request")]
public class PaymentRequestController : ControllerBase {
    private readonly MContext _context;
    private readonly IStripeAppService _stripe;
    private readonly UserManager<CareUser> _userManager;

    public PaymentRequestController(MContext context, IStripeAppService stripe, UserManager<CareUser> userManager) 
    {
        _context = context;
        _stripe = stripe;
        _userManager = userManager;
    }
    [HttpPost("{prid}/{cmid}")]
    public async Task<IActionResult> Payed(string prid, int cmid) {
        PaymentRequest pr = await _context.PaymentRequests.SingleAsync(pr => pr.Id == prid);
        Session session = _stripe.GetSession(pr.SessionId);
        if (session.PaymentStatus == "paid") {
            pr.IsPayed = true;
            _context.PaymentRequests.Update(pr);
            ChatMessage cm = await _context.ChatMessages.SingleAsync(cm => cm.Id == cmid);
            cm.ChatMessageButtonEnum = ChatMessageButtonEnum.ToReview;
            _context.ChatMessages.Update(cm);
            await _context.SaveChangesAsync();
            return Ok();
        } else return BadRequest("Betaling mislukt");
    }
    /// and here we have to make sure both sides works correctly
    /// somehow we rendered a betaal button on the wrong side of the story
    /// if its the careconsumer we update a chat message and if we are the caregiver we add a new chat message
    /// should this new chatmessage for the caregiver disable the tile of previous chat messag if adjust a chat message not if create a new one we should
    [HttpPost("{to}/{appointmentId}/{cmid}")]
    [Authorize]
    public async Task<IActionResult> Accept(Guid to, int appointmentId, int cmid) {
        Appointment appointment = 
        await _context.Appointments
        .Include(a => a.CareGiver)
        .Include(a => a.CareRequest)
        .ThenInclude(cr => cr.Appointments)
        .ThenInclude(a => a.PaymentRequest)
        // .Include(a => a.PaymentRequest)
        .SingleAsync(a => a.Id == appointmentId);
        if (appointment.PaymentRequest != null) return BadRequest(appointment.PaymentRequest);        
        string tpmid = Raschan.RandomString(16);       

        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        string userId = ci!.FindFirst(ClaimTypes.Name)?.Value!; 
        CareUser? cu = await _userManager.FindByIdAsync(userId);
        if (cu == null) return Unauthorized();
        else if (cu is CareConsumer) {
            ChatMessage cm = await _context.ChatMessages.Include(cm => cm.CareRequest).ThenInclude(cr => cr!.Appointments).SingleAsync(cm => cm.Id == cmid);
            Session session = _stripe.CreatePaymentRequest((int) appointment.Compensation!, appointment.CareGiver.StripeId, tpmid, cmid);
            if (cm.CareRequest == null) return BadRequest();
            foreach(Appointment a in cm.CareRequest.Appointments.Where(a => a.Id != appointmentId && a.PaymentRequest == null)) {
                a.IsOccupied = true;
            }
            _context.Appointments.UpdateRange(cm.CareRequest.Appointments);
            Appointment aa = cm.CareRequest.Appointments.Single(a => a.Id == appointmentId);
            cm.ChatMessageButtonEnum = ChatMessageButtonEnum.ToPay;
            cm.AlwaysRenderTile = true;
            _context.ChatMessages.Update(cm);
            aa.PaymentRequest = new PaymentRequest {
                Id = tpmid,
                Url = session.Url,
                Amount = (int) appointment.Compensation,
                SessionId = session.Id
            };
            _context.Appointments.Update(aa);
            await _context.SaveChangesAsync();
            return Ok();
        } else {
            CareRequest cr = appointment.CareRequest;
            List<ChatMessage> lcm = 
            await _context.ChatMessages.Where(
                cm => cm.CareRequestId == cr.Id && 
                ((cm.SenderId == Guid.Parse(userId) && cm.RecieverId == to) || (cm.SenderId == to && cm.RecieverId == Guid.Parse(userId)))
            ).ToListAsync();
            foreach (ChatMessage clcm in lcm) {
                clcm.ShouldRenderTile = false;
                _context.Update(clcm);
            }
            ChatMessage ncm = new ChatMessage {
                CareRequestId = cr.Id,
                SenderId = Guid.Parse(userId),
                RecieverId = to,
                ChatMessageButtonEnum = ChatMessageButtonEnum.ToPay,
                AlwaysRenderTile = true
            };
            await _context.ChatMessages.AddAsync(ncm);
            await _context.SaveChangesAsync();
            Session session = _stripe.CreatePaymentRequest((int) appointment.Compensation!, appointment.CareGiver.StripeId, tpmid, ncm.Id);
            Appointment app = cr.Appointments.Single(a => a.Id == appointmentId);
            app.PaymentRequest = new PaymentRequest {   
                Id = tpmid,
                Url = session.Url,
                Amount = (int) appointment.Compensation,
                SessionId = session.Id
            };
            _context.Appointments.Update(app);
            foreach (Appointment a in cr.Appointments.Where(a => a.Id != appointmentId && a.PaymentRequest == null)) {
                a.IsOccupied = true;
            }
            _context.Appointments.UpdateRange(cr.Appointments);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}