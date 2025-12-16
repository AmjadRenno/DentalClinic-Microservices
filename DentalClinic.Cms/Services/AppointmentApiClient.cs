using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using DentalClinic.Cms.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;

namespace DentalClinic.Cms.Services
{
    public interface IAppointmentApiClient
    {
        Task<List<AppointmentResult>> GetMyAppointmentsAsync();
        Task<List<AppointmentResult>> GetDentistAppointmentsAsync();  // Changed from GetAdminAppointmentsAsync
        Task<Guid?> CreateAppointmentAsync(BookingFormModel model);
        Task<bool> CancelAppointmentAsync(Guid appointmentId);
        Task<bool> ConfirmAppointmentAsync(Guid appointmentId);
        void SetAuthToken(string token);
    }



    public class AppointmentApiClient : IAppointmentApiClient
    {
        private readonly HttpClient _http;
        private readonly IDentistApiClient _dentists;   // Added


        public AppointmentApiClient(HttpClient http, IDentistApiClient dentists)
        {
            _http = http;
            _dentists = dentists;

        }

        // ----------------------------
        // Add JWT Token for requests
        // ----------------------------
        public void SetAuthToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return;

            // If token has "Bearer " prefix, remove it
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring("Bearer ".Length);
            }

            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }


        // ----------------------------
        // Get My Appointments + Dentist Name
        // ----------------------------
        public async Task<List<AppointmentResult>> GetMyAppointmentsAsync()
        {
            var response = await _http.GetAsync("/api/appointments/mine");

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(); // New addition

                // During development only – helps us see the real error
                throw new Exception($"Error calling /api/appointments/mine. " +
                                    $"Status: {(int)response.StatusCode} {response.ReasonPhrase}. " +
                                    $"Body: {body}");
            }


            var json = await response.Content.ReadAsStringAsync();

            var appointments = JsonSerializer.Deserialize<List<AppointmentResult>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<AppointmentResult>();

            var dentistsList = await _dentists.GetDentistsAsync();

            foreach (var appt in appointments)
            {
                var d = dentistsList.FirstOrDefault(x => x.Id.ToString() == appt.DentistId);
                appt.DentistName = d != null ? d.FullName : "Unknown Dentist";
            }

            return appointments;
        }

        public async Task<List<AppointmentResult>> GetDentistAppointmentsAsync()
        {
            var response = await _http.GetAsync("/api/appointments/dentist");

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error calling /api/appointments/dentist. " +
                                    $"Status: {(int)response.StatusCode} {response.ReasonPhrase}. " +
                                    $"Body: {body}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var appointments = JsonSerializer.Deserialize<List<AppointmentResult>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<AppointmentResult>();

            var dentistsList = await _dentists.GetDentistsAsync();

            foreach (var appt in appointments)
            {
                var d = dentistsList.FirstOrDefault(x => x.Id.ToString() == appt.DentistId);
                appt.DentistName = d != null ? d.FullName : "Unknown Dentist";
            }

            return appointments;
        }



        // ----------------------------
        // Create new appointment
        // ----------------------------
        public async Task<Guid?> CreateAppointmentAsync(BookingFormModel model)
        {
            var token = _http.DefaultRequestHeaders.Authorization?.Parameter;
            if (string.IsNullOrEmpty(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return null;

            var patientId = Guid.Parse(userIdClaim.Value);

            var appointmentId = Guid.NewGuid(); // We'll use this in both client and service

            var command = new
            {
                AppointmentId = appointmentId,
                PatientId = patientId,
                DentistId = Guid.Parse(model.DentistId),
                Start = DateTime.Parse($"{model.Date}T{model.Time}"),
                End = DateTime.Parse($"{model.Date}T{model.Time}").AddMinutes(30)
            };

            var response = await _http.PostAsJsonAsync("/api/appointments", command);

            if (!response.IsSuccessStatusCode)
                return null;

            return appointmentId; // Done!
        }


        public async Task<bool> CancelAppointmentAsync(Guid appointmentId)
        {
            var response = await _http.PutAsJsonAsync("/api/appointments/cancel", new
            {
                AppointmentId = appointmentId
            });

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ConfirmAppointmentAsync(Guid appointmentId)
        {
            // BookingService expects ConfirmAppointmentCommand with AppointmentId
            var payload = new
            {
                AppointmentId = appointmentId
            };

            // Pass through the Gateway: /api/appointments/confirm
            var response = await _http.PutAsJsonAsync("/api/appointments/confirm", payload);

            return response.IsSuccessStatusCode;
        }


    }
}
