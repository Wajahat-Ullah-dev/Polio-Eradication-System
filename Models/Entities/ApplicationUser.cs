
using Microsoft.AspNetCore.Identity;

namespace PolioEradication.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? CNIC { get; set; }
        public string? Address { get; set; }
        public string? Role { get; set; } // "Admin", "HealthWorker", "Patient" - For easier querying if needed, though Identity Roles manage this.
    }
}
