

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace latest.Models;

public enum Interval {
    OneTime,
    Weekly,
    Monthly,
    Annual
}

public class CareRequest {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string Task { get; set; } = "";
    [Required]
    public string Description { get; set; } = "";
    [Required]
    public Interval Interval { get; set; }
    public CareConsumer CareConsumer { get; set; } = null!;
    [ForeignKey("CareConsumer")]
    [Required]
    public Guid CareConsumerId { get; set; }

    public ICollection<CareGiverCareRequest> CareGiverCareRequests { get; set; }
    public ICollection<Appointment> Appointments { get; set; }
    
    public CareRequest() {
        CareGiverCareRequests = new Collection<CareGiverCareRequest>();
        Appointments = new Collection<Appointment>();
    }

}