
using System.ComponentModel.DataAnnotations;

namespace PolioEradication.Models.Entities
{
    public class VaccinationSchedule
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        public string? AgeGroup { get; set; }
    }
}
