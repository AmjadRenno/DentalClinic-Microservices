using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DentalClinic.Cms.Models
{
    public class BookingViewModel
    {
        public string BookingPageTitle { get; set; }

        public List<DentistViewModel> Dentists { get; set; } = new();

        [Required]
        public string FullName { get; set; }

        [Required]
        public string DentistId { get; set; }

        [Required]
        public string Date { get; set; }

        [Required]
        public string Time { get; set; }
    }
}
