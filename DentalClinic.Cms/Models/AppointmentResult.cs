public class AppointmentResult
{
    public Guid Id { get; set; }

    public string Status { get; set; }

    public string DentistId { get; set; }   // يبقى كما هو

    public string DentistName { get; set; }  // 👈 نضيف هذا

    public AppointmentSlot Slot { get; set; }
}

public class AppointmentSlot
{
    public string Date { get; set; }
    public string Time { get; set; }
}
