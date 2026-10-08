using HospitalMS.Data;
using HospitalMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class BedsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BedsController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index(string? ward, string? status)
        {
            var query = _context.Beds.Where(b => b.IsActive).AsQueryable();

            if (!string.IsNullOrEmpty(ward))
                query = query.Where(b => b.WardName == ward);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<BedStatus>(status, out var st))
                query = query.Where(b => b.Status == st);

            var beds = await query.OrderBy(b => b.WardName).ThenBy(b => b.BedNumber).ToListAsync();
            ViewBag.Wards = new SelectList(await _context.Beds.Select(b => b.WardName).Distinct().ToListAsync());
            ViewBag.Statuses = new SelectList(Enum.GetValues<BedStatus>());
            ViewBag.SelectedWard = ward;
            ViewBag.SelectedStatus = status;

            ViewBag.Stats = new
            {
                Available = beds.Count(b => b.Status == BedStatus.Available),
                Occupied = beds.Count(b => b.Status == BedStatus.Occupied),
                Maintenance = beds.Count(b => b.Status == BedStatus.Maintenance),
                Total = beds.Count
            };

            return View(beds);
        }

        public async Task<IActionResult> Assign(int id)
        {
            var bed = await _context.Beds.FindAsync(id);
            if (bed == null || bed.Status != BedStatus.Available) return NotFound();

            ViewBag.Bed = bed;
            ViewBag.Patients = new SelectList(
                await _context.Patients.Where(p => p.IsActive).OrderBy(p => p.FullName).ToListAsync(),
                "Id", "FullName");
            return View(new BedAssignment { BedId = id, AdmittedAt = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(BedAssignment assignment)
        {
            var bed = await _context.Beds.FindAsync(assignment.BedId);
            if (bed == null || bed.Status != BedStatus.Available)
            {
                TempData["Error"] = "Bed is no longer available.";
                return RedirectToAction(nameof(Index));
            }

            bed.Status = BedStatus.Occupied;
            assignment.AdmittedBy = User.Identity?.Name;
            assignment.IsActive = true;
            _context.BedAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Patient assigned to bed {bed.BedNumber}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Discharge(int bedId)
        {
            var bed = await _context.Beds.FindAsync(bedId);
            if (bed == null) return NotFound();

            var assignment = await _context.BedAssignments
                .FirstOrDefaultAsync(a => a.BedId == bedId && a.IsActive);

            if (assignment != null)
            {
                assignment.IsActive = false;
                assignment.DischargedAt = DateTime.UtcNow;
            }

            bed.Status = BedStatus.Cleaning;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Patient discharged. Bed {bed.BedNumber} marked for cleaning.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, BedStatus status)
        {
            var bed = await _context.Beds.FindAsync(id);
            if (bed == null) return NotFound();
            bed.Status = status;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Bed {bed.BedNumber} status → {status}";
            return RedirectToAction(nameof(Index));
        }
    }
}
