
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace latest.Models;

public class PaymentRequest {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id { get; set; } = "";
    public string Url { get; set; } = "";
    public int Amount { get; set; }
    public string SessionId { get; set; } = "";
    public bool IsPayed { get; set; } = false;
    public Appointment Appointment { get; set; } = null!;

}
