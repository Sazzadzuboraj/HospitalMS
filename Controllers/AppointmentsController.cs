using HospitalMS.Data;
using HospitalMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? status, DateTime? date)
        {
            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<AppointmentStatus>(status, out var st))
                query = query.Where(a => a.Status == st);

            if (date.HasValue)
                query = query.Where(a => a.AppointmentDate.Date == date.Value.Date);

            var list = await query.OrderByDescending(a => a.AppointmentDate).ToListAsync();
            ViewBag.StatusFilter = status;
            ViewBag.DateFilter = date?.ToString("yyyy-MM-dd");
            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View(new Appointment { AppointmentDate = DateTime.Today.AddHours(10) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                appointment.CreatedAt = DateTime.UtcNow;
                appointment.CreatedBy = User.Identity?.Name;
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Appointment scheduled successfully.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdowns();
            return View(appointment);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();
            await PopulateDropdowns();
            return View(appt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment appointment)
        {
            if (id != appointment.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(appointment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Appointment updated.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdowns();
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus status)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();
            appt.Status = status;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Status updated to {status}.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns()
        {
            ViewBag.Patients = new SelectList(await _context.Patients.Where(p => p.IsActive).OrderBy(p => p.FullName).ToListAsync(), "Id", "FullName");
            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");
            ViewBag.Doctors = new SelectList(doctors.Where(d => d.IsActive), "Id", "FullName");
            ViewBag.Statuses = new SelectList(Enum.GetValues<AppointmentStatus>());
            ViewBag.TimeSlots = new SelectList(new[] {
                "09:00 - 10:00", "10:00 - 11:00", "11:00 - 12:00",
                "12:00 - 13:00", "14:00 - 15:00", "15:00 - 16:00",
                "16:00 - 17:00", "17:00 - 18:00"
            });
        }
    }
}
