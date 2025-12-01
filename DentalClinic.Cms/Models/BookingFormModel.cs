using System.ComponentModel.DataAnnotations;

namespace DentalClinic.Cms.Models
{
    public class BookingFormModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be at least 2 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Dentist is required")]
        public string DentistId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required")]
        public string Date { get; set; } = string.Empty;

        [Required(ErrorMessage = "Time is required")]
        public string Time { get; set; } = string.Empty;
    }
}
