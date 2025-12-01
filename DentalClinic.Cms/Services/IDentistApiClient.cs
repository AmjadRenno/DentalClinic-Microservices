using System.Collections.Generic;
using System.Threading.Tasks;

namespace DentalClinic.Cms.Services
{
    public interface IDentistApiClient
    {
        Task<List<DentistDto>> GetDentistsAsync();
    }

    public class DentistDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
    }
}
