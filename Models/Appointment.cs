
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace latest.Models;

public class Appointment {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public DateTime? Date { get; set; }
    public string? Time { get; set; } // can be removed ithink
    public int? Compensation { get; set; }
    public CareRequest CareRequest { get; set; } = null!;
    [ForeignKey("CareRequest")]
    public int CareRequestId { get; set; }
    public CareGiver CareGiver { get; set; } = null!;
    [ForeignKey("CareGiver")]
    public Guid CareGiverId { get; set; }
    public  PaymentRequest PaymentRequest { get; set; } = null!;    
    public Review Review { get; set; } = null!;
    public bool IsOccupied { get; set; } = false;
    public bool ShouldDelete { get; set; } = false;
    public DateTime Added { get; set; } = DateTime.Now;

    // public bool IsAccepted { get; set; }

}