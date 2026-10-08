namespace HospitalMS.Models
{
    public class DashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TodayAppointments { get; set; }
        public int AvailableBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int PendingBills { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }

        public List<Appointment> UpcomingAppointments { get; set; } = new();
        public List<Bed> CriticalBeds { get; set; } = new();
        public List<Billing> RecentBills { get; set; } = new();
        public Dictionary<string, int> AppointmentsByStatus { get; set; } = new();
        public Dictionary<string, int> BedsByWard { get; set; } = new();
    }
}
