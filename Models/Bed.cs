using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Models
{
    public enum BedStatus
    {
        Available,
        Occupied,
        Reserved,
        Maintenance,
        Cleaning
    }

    public enum WardType
    {
        General,
        ICU,
        CCU,
        Pediatric,
        Maternity,
        Private,
        Isolation
    }

    public class Bed
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string BedNumber { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string WardName { get; set; } = string.Empty;

        public WardType WardType { get; set; } = WardType.General;

        public int Floor { get; set; }

        public BedStatus Status { get; set; } = BedStatus.Available;

        public decimal DailyRate { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<BedAssignment> Assignments { get; set; } = new List<BedAssignment>();
    }

    public class BedAssignment
    {
        public int Id { get; set; }

        public int BedId { get; set; }
        public Bed Bed { get; set; } = null!;

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public DateTime AdmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DischargedAt { get; set; }

        [StringLength(200)]
        public string? AdmissionNotes { get; set; }

        public string? AdmittedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
