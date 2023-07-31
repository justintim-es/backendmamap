using System.ComponentModel.DataAnnotations;
using latest.Validation;

namespace latest.Models.Dtos;

public class CareRequestOfferDto {
    /// wee need to check what if date is null too before we are ready
    // [InTheFuture(ErrorMessage = "Deze dag is al voorbij")]
    public DateTime? Date { get; set; }
    public string? Time { get; set; }
    [Range(2, int.MaxValue, ErrorMessage = "compensatie moet minimaal €2 zijn")]
    public int? Compensation { get; set; }

}