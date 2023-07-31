namespace latest.Models.Dtos;

public class AppointmentDto {
    public int? Id { get; set; }
    public DateTime? Date { get; set; }
    public string? Time { get; set; }
    public int? Compensation { get; set; }
    public bool IsAccepted { get; set; }
    public Guid CareGiverId { get; set; }
    public CareUserDto CareGiver { get; set; } = null!;
    public PaymentRequestDto? PaymentRequest { get; set; }
    public CareRequestDto? CareRequest { get; set; }
    public ReviewOutDto? Review { get; set; } 
    public bool IsOccupied { get; set; } = false;
    // public int Index { get; set; }

}