using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string PatientCode { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required, StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [StringLength(20)]
        public string? BloodGroup { get; set; }

        [Phone, StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress, StringLength(100)]
        public string? Email { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? EmergencyContact { get; set; }

        [StringLength(20)]
        public string? EmergencyPhone { get; set; }

        [StringLength(500)]
        public string? MedicalHistory { get; set; }

        [StringLength(300)]
        public string? Allergies { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Billing> Billings { get; set; } = new List<Billing>();
        public ICollection<BedAssignment> BedAssignments { get; set; } = new List<BedAssignment>();
    }
}
