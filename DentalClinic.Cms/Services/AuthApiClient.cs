using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DentalClinic.Cms.Models;
using static DentalClinic.Cms.Services.AuthApiClient;

namespace DentalClinic.Cms.Services
{
    public interface IAuthApiClient
    {
        Task<LoginResult?> LoginAsync(string email, string password);
        Task<bool> RegisterAsync(RegisterFormModel model);
    }

    public class AuthApiClient : IAuthApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResult?> LoginAsync(string username, string password)
        {
            var payload = new { Username = username, Password = password };

            var response = await _httpClient.PostAsJsonAsync("/auth/login", payload);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadFromJsonAsync<LoginResult>();

            return json;
        }

        public async Task<bool> RegisterAsync(RegisterFormModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("/auth/register", model);
            return response.IsSuccessStatusCode;
        }

        public class LoginResult
        {
            public string token { get; set; } = string.Empty;
            public string? role { get; set; }
        }
    }
}
