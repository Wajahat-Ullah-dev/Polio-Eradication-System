
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolioEradication.Data;
using PolioEradication.Models.Entities;

namespace PolioEradication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalRequests = await _context.VaccinationRequests.CountAsync();
            var pendingRequests = await _context.VaccinationRequests.CountAsync(r => r.Status == "Pending");
            var totalCases = await _context.PolioCases.CountAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalRequests = totalRequests;
            ViewBag.PendingRequests = pendingRequests;
            ViewBag.TotalCases = totalCases;

            return View();
        }
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                user.LockoutEnd = null;
            }
            else
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            }
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Users));
        }
        public async Task<IActionResult> Schedule()
        {
            return View(await _context.VaccinationSchedules.ToListAsync());
        }

        public IActionResult CreateSchedule()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSchedule([Bind("Id,Title,Description,Date,Location,AgeGroup")] VaccinationSchedule schedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(schedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Schedule));
            }
            return View(schedule);
        }

        public async Task<IActionResult> EditSchedule(int? id)
        {
            if (id == null) return NotFound();

            var schedule = await _context.VaccinationSchedules.FindAsync(id);
            if (schedule == null) return NotFound();
            return View(schedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSchedule(int id, [Bind("Id,Title,Description,Date,Location,AgeGroup")] VaccinationSchedule schedule)
        {
            if (id != schedule.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schedule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.VaccinationSchedules.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Schedule));
            }
            return View(schedule);
        }

        public async Task<IActionResult> DeleteSchedule(int? id)
        {
            if (id == null) return NotFound();

            var schedule = await _context.VaccinationSchedules
                .FirstOrDefaultAsync(m => m.Id == id);
            if (schedule == null) return NotFound();

            return View(schedule);
        }

        [HttpPost, ActionName("DeleteSchedule")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteScheduleConfirmed(int id)
        {
            var schedule = await _context.VaccinationSchedules.FindAsync(id);
            if (schedule != null)
            {
                _context.VaccinationSchedules.Remove(schedule);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Schedule));
        }
        public async Task<IActionResult> Requests()
        {
            var requests = await _context.VaccinationRequests
                .Include(r => r.Patient)
                .Include(r => r.Schedule)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();
            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRequestStatus(int id, string status)
        {
            var request = await _context.VaccinationRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Requests));
        }
    }
}
