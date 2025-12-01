using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DentalClinic.Cms.Models;

namespace DentalClinic.Cms.Services
{
    public interface IUserRegistrationApiClient
    {
        Task<bool> RegisterUserAsync(RegisterFormModel model);
    }

    public class UserRegistrationApiClient : IUserRegistrationApiClient
    {
        private readonly HttpClient _http;

        public UserRegistrationApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> RegisterUserAsync(RegisterFormModel model)
        {
            var response = await _http.PostAsJsonAsync("/auth/register", model);
            return response.IsSuccessStatusCode;
        }
    }
}
