using HospitalMS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<BedAssignment> BedAssignments { get; set; }
        public DbSet<Billing> Billings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Patient>(e =>
            {
                e.HasIndex(p => p.PatientCode).IsUnique();
                e.HasIndex(p => p.Phone);
            });

            builder.Entity<Appointment>(e =>
            {
                e.HasOne(a => a.Patient).WithMany(p => p.Appointments).HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(a => a.Doctor).WithMany().HasForeignKey(a => a.DoctorId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Bed>(e =>
            {
                e.HasIndex(b => b.BedNumber).IsUnique();
            });

            builder.Entity<BedAssignment>(e =>
            {
                e.HasOne(ba => ba.Bed).WithMany(b => b.Assignments).HasForeignKey(ba => ba.BedId);
                e.HasOne(ba => ba.Patient).WithMany(p => p.BedAssignments).HasForeignKey(ba => ba.PatientId);
            });

            builder.Entity<Billing>(e =>
            {
                e.HasIndex(b => b.InvoiceNumber).IsUnique();
                e.HasOne(b => b.Patient).WithMany(p => p.Billings).HasForeignKey(b => b.PatientId);
            });
        }
    }
}
