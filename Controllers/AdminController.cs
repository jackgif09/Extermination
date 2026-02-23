using Extermination.Data;
using Extermination.Models;
using Extermination.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Extermination.Controllers;

public class AdminController : Controller
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /Admin
    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        // Week runs Monday–Sunday
        var dow = (int)today.DayOfWeek;
        var daysFromMonday = (dow + 6) % 7;
        var weekStart = today.AddDays(-daysFromMonday);
        var weekEnd = weekStart.AddDays(6);

        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var all = await _db.ServiceRequests
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var todaySchedule = all
            .Where(r => r.ScheduledFor.HasValue && r.ScheduledFor.Value.Date == today)
            .OrderBy(r => r.ScheduledFor)
            .ToList();

        var weekAppts = all
            .Where(r => r.ScheduledFor.HasValue
                     && r.ScheduledFor.Value.Date >= weekStart
                     && r.ScheduledFor.Value.Date <= weekEnd)
            .ToList();

        var monthRevenue = all
            .Where(r => r.Status == ServiceStatus.Completed
                     && r.ScheduledFor.HasValue
                     && r.ScheduledFor.Value >= monthStart
                     && r.ScheduledFor.Value < monthEnd)
            .Sum(r => r.Price ?? 0);

        var vm = new AdminDashboardViewModel
        {
            TodayCount = todaySchedule.Count,
            WeekCount = weekAppts.Count,
            PendingCount = all.Count(r => r.Status == ServiceStatus.New),
            MonthRevenue = monthRevenue,
            TodaySchedule = todaySchedule,
            AllRequests = all,
            WeekAppointments = weekAppts,
            WeekStart = weekStart,
            WeekEnd = weekEnd
        };

        return View(vm);
    }

    // GET: /Admin/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var r = await _db.ServiceRequests.FindAsync(id);
        if (r is null) return NotFound();

        return View(MapToDetail(r));
    }

    // POST: /Admin/UpdateStatus/{id}  (form fallback used by Details.cshtml)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ServiceStatus status)
    {
        var r = await _db.ServiceRequests.FindAsync(id);
        if (r is null) return NotFound();
        r.Status = status;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Admin/UpdateNotes/{id}  (form fallback used by Details.cshtml)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateNotes(int id, string? notes)
    {
        var r = await _db.ServiceRequests.FindAsync(id);
        if (r is null) return NotFound();
        r.Notes = notes;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    // ─── JSON API endpoints ───────────────────────────────────────────────────

    // GET /Admin/api/appointment/{id}
    [HttpGet("Admin/api/appointment/{id:int}")]
    public async Task<IActionResult> ApiGetAppointment(int id)
    {
        var r = await _db.ServiceRequests.FindAsync(id);
        if (r is null) return NotFound();
        return Json(MapToDetail(r));
    }

    // POST /Admin/api/status
    [HttpPost("Admin/api/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApiUpdateStatus([FromBody] ApiStatusRequest req)
    {
        var r = await _db.ServiceRequests.FindAsync(req.Id);
        if (r is null) return NotFound();
        if (!Enum.IsDefined(typeof(ServiceStatus), req.Status))
            return BadRequest();
        r.Status = (ServiceStatus)req.Status;
        await _db.SaveChangesAsync();
        return Json(new { success = true, newStatus = r.Status.ToString(), newStatusInt = (int)r.Status });
    }

    // POST /Admin/api/notes
    [HttpPost("Admin/api/notes")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApiUpdateNotes([FromBody] ApiNotesRequest req)
    {
        var r = await _db.ServiceRequests.FindAsync(req.Id);
        if (r is null) return NotFound();
        r.Notes = req.Notes;
        await _db.SaveChangesAsync();
        return Json(new { success = true });
    }

    // POST /Admin/api/schedule
    [HttpPost("Admin/api/schedule")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApiUpdateSchedule([FromBody] ApiScheduleRequest req)
    {
        var r = await _db.ServiceRequests.FindAsync(req.Id);
        if (r is null) return NotFound();
        r.ScheduledFor = req.ScheduledFor;
        if (req.Price.HasValue) r.Price = req.Price;
        await _db.SaveChangesAsync();
        return Json(new { success = true });
    }

    // POST /Admin/api/quickcreate
    [HttpPost("Admin/api/quickcreate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApiQuickCreate([FromBody] QuickCreateViewModel vm)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

        var r = new ServiceRequest
        {
            CustomerName = vm.CustomerName,
            Phone = vm.Phone,
            Email = vm.Email ?? string.Empty,
            Address = vm.Address,
            PestType = vm.PestType,
            ScheduledFor = vm.ScheduledFor,
            Price = vm.Price,
            Description = vm.Description,
            Status = ServiceStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        _db.ServiceRequests.Add(r);
        await _db.SaveChangesAsync();

        return Json(new { success = true, id = r.Id, customerName = r.CustomerName });
    }

    // GET /Admin/api/calendar?weekStart=YYYY-MM-DD
    [HttpGet("Admin/api/calendar")]
    public async Task<IActionResult> ApiCalendar(string weekStart)
    {
        if (!DateTime.TryParse(weekStart, out var start))
            return BadRequest();

        start = start.Date;
        var end = start.AddDays(6);

        var appts = await _db.ServiceRequests
            .Where(r => r.ScheduledFor.HasValue
                     && r.ScheduledFor.Value.Date >= start
                     && r.ScheduledFor.Value.Date <= end)
            .ToListAsync();

        var vm = new CalendarWeekViewModel
        {
            WeekStart = start,
            WeekEnd = end,
            Appointments = appts.Select(r => new CalendarAppointmentDto
            {
                Id = r.Id,
                CustomerName = r.CustomerName,
                PestType = r.PestType.ToString().Replace("_", " "),
                ScheduledFor = r.ScheduledFor!.Value,
                StatusInt = (int)r.Status,
                Status = r.Status.ToString()
            })
        };

        return Json(vm);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static AppointmentDetailViewModel MapToDetail(ServiceRequest r) => new()
    {
        Id = r.Id,
        CustomerName = r.CustomerName,
        Phone = r.Phone,
        Email = r.Email,
        Address = r.Address,
        PestType = r.PestType,
        Description = r.Description,
        PreferredDate = r.PreferredDate,
        ScheduledFor = r.ScheduledFor,
        Price = r.Price,
        Status = r.Status,
        CreatedAt = r.CreatedAt,
        Notes = r.Notes
    };
}

// ─── Request DTOs ─────────────────────────────────────────────────────────────

public record ApiStatusRequest(int Id, int Status);
public record ApiNotesRequest(int Id, string? Notes);
public record ApiScheduleRequest(int Id, DateTime? ScheduledFor, decimal? Price);
