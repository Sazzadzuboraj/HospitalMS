using HospitalMS.Data;
using HospitalMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            const int pageSize = 10;
            var query = _context.Patients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(p =>
                    p.FullName.ToLower().Contains(search) ||
                    p.PatientCode.ToLower().Contains(search) ||
                    (p.Phone != null && p.Phone.Contains(search)));
            }

            var total = await query.CountAsync();
            var patients = await query
                .OrderByDescending(p => p.RegisteredAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Search = search;
            ViewBag.TotalCount = total;

            return View(patients);
        }

        public async Task<IActionResult> Details(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.Appointments).ThenInclude(a => a.Doctor)
                .Include(p => p.Billings)
                .Include(p => p.BedAssignments).ThenInclude(ba => ba.Bed)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();
            return View(patient);
        }

        public IActionResult Create() => View(new Patient { PatientCode = GenerateCode() });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Patient patient)
        {
            if (ModelState.IsValid)
            {
                patient.PatientCode = GenerateCode();
                patient.RegisteredAt = DateTime.UtcNow;
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Patient registered successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Patient patient)
        {
            if (id != patient.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(patient);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Patient updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                patient.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Patient deactivated.";
            }
            return RedirectToAction(nameof(Index));
        }

        private string GenerateCode()
        {
            var count = _context.Patients.Count() + 1;
            return $"PAT-{DateTime.Now:yyyy}-{count:D4}";
        }
    }
}
