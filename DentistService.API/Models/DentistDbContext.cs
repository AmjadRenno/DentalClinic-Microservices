using Microsoft.EntityFrameworkCore;

namespace DentistService.API.Models;

public class DentistDbContext : DbContext
{
    public DentistDbContext(DbContextOptions<DentistDbContext> options)
        : base(options)
    {
    }

    public DbSet<Dentist> Dentists => Set<Dentist>();
}
