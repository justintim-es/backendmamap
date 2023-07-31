using System.Security.Claims;
using AutoMapper;
using latest.Models;
using latest.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace latest.Controllers;
[ApiController]
[Route("chat-message")]
[Authorize]
public class ChatMessageController : ControllerBase {
    private readonly MContext _context;
    private readonly IMapper _mapper;
    public ChatMessageController(MContext context, IMapper mapper) {
        _context = context;
        _mapper = mapper;
    }
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ChatMessageCreateDto cmcd) {
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        Guid userId = Guid.Parse(ci!.FindFirst(ClaimTypes.Name)?.Value!); 
        ChatMessage cm = _mapper.Map<ChatMessageCreateDto, ChatMessage>(cmcd, 
        opt => opt.AfterMap((src, dest) => dest.SenderId = userId));
        await _context.AddAsync(cm);
        await _context.SaveChangesAsync();
        return Ok();
    }
    [HttpGet("conversation/{to}")]
    /// the propper appointment would be the index of the how manied chatmessages that reference the same carerequestid
    /// the appointment is grabbed too many times it can only be used once so before we dequeue we have to consider do we display it?
    /// maby if we show the appointment only once to the proper chatmessage because chatmessages dissapear are not always rendered with isrendertile
    /// so we dequeue less only when nesassary
    public async Task<IActionResult> Conversation(Guid to) {
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        Guid userId = Guid.Parse(ci!.FindFirst(ClaimTypes.Name)?.Value!); 
        List<ChatMessage> lcm = 
        await _context.ChatMessages
        .Where(cm => (cm.SenderId == to && cm.RecieverId == userId) || (cm.RecieverId == to && cm.SenderId == userId))
        .Include(cm => cm.Sender)
        .Include(cm => cm.Reciever)
        .Include(cm => cm.CareRequest)
        .ThenInclude(cr => cr!.Appointments.Where(a => a.CareGiverId == userId || a.CareGiverId == to).OrderBy(a => a.Added))
        .ThenInclude(a => a.PaymentRequest)
        .Include(cm => cm.CareRequest)
        .ThenInclude(cr => cr!.Appointments.Where(a => a.CareGiverId == userId || a.CareGiverId == to).OrderBy(a => a.Added))
        .ThenInclude(a => a.Review)
        .OrderBy(cm => cm.Date)
        .ToListAsync();
        // var howmcra = lcm.GroupBy(cm => (int) cm.CareRequestId!).ToDictionary(crid => crid.Key, c => c.Count());
        /// foreach index i can find the same list of appointments
        /// the  second time it call dequeue its the same appointment as of the previous list appointments
        /// thats why dequeue gives us the same appointment
        Dictionary<int, Queue<Appointment>> dic = new Dictionary<int, Queue<Appointment>>();
        foreach (ChatMessage cm in lcm) {
            if (!dic.ContainsKey((int) cm.CareRequestId!)) dic.Add((int) cm.CareRequestId, new Queue<Appointment>(cm.CareRequest!.Appointments));
        }
        // var apps = lcm.Select(cm => new {
        //   CRID = (int) cm.CareRequestId!,
        //   Appointments = new Queue<Appointment>(cm.CareRequest!.Appointments)
        // }).Distinct().ToDictionary(t => t.CRID, t => t.Appointments);
        /// eigenlijk willen wij geen distinct gebruiken maar dequeue vaker aanroepen
        // List<int> appointmentIds  = new List<int>();
        // List<List<Appointment>> apps = lcm.Select(cm => cm.CareRequest!.Appointments.ToList()).ToList();
        List<ChatMessageDto> lcmd = _mapper.Map<List<ChatMessage>, List<ChatMessageDto>>(
            lcm,
            opt => opt.AfterMap((src, dest) => {
                for (int i = 0; i < dest.Count; i++) {
                    Console.WriteLine("aftermap");
                    Console.WriteLine("INDEX {0}", i);
                    dest[i].AmISender = dest[i].SenderId == userId;
                    if (dest[i].CareRequest != null) {
                        dest[i].CareRequest!.isCareConsumer = dest[i].CareRequest!.CareConsumerId == userId;
                        // Appointment? app = new Queue<Appointment>(appointmentss[i].Where(a => !appointmentIds.Contains(a.Id))).Dequeue();
                        // appointmentIds.Add(app.Id);
                        // something howmcra minus the index
                        // dest[i].CareRequest!.Appointment = _mapper.Map<Appointment, AppointmentDto>(appointmentss[i].Dequeue());
                        // if (howmcra[(int) dest[i].CareRequestId!] > 1) dest[i].CareRequest!.Appointment = _mapper.Map<Appointment, AppointmentDto>(apps[i][0]);
                        ///how can we reference the same list if the crid is the same  
                        /// op het moemnt dat we dequeue doen moeten we voor de volgende keer dezelfde lijst gebruiken
                        /// voor de volgende index van i willen wij dezelfde appointment lijst gebruiken die gedequeued is
                        /// dus als de carerequest id voor de tweede keer voorkomt willen wij de eerste lijst gebruiken
                        /// of kunnen wij distinct gebruiken 
                        /// als wij deqeue konden gebruiken voor de lijst erna of voor de laatste lijst
                        /// stel je voor als apps[i] geen duplicate list heeft dan krijgen we idnex out of range error
                        if (dest[i].ShouldRenderTile || dest[i].AlwaysRenderTile) {
                            dest[i].CareRequest!.Appointment = 
                            _mapper.Map<Appointment, AppointmentDto>(dic[(int) src[i].CareRequestId!].Dequeue());
                        }
                    }
                }
            })
        );
        // List<ChatMessageDto> lcmd = _mapper.Map<List<ChatMessage>, List<ChatMessageDto>>(
        //     lcm,
        //     opt => opt.AfterMap((src, dest) => dest.ForEach(d => {
        //         d.AmISender = d.SenderId == userId;
        //         if (d.CareRequest != null) d.CareRequest.isCareConsumer = d.CareRequest.CareConsumerId == userId;
        //         // d.ShouldButtonDisplay = 
        //         // src.ForEach(s => {
        //         //     d.CareRequest.Appointment = _mapper.Map<Appointment, AppointmentDto>(s.CareRequest!.Appointments.ToList()[howmcra[(int) s.CareRequestId!-1]]);
        //         // });
                
        //     }))
        // );
        // List<ChatMessageDto> lcmd = _mapper.Map<List<ChatMessage>, List<ChatMessageDto>>(
        //     lcm,
        //     opt => opt.AfterMap((src, dest) => {
        //         Dictionary<int, int> crlogic = new Dictionary<int, int>();
                
        //         for (int i = 0; i < dest.Count; i++) {
        //             dest[i].AmISender = dest[i].SenderId == userId;
        //             if (dest[i].CareRequest != null) dest[i].CareRequest!.isCareConsumer = dest[i].CareRequest!.CareConsumerId == userId;
        //             Console.WriteLine("hmmmm");
        //             Console.WriteLine(i);
        //             Console.WriteLine(crlogic.GetValueOrDefault((int)src[i].CareRequestId!));
        //             foreach (Appointment app in src[i].CareRequest!.Appointments) {
        //                 Console.WriteLine("found appointment");
        //                 Console.WriteLine(app.Id);
        //             }
        //             dest[i].CareRequest!.Appointment = _mapper.Map<Appointment, AppointmentDto>(src[i].CareRequest!.Appointments.ToList()[crlogic.GetValueOrDefault((int)src[i].CareRequestId!)]);
        //             crlogic[(int) src[i].CareRequestId!] = crlogic.GetValueOrDefault((int)src[i].CareRequestId!)+1;
        //         }
        //     })
        // );
        return Ok(lcmd);
    }
    [HttpPut("read-conversation/{to}")]
    public async Task<IActionResult> ReadConversation(Guid to) {
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        Guid userId = Guid.Parse(ci!.FindFirst(ClaimTypes.Name)?.Value!); 
        List<ChatMessage> lcm = 
        await _context.ChatMessages
        .Where(cm => cm.SenderId == to && cm.RecieverId == userId)
        .ToListAsync();
        lcm.ForEach(cm => cm.Seen = true);
        _context.ChatMessages.UpdateRange(lcm);
        await _context.SaveChangesAsync();
        return Ok();
    }
    [HttpGet("unseen")]
    public IActionResult Unseen() {
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        Guid userId = Guid.Parse(ci!.FindFirst(ClaimTypes.Name)?.Value!); 
        return Ok(_context.ChatMessages.Where(cm => cm.RecieverId == userId && !cm.Seen).Count());
    }
    [HttpGet("conversations")]
    public async Task<IActionResult> Conversations() {
        ClaimsIdentity? ci = this.User.Identity as ClaimsIdentity;
        Guid userId = Guid.Parse(ci!.FindFirst(ClaimTypes.Name)?.Value!); 
        List<ChatMessage> lcm = 
        await _context.ChatMessages.Where(cm => cm.RecieverId == userId || cm.SenderId == userId)
        .Include(cm => cm.CareRequest)
        .ThenInclude(cr => cr!.Appointments)
        .Include(cm => cm.Sender)
        .Include(cm => cm.Reciever)
        .OrderByDescending(cm => cm.Date)
        .ToListAsync(); 
        List<ChatMessage> lcmr = lcm.Where(cm => cm.RecieverId == userId).ToList();
        List<ChatMessage> lcms = lcm.Where(cm => cm.SenderId == userId).ToList();
        List<ChatMessageStranger> lcmrSenders = new List<ChatMessageStranger>();
        List<ChatMessageStranger> lcmsRecievers = new List<ChatMessageStranger>();
        lcmrSenders.AddRange(lcmr.Select(x => new ChatMessageStranger { StrangerId = x.SenderId, Date = x.Date }).Distinct());
        lcmsRecievers.AddRange(lcms.Select(cm => new ChatMessageStranger { StrangerId = cm.RecieverId, Date = cm.Date }).Distinct());
        List<ChatMessageStranger> strangers = new List<ChatMessageStranger>();
        strangers.AddRange(lcmrSenders);
        strangers.AddRange(lcmsRecievers);
        List<List<ChatMessage>> conversations = new List<List<ChatMessage>>();
        foreach (Guid stranger in strangers.OrderByDescending(s => s.Date).Select(s => s.StrangerId).Distinct()) {
            conversations.Add(lcm.Where(cm => cm.RecieverId == stranger || cm.SenderId == stranger).OrderBy(cm => cm.Date).ToList());
        }
        List<List<ChatMessageDto>> llcmd = _mapper.Map<List<List<ChatMessage>>, List<List<ChatMessageDto>>>(
            conversations.Where(c => c.Count > 0).ToList(),
                opt => opt.AfterMap((src, dest) => dest.ForEach(d => {
                d.ForEach(id => {
                    id.AmISender = id.SenderId == userId;
                });
            }))
        );
        return Ok(llcmd);    
    }
}