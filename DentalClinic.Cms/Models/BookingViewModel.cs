using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DentalClinic.Cms.Models
{
    public class BookingViewModel
    {
        // 🔹 عنوان الصفحة القادم من الـ Document Type
        public string BookingPageTitle { get; set; }

        // 🔹 قائمة الأطباء القادمة من الـ API
        public List<DentistViewModel> Dentists { get; set; } = new();

        // 🔹 الحقول المرسلة عند الحجز
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
