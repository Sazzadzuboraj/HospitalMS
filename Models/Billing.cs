using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Models
{
    public enum PaymentStatus
    {
        Pending,
        Partial,
        Paid,
        Overdue,
        Cancelled,
        Refunded
    }

    public enum PaymentMethod
    {
        Cash,
        Card,
        UPI,
        Insurance,
        BankTransfer,
        Cheque
    }

    public class Billing
    {
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public int? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        public decimal ConsultationFee { get; set; }
        public decimal LabCharges { get; set; }
        public decimal MedicineCharges { get; set; }
        public decimal RoomCharges { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxAmount { get; set; }

        public decimal TotalAmount => ConsultationFee + LabCharges + MedicineCharges + RoomCharges + OtherCharges - Discount + TaxAmount;

        public decimal AmountPaid { get; set; }
        public decimal Balance => TotalAmount - AmountPaid;

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public PaymentMethod? Method { get; set; }

        [StringLength(300)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}
