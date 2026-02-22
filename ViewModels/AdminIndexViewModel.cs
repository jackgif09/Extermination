using Extermination.Models;

namespace Extermination.ViewModels;

public class AdminIndexViewModel
{
    public int TotalCount { get; set; }
    public int NewCount { get; set; }
    public int ScheduledCount { get; set; }
    public int CompletedCount { get; set; }
    public IEnumerable<ServiceRequest> Requests { get; set; } = [];
}
