
using System.ComponentModel.DataAnnotations;

namespace latest.Models.Dtos;

public class ReviewDto {
    [Required(ErrorMessage = "Commentaar is verplicht")]
    public string Comment { get; set; } = "";
    [Required]
    [Range(1, 5, ErrorMessage = "U moet minimaal 1 ster geven")]
    public int Stars { get; set; }
}