
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace latest.Models.Dtos;

public class CareRequestDto {
    public int Id { get; set; }
    [Required(ErrorMessage = "Wij hebben uw taak nodig")]
    public string Task { get; set; } = "";
    [Required(ErrorMessage = "Wij hebben uw omschrijving nodig")]
    public string Description { get; set; } = "";
    [Required(ErrorMessage = "Wij hebben uw interval nodig")]
    public string Interval { get; set; } = "";
    // public DateTime? Date { get; set; }
    public Guid CareConsumerId { get; set; }
    // public string? Time { get; set; } = "";
    // public int? Compensation { get; set; }
    public bool isCareConsumer { get; set; }
    public AppointmentDto? Appointment { get; set; } = null!;
}