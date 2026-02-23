using Extermination.Models;

namespace Extermination.ViewModels;

public class AppointmentDetailViewModel
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public PestType PestType { get; set; }
    public string? Description { get; set; }
    public DateTime? PreferredDate { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public decimal? Price { get; set; }
    public ServiceStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }

    // Computed contact link properties
    public string PhoneHref => $"tel:{Phone}";
    public string SmsHref => $"sms:{Phone}";
    public string EmailHref => $"mailto:{Email}";
    public string MapsHref => $"https://maps.google.com/?q={Uri.EscapeDataString(Address)}";
}
