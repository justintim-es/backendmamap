using System.Collections.ObjectModel;
using latest.Models;
namespace latest.Models.Dtos;

public class CareGiverChooseDto {
    public Guid Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string City { get; set; } = "";
    public int Age { get; set; }
    public string Biography { get; set; } = "";
    public bool HasImage { get; set; }
    public int AverageStars { get; set; }
    public CourseDto? Course { get; set; }
    public ICollection<AppointmentDto> Appointments { get; set; }
    public CareGiverChooseDto()
    {
        Appointments = new Collection<AppointmentDto>();
    }
}