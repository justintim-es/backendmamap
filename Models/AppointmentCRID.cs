
namespace latest.Models;

public class AppointmentCRID {
    public int CRID { get; set; }
    public Queue<Appointment> Appointments { get; set; } = null!;
}