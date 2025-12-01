using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static DentalClinic.Cms.Services.AuthApiClient;

namespace DentalClinic.Cms.Services
{
    public interface IAuthApiClient
    {
        Task<LoginResult?> LoginAsync(string email, string password);
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

        public class LoginResult
        {
            public string token { get; set; } = string.Empty;
            public string? role { get; set; }   // 👈 مهم: نقرأ الدور من الـ JSON
        }
    }
}
