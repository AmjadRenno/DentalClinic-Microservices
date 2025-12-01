using System;
using System.ComponentModel.DataAnnotations;

namespace DentalClinic.Cms.Models
{
    public class PaymentFormViewModel
    {
        [Required]
        public Guid BookingId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public string PaymentMethod { get; set; } = "Card";

        [Required(ErrorMessage = "Card holder name is required")]
        [StringLength(100, ErrorMessage = "Card holder name is too long")]
        public string CardHolderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last 4 digits are required")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "Card last 4 must be 4 digits")]
        public string CardLast4 { get; set; } = string.Empty;
    }
}
