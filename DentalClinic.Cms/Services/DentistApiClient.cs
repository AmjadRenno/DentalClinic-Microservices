using System.Collections.Generic;
using System.Threading.Tasks;

namespace DentalClinic.Cms.Services
{
    /// <summary>
    /// In-memory dentist list for simplified architecture
    /// </summary>
    public class InMemoryDentistApiClient : IDentistApiClient
    {
        private static readonly List<DentistDto> _dentists = new()
        {
            new DentistDto { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), FullName = "Dr. Ahmad Al-Sayed" },
            new DentistDto { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), FullName = "Dr. Fatima Hassan" },
            new DentistDto { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), FullName = "Dr. Omar Khalil" }
        };

        public Task<List<DentistDto>> GetDentistsAsync()
        {
            return Task.FromResult(_dentists);
        }
    }
}
