

using System.ComponentModel.DataAnnotations;
using latest.Validation;

namespace latest.Models.Dtos;

public class CareConsumerDto {
    [Required(ErrorMessage = "Wij hebben uw voornaam nodig")]
    public string FirstName { get; set; } = "";
    [Required(ErrorMessage = "Wij hebben uw achternaam nodig")]
    public string LastName { get; set; } = "";
    [Required(ErrorMessage = "Wij hebben uw geboortedatum nodig")]
    [NotToday(ErrorMessage = "De datum van vandaag is ongeldig")]
    public DateTime DateOfBirth { get; set; }
    [Required(ErrorMessage = "Wij hebben uw adres nodig")]
    public string AddressOne { get; set; } = "";
    public string AddressTwo { get; set; } = "";
    [Required(ErrorMessage = "Wij hebben uw postcode nodig")]
    public string ZipCode { get; set; } = "";
    [Required(ErrorMessage = "Wij hebben uw stad nodig")]
    public string City { get; set; } = "";
    [Required(ErrorMessage = "Wij hebben uw telefoonnummer nodig")]
    public string PhoneNumber { get; set; } = "";
    [Required(ErrorMessage = "Wij hebben uw E-Mail nodig")]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Wij hebben een wachtwoord nodig")]
    public string Password { get; set; } = "";
}