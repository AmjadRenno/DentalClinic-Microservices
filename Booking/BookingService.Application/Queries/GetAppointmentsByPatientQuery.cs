using BookingService.Application.Interfaces;
using BookingService.Domain.Entities;

namespace BookingService.Application.Queries
{
    public class GetAppointmentsByPatientQuery
    {
        private readonly IAppointmentRepository _repo;

        public GetAppointmentsByPatientQuery(IAppointmentRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Appointment>> Handle(Guid patientId)
        {
            return await _repo.GetByPatientIdAsync(patientId);
        }
    }
}
