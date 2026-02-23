using Extermination.Models;

namespace Extermination.ViewModels;

public class AdminDashboardViewModel
{
    public int TodayCount { get; set; }
    public int WeekCount { get; set; }
    public int PendingCount { get; set; }
    public decimal MonthRevenue { get; set; }
    public IEnumerable<ServiceRequest> TodaySchedule { get; set; } = [];
    public IEnumerable<ServiceRequest> AllRequests { get; set; } = [];
    public IEnumerable<ServiceRequest> WeekAppointments { get; set; } = [];
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
}
