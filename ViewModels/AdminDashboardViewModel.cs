using Extermination.Models;

namespace Extermination.ViewModels;

public class AdminDashboardViewModel
{
    // Tab data
    public IList<ServiceRequest> TodaySchedule { get; set; } = [];
    public IList<ServiceRequest> UpcomingRequests { get; set; } = [];
    public IList<ServiceRequest> PastRequests { get; set; } = [];

    // Hero
    public ServiceRequest? NextAppointment { get; set; }

    // Tab badges (unconfirmed = New status)
    public int TodayNewCount { get; set; }
    public int UpcomingNewCount { get; set; }

    public int TodayCount => TodaySchedule.Count;

    // Metric tracker
    public int NewRequestsCount { get; set; }
    public int BookedCount { get; set; }
    public int CompletedThisMonthCount { get; set; }
    public decimal TotalRevenue { get; set; }
}
