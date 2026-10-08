using HospitalMS.Data;
using HospitalMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillingController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index(string? status)
        {
            var query = _context.Billings.Include(b => b.Patient).AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<PaymentStatus>(status, out var st))
                query = query.Where(b => b.Status == st);

            var bills = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
            ViewBag.Statuses = new SelectList(Enum.GetValues<PaymentStatus>());
            ViewBag.SelectedStatus = status;
            return View(bills);
        }

        public async Task<IActionResult> Details(int id)
        {
            var bill = await _context.Billings
                .Include(b => b.Patient)
                .Include(b => b.Appointment)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (bill == null) return NotFound();
            return View(bill);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Patients = new SelectList(
                await _context.Patients.Where(p => p.IsActive).OrderBy(p => p.FullName).ToListAsync(),
                "Id", "FullName");
            return View(new Billing { InvoiceNumber = GenerateInvoice() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Billing billing)
        {
            if (ModelState.IsValid)
            {
                billing.InvoiceNumber = GenerateInvoice();
                billing.CreatedAt = DateTime.UtcNow;
                billing.CreatedBy = User.Identity?.Name;
                if (billing.AmountPaid >= billing.TotalAmount)
                {
                    billing.Status = PaymentStatus.Paid;
                    billing.PaidAt = DateTime.UtcNow;
                }
                else if (billing.AmountPaid > 0)
                    billing.Status = PaymentStatus.Partial;

                _context.Billings.Add(billing);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Invoice created successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Patients = new SelectList(
                await _context.Patients.Where(p => p.IsActive).ToListAsync(), "Id", "FullName");
            return View(billing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPayment(int id, decimal amount, PaymentMethod method)
        {
            var bill = await _context.Billings.FindAsync(id);
            if (bill == null) return NotFound();

            bill.AmountPaid += amount;
            bill.Method = method;
            if (bill.AmountPaid >= bill.TotalAmount)
            {
                bill.Status = PaymentStatus.Paid;
                bill.PaidAt = DateTime.UtcNow;
            }
            else
                bill.Status = PaymentStatus.Partial;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Payment of {amount:C} recorded.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private string GenerateInvoice()
        {
            var count = _context.Billings.Count() + 1;
            return $"INV-{DateTime.Now:yyyy}-{1000 + count}";
        }
    }
}
