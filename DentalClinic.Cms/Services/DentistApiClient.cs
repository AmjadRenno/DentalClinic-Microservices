using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DentalClinic.Cms.Services
{
    public class DentistApiClient : IDentistApiClient
    {
        private readonly HttpClient _http;

        public DentistApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<DentistDto>> GetDentistsAsync()
        {
            var result = await _http.GetFromJsonAsync<List<DentistDto>>("/api/dentists");

            return result ?? new List<DentistDto>();
        }
    }
}
