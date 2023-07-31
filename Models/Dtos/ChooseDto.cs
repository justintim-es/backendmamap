
namespace latest.Models.Dtos;

public class ChooseDto {
    public string City { get; set; } = "";

    public List<CareGiverChooseDto> CareGivers { get; set; }
}