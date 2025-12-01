namespace DentistService.API.Models;

public class Dentist
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = default!;
    public string? Specialty { get; set; }
    public string? Email { get; set; }
}
