namespace Extermination.ViewModels;

public class CalendarWeekViewModel
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public IEnumerable<CalendarAppointmentDto> Appointments { get; set; } = [];
}

public class CalendarAppointmentDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PestType { get; set; } = string.Empty;
    public DateTime ScheduledFor { get; set; }
    public int StatusInt { get; set; }
    public string Status { get; set; } = string.Empty;
}
