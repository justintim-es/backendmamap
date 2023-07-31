
using System.ComponentModel.DataAnnotations.Schema;

namespace latest.Models;

public class CareGiverCareRequest {
    public CareRequest CareRequest { get; set; } = null!;
    [ForeignKey("CareRequest")]
    public int CareRequestId { get; set; }

    public CareGiver CareGiver { get; set; } = null!;
    [ForeignKey("CareGiver")]
    public Guid CareGiverId { get; set; }


}