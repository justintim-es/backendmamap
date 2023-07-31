using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using latest.Models.Enums;
namespace latest.Models;


public class Review {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Comment { get; set; } = "";
    [Required]
    public StarEnum Stars { get; set; }
    public Appointment Appointment { get; set; } = null!;

}