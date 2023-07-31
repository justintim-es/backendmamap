using System.ComponentModel.DataAnnotations;

namespace latest.Models;

public class LoginDto {
    [Required(ErrorMessage = "Uw e-mail is vereist")]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Uw wachtwoord is vereist")]

    public string Password { get; set; } = "";
}