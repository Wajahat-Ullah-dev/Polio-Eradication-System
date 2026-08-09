
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolioEradication.Data;
using PolioEradication.Models.Entities;

namespace PolioEradication.Controllers
{
    [Authorize(Roles = "HealthWorker,Admin")] 
    public class HealthWorkerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HealthWorkerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var assignedCases = await _context.PolioCases
                .Where(c => c.ReportedById == currentUser.Id)
                .OrderByDescending(c => c.DateReported)
                .ToListAsync();

            ViewBag.AssignedCasesCount = assignedCases.Count;
            // For now, assuming HealthWorkers inspect all Pending requests or assigned ones. 
            // Let's show all pending requests in their area? Or just total pending requests for now.
            ViewBag.PendingRequestsCount = await _context.VaccinationRequests.CountAsync(r => r.Status == "Pending");

            return View(assignedCases);
        }
        public async Task<IActionResult> Patients()
        {
            var patients = await _userManager.GetUsersInRoleAsync("Patient");
            return View(patients);
        }

        public async Task<IActionResult> PatientDetails(string id)
        {
            if (id == null) return NotFound();

            var patient = await _userManager.FindByIdAsync(id);
            if (patient == null) return NotFound();
            
            var requests = await _context.VaccinationRequests
                .Where(r => r.PatientId == id)
                .Include(r => r.Schedule)
                .ToListAsync();
            
            ViewBag.Requests = requests;

            return View(patient);
        }
        public IActionResult ReportCase()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportCase([Bind("Id,PatientName,Age,Location,Description,DateReported")] PolioCase polioCase)
        {
            ModelState.Remove("ReportedBy");
            ModelState.Remove("ReportedById");

            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                polioCase.ReportedById = currentUser.Id;
                polioCase.Status = "Reported";
                
                _context.Add(polioCase);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(polioCase);
        }

        public async Task<IActionResult> EditCase(int? id)
        {
            if (id == null) return NotFound();

            var polioCase = await _context.PolioCases.FindAsync(id);
            if (polioCase == null) return NotFound();
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (polioCase.ReportedById != currentUser.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(polioCase);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCase(int id, [Bind("Id,PatientName,Age,Location,Description,DateReported,Status")] PolioCase polioCase)
        {
            if (id != polioCase.Id) return NotFound();

            var existingCase = await _context.PolioCases.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (existingCase == null) return NotFound();

            ModelState.Remove("ReportedBy");
            ModelState.Remove("ReportedById");

            if (ModelState.IsValid)
            {
                try
                {
                    polioCase.ReportedById = existingCase.ReportedById; 
                    _context.Update(polioCase);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PolioCases.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(polioCase);
        }
    }
}
