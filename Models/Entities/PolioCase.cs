
using System.ComponentModel.DataAnnotations;

namespace PolioEradication.Models.Entities
{
    public class PolioCase
    {
        public int Id { get; set; }

        [Required]
        public string ReportedById { get; set; } = string.Empty;
        public ApplicationUser? ReportedBy { get; set; }

        [Required]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        public int Age { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime DateReported { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Reported"; // Reported, Investigating, Confirmed, Discarded
    }
}
