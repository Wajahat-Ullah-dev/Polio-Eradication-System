
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolioEradication.Data;
using PolioEradication.Models.Entities;

namespace PolioEradication.Controllers
{
    [Authorize(Roles = "Patient,Admin")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var myRequests = await _context.VaccinationRequests
                .Where(r => r.PatientId == currentUser.Id)
                .Include(r => r.Schedule)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(myRequests);
        }

        public async Task<IActionResult> Schedule()
        {
            var schedules = await _context.VaccinationSchedules
                .Where(s => s.Date >= DateTime.Today)
                .OrderBy(s => s.Date)
                .ToListAsync();
            return View(schedules);
        }

        public IActionResult RequestVaccination(int? scheduleId)
        {
            ViewBag.ScheduleId = scheduleId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestVaccination([Bind("Address,ScheduleId")] VaccinationRequest request)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Manually bind properties not in form
            request.PatientId = currentUser.Id;
            request.RequestDate = DateTime.Now;
            request.Status = "Pending";

            ModelState.Remove("Patient");
            ModelState.Remove("PatientId");
            // ScheduleId is nullable, so if it's null it's fine (general request)

            if (ModelState.IsValid)
            {
                _context.Add(request);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(request);
        }

        public IActionResult ContactAdmin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ContactAdmin(string subject, string message)
        {
            ViewBag.Message = "Your message has been sent to the Administrator.";
            return View();
        }
    }
}
