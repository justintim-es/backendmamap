using AutoMapper;
using latest.Models;
using latest.Models.Dtos;
using latest.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace latest.Controllers;

[ApiController]
[Route("review")]
public class ReviewController : ControllerBase {
    private readonly MContext _context;
    private readonly IMapper _mapper;
    public ReviewController(MContext context, IMapper mapper)
    {   
        _context = context;
        _mapper = mapper;
    }
    [HttpPost("{appid}/{cmid}")]
    [Authorize(Roles = "careconsumer")]
    public async Task<IActionResult> Review(int appid, int cmid, [FromBody] ReviewDto rd) {
        Appointment app = await _context.Appointments.SingleAsync(a => a.Id == appid);
        if (app.Date >= DateTime.Now) return StatusCode(405, "Afspraak heeft nog niet plaatsgevonden");
        Review r = _mapper.Map<ReviewDto, Review>(rd);
        app.Review = r;
        _context.Appointments.Update(app);
        ChatMessage cm = await _context.ChatMessages.SingleAsync(cm => cm.Id == cmid);
        cm.ChatMessageButtonEnum = ChatMessageButtonEnum.Reviewed;
        _context.ChatMessages.Update(cm);
        await _context.SaveChangesAsync();
        return Ok();
    }

}
