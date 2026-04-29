using System.ComponentModel.DataAnnotations;

namespace SmartBank.MVC.ViewModels
{
    public class TicketCreateViewModel
    {
        [Required(ErrorMessage = "Subject is required.")]
        [MaxLength(150)]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
    }
}