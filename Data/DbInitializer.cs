using HospitalMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();

            // Roles
            string[] roles = { "Admin", "Doctor", "Nurse", "Receptionist", "Accountant" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Admin
            if (await userManager.FindByEmailAsync("admin@hospitalms.com") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@hospitalms.com",
                    Email = "admin@hospitalms.com",
                    FullName = "System Administrator",
                    EmployeeId = "EMP-001",
                    Department = "Administration",
                    EmailConfirmed = true,
                    IsActive = true
                };
                await userManager.CreateAsync(admin, "Admin@12345");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Doctors
            var doctors = new[]
            {
                new { Email = "dr.smith@hospitalms.com", Name = "Dr. Sarah Smith", Spec = "Cardiology", Dept = "Cardiology" },
                new { Email = "dr.khan@hospitalms.com", Name = "Dr. Ahmed Khan", Spec = "Neurology", Dept = "Neurology" },
                new { Email = "dr.patel@hospitalms.com", Name = "Dr. Priya Patel", Spec = "Pediatrics", Dept = "Pediatrics" },
                new { Email = "dr.wilson@hospitalms.com", Name = "Dr. James Wilson", Spec = "Orthopedics", Dept = "Orthopedics" },
            };

            foreach (var d in doctors)
            {
                if (await userManager.FindByEmailAsync(d.Email) == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = d.Email,
                        Email = d.Email,
                        FullName = d.Name,
                        Specialization = d.Spec,
                        Department = d.Dept,
                        EmployeeId = $"DOC-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                        EmailConfirmed = true,
                        IsActive = true
                    };
                    await userManager.CreateAsync(user, "Doctor@123");
                    await userManager.AddToRoleAsync(user, "Doctor");
                }
            }

            // Receptionist
            if (await userManager.FindByEmailAsync("reception@hospitalms.com") == null)
            {
                var rec = new ApplicationUser
                {
                    UserName = "reception@hospitalms.com",
                    Email = "reception@hospitalms.com",
                    FullName = "Emily Johnson",
                    EmployeeId = "REC-001",
                    Department = "Front Desk",
                    EmailConfirmed = true,
                    IsActive = true
                };
                await userManager.CreateAsync(rec, "Reception@123");
                await userManager.AddToRoleAsync(rec, "Receptionist");
            }

            if (await context.Patients.AnyAsync()) return;

            // Sample Patients
            var patients = new List<Patient>
            {
                new() { PatientCode = "PAT-2026-001", FullName = "John Doe", DateOfBirth = new DateTime(1985, 3, 15), Gender = "Male", BloodGroup = "O+", Phone = "+1-555-0101", Email = "john.doe@email.com", Address = "123 Main St, Springfield", EmergencyContact = "Jane Doe", EmergencyPhone = "+1-555-0102", MedicalHistory = "Hypertension", Allergies = "Penicillin" },
                new() { PatientCode = "PAT-2026-002", FullName = "Maria Garcia", DateOfBirth = new DateTime(1992, 7, 22), Gender = "Female", BloodGroup = "A+", Phone = "+1-555-0201", Email = "maria.g@email.com", Address = "456 Oak Ave, Riverside", EmergencyContact = "Carlos Garcia", EmergencyPhone = "+1-555-0202", Allergies = "None" },
                new() { PatientCode = "PAT-2026-003", FullName = "Robert Chen", DateOfBirth = new DateTime(1978, 11, 8), Gender = "Male", BloodGroup = "B+", Phone = "+1-555-0301", Email = "r.chen@email.com", Address = "789 Pine Rd, Lakeside", EmergencyContact = "Lisa Chen", EmergencyPhone = "+1-555-0302", MedicalHistory = "Diabetes Type 2", Allergies = "Sulfa" },
                new() { PatientCode = "PAT-2026-004", FullName = "Aisha Rahman", DateOfBirth = new DateTime(1995, 1, 30), Gender = "Female", BloodGroup = "AB+", Phone = "+1-555-0401", Email = "aisha.r@email.com", Address = "321 Elm St, Hillview", EmergencyContact = "Omar Rahman", EmergencyPhone = "+1-555-0402" },
                new() { PatientCode = "PAT-2026-005", FullName = "Michael Brown", DateOfBirth = new DateTime(1965, 5, 12), Gender = "Male", BloodGroup = "O-", Phone = "+1-555-0501", Email = "m.brown@email.com", Address = "654 Cedar Ln, Brookside", EmergencyContact = "Susan Brown", EmergencyPhone = "+1-555-0502", MedicalHistory = "COPD, Heart Disease" },
            };
            context.Patients.AddRange(patients);
            await context.SaveChangesAsync();

            // Beds
            var wards = new[] { ("General Ward A", WardType.General, 1, 150m), ("ICU", WardType.ICU, 2, 800m), ("CCU", WardType.CCU, 2, 750m), ("Pediatric Ward", WardType.Pediatric, 1, 200m), ("Private Suite", WardType.Private, 3, 500m), ("Maternity", WardType.Maternity, 1, 350m) };
            int bedCounter = 1;
            foreach (var (name, type, floor, rate) in wards)
            {
                int count = type == WardType.ICU || type == WardType.CCU ? 6 : type == WardType.Private ? 4 : 10;
                for (int i = 1; i <= count; i++)
                {
                    context.Beds.Add(new Bed
                    {
                        BedNumber = $"B{bedCounter:D3}",
                        WardName = name,
                        WardType = type,
                        Floor = floor,
                        DailyRate = rate,
                        Status = bedCounter % 5 == 0 ? BedStatus.Occupied : bedCounter % 7 == 0 ? BedStatus.Maintenance : BedStatus.Available
                    });
                    bedCounter++;
                }
            }
            await context.SaveChangesAsync();

            // Appointments
            var doctorUsers = await userManager.GetUsersInRoleAsync("Doctor");
            var doctorList = doctorUsers.ToList();
            var statuses = Enum.GetValues<AppointmentStatus>();
            var rnd = new Random(42);

            for (int i = 0; i < 15; i++)
            {
                context.Appointments.Add(new Appointment
                {
                    PatientId = patients[i % patients.Count].Id,
                    DoctorId = doctorList[i % doctorList.Count].Id,
                    AppointmentDate = DateTime.Today.AddDays(rnd.Next(-2, 7)).AddHours(9 + (i % 8)),
                    TimeSlot = $"{9 + (i % 8):D2}:00 - {10 + (i % 8):D2}:00",
                    Reason = new[] { "General Checkup", "Follow-up", "Consultation", "Lab Review", "Pre-op Assessment" }[i % 5],
                    Status = statuses[i % statuses.Length],
                    Notes = i % 3 == 0 ? "Patient requested morning slot" : null
                });
            }
            await context.SaveChangesAsync();

            // Billings
            for (int i = 0; i < 8; i++)
            {
                var total = 150 + rnd.Next(50, 800);
                context.Billings.Add(new Billing
                {
                    InvoiceNumber = $"INV-2026-{1001 + i}",
                    PatientId = patients[i % patients.Count].Id,
                    ConsultationFee = 150,
                    LabCharges = rnd.Next(0, 300),
                    MedicineCharges = rnd.Next(0, 200),
                    RoomCharges = i < 3 ? rnd.Next(200, 600) : 0,
                    Discount = i % 4 == 0 ? 50 : 0,
                    TaxAmount = 0,
                    AmountPaid = i % 3 == 0 ? total : i % 2 == 0 ? total / 2 : 0,
                    Status = i % 3 == 0 ? PaymentStatus.Paid : i % 2 == 0 ? PaymentStatus.Partial : PaymentStatus.Pending,
                    Method = i % 3 == 0 ? PaymentMethod.Card : null,
                    PaidAt = i % 3 == 0 ? DateTime.UtcNow.AddDays(-i) : null
                });
            }
            await context.SaveChangesAsync();
        }
    }
}
