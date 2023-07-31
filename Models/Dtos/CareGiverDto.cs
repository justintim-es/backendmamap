using System.ComponentModel.DataAnnotations;
using latest.Validation;

namespace latest.Models.Dtos;

public class CareGiverDto {
    [Required(ErrorMessage = "Wij hebben uw voornaam nodig")]
    public string FirstName { get; set; } = "";
    [Required(ErrorMessage = "Wij hebben uw achternaam nodig")]
    public string LastName { get; set; } = "";
    [Required(ErrorMessage = "Wij hebben uw woonplaats nodig")]
    public string City { get; set; } = "";
    [Required(ErrorMessage = "Wij hebben uw telefoonnummer nodig")]
    public string PhoneNumber { get; set; } = "";
    [Required(ErrorMessage = "Wij hebben uw geboortedatum nodig")]
    [Min18Attribute(ErrorMessage = "U moet minstens 18 jaar oud zijn")]
    public DateTime DateOfBirth { get; set; }
    public CourseDto? Course { get; set; }
    [Required(ErrorMessage = "Wij hebben uw E-Mail nodig")]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Wij hebben een wachtwoord nodig")]
    public string Password { get; set; } =  "";
}