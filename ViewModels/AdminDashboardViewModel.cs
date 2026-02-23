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
}
