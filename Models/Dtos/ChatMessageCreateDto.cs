
using System.ComponentModel.DataAnnotations;

namespace latest.Models.Dtos;

public class ChatMessageCreateDto {
    [Required(ErrorMessage = "Uw bericht kan niet leeg zijn")]
    public string Content { get; set; } = "";

    public Guid RecieverId { get; set; }    
}