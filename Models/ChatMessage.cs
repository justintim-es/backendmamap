
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using latest.Models.Enums;

namespace latest.Models;

public class ChatMessage {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? Content { get; set; }
    public CareRequest? CareRequest { get; set; }
    [ForeignKey("CareRequest")]
    public int? CareRequestId { get; set; }
    public CareUser Sender { get; set; } = null!;
    [ForeignKey("Sender")]
    [Required]
    public Guid SenderId { get; set; }
    public CareUser Reciever { get; set; } = null!;
    [ForeignKey("Reciever")]
    [Required]
    public Guid RecieverId { get; set; }
    public bool Seen { get; set; } = false;
    public bool ShouldRenderTile { get; set; } = true;
    public bool AlwaysRenderTile { get; set; } = false;
    public ChatMessageButtonEnum ChatMessageButtonEnum { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    
}