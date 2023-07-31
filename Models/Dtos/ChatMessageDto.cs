using latest.Models.Enums;
namespace latest.Models.Dtos;

public class ChatMessageDto {
    public int Id { get; set; }
    public string? Content { get; set; }
    public CareRequestDto? CareRequest { get; set; }
    public int? CareRequestId { get; set; }
    public Guid SenderId { get; set; }
    public ChatMessageCareUserDto Sender { get; set; } = null!;
    public Guid RecieverId { get; set; }
    public ChatMessageCareUserDto Reciever { get; set; } = null!;
    public bool Seen { get; set; }
    public bool AmISender { get; set; }
    public bool ShouldRenderTile { get; set; }
    public bool AlwaysRenderTile { get; set; }
    public string ChatMessageButtonEnum { get; set; } = "";
    public DateTime Date { get; set; }
}