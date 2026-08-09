
using System.ComponentModel.DataAnnotations;

namespace PolioEradication.Models.Entities
{
    public class VaccinationRequest
    {
        public int Id { get; set; }

        [Required]
        public string PatientId { get; set; } = string.Empty;
        public ApplicationUser? Patient { get; set; }

        public int? ScheduleId { get; set; }
        public VaccinationSchedule? Schedule { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Approved, Completed

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Required]
        public string Address { get; set; } = string.Empty;
    }
}
