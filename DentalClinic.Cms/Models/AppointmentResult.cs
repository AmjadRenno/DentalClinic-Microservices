public class AppointmentResult
{
    public Guid Id { get; set; }

    public string Status { get; set; }

    public string DentistId { get; set; }  

    public string DentistName { get; set; } 

    public AppointmentSlot Slot { get; set; }
}

public class AppointmentSlot
{
    public string Date { get; set; }
    public string Time { get; set; }
}
