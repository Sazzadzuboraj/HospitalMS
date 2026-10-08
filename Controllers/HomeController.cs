using HospitalMS.Data;
using HospitalMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");

            var vm = new DashboardViewModel
            {
                TotalPatients = await _context.Patients.CountAsync(p => p.IsActive),
                TotalDoctors = doctors.Count,
                TodayAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate.Date == today),
                AvailableBeds = await _context.Beds.CountAsync(b => b.Status == BedStatus.Available && b.IsActive),
                OccupiedBeds = await _context.Beds.CountAsync(b => b.Status == BedStatus.Occupied && b.IsActive),
                PendingBills = await _context.Billings.CountAsync(b => b.Status == PaymentStatus.Pending || b.Status == PaymentStatus.Partial),
                TodayRevenue = await _context.Billings
                    .Where(b => b.PaidAt.HasValue && b.PaidAt.Value.Date == today)
                    .SumAsync(b => (decimal?)b.AmountPaid) ?? 0,
                MonthlyRevenue = await _context.Billings
                    .Where(b => b.PaidAt.HasValue && b.PaidAt.Value >= monthStart)
                    .SumAsync(b => (decimal?)b.AmountPaid) ?? 0,
                UpcomingAppointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Where(a => a.AppointmentDate >= today && a.Status != AppointmentStatus.Cancelled)
                    .OrderBy(a => a.AppointmentDate)
                    .Take(6)
                    .ToListAsync(),
                RecentBills = await _context.Billings
                    .Include(b => b.Patient)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                AppointmentsByStatus = await _context.Appointments
                    .GroupBy(a => a.Status.ToString())
                    .Select(g => new { Key = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Key, x => x.Count),
                BedsByWard = await _context.Beds
                    .Where(b => b.IsActive)
                    .GroupBy(b => b.WardName)
                    .Select(g => new { Key = g.Key, Count = g.Count(b => b.Status == BedStatus.Available) })
                    .ToDictionaryAsync(x => x.Key, x => x.Count)
            };

            return View(vm);
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }
    }
}
