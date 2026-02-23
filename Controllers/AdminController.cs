using Extermination.Data;
using Extermination.Models;
using Extermination.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Extermination.Controllers;

[Authorize]
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
        var now = DateTime.Now;

        var all = await _db.ServiceRequests
            .OrderBy(r => r.ScheduledFor)
            .ThenBy(r => r.CreatedAt)
            .ToListAsync();

        // Today: has ScheduledFor on today's date (regardless of status)
        var todayList = all
            .Where(r => r.ScheduledFor.HasValue && r.ScheduledFor.Value.Date == today)
            .OrderBy(r => r.ScheduledFor)
            .ToList();

        // Past: completed/cancelled, OR scheduled before today
        var pastList = all
            .Where(r => !(r.ScheduledFor.HasValue && r.ScheduledFor.Value.Date == today))
            .Where(r => r.Status == ServiceStatus.Completed
                     || r.Status == ServiceStatus.Cancelled
                     || (r.ScheduledFor.HasValue && r.ScheduledFor.Value.Date < today))
            .OrderByDescending(r => r.ScheduledFor ?? r.CreatedAt)
            .ToList();

        // Upcoming: not today, not past (future scheduled + unscheduled active requests)
        var pastIds = pastList.Select(r => r.Id).ToHashSet();
        var upcomingList = all
            .Where(r => !(r.ScheduledFor.HasValue && r.ScheduledFor.Value.Date == today))
            .Where(r => !pastIds.Contains(r.Id))
            .OrderBy(r => r.ScheduledFor ?? DateTime.MaxValue)
            .ThenBy(r => r.CreatedAt)
            .ToList();

        // Next appointment: next today appointment after now, or first upcoming
        var nextAppt = todayList.FirstOrDefault(r => r.ScheduledFor.HasValue && r.ScheduledFor.Value > now)
                    ?? upcomingList.FirstOrDefault(r => r.ScheduledFor.HasValue);

        var monthStart = new DateTime(today.Year, today.Month, 1);

        var vm = new AdminDashboardViewModel
        {
            TodaySchedule = todayList,
            UpcomingRequests = upcomingList,
            PastRequests = pastList,
            NextAppointment = nextAppt,
            TodayNewCount = todayList.Count(r => r.Status == ServiceStatus.New),
            UpcomingNewCount = upcomingList.Count(r => r.Status == ServiceStatus.New),
            NewRequestsCount        = all.Count(r => r.Status == ServiceStatus.New),
            BookedCount             = upcomingList.Count(r => r.ScheduledFor.HasValue),
            CompletedThisMonthCount = all.Count(r => r.Status == ServiceStatus.Completed
                                                  && r.CreatedAt >= monthStart),
            TotalRevenue            = all.Where(r => r.Price.HasValue).Sum(r => r.Price!.Value)
        };

        return View(vm);
    }

    // GET: /Admin/Create
    public IActionResult Create()
    {
        return View(new QuickCreateViewModel());
    }

    // POST: /Admin/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuickCreateViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

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

        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var r = await _db.ServiceRequests.FindAsync(id);
        if (r is null) return NotFound();
        return View(MapToDetail(r));
    }

    // POST: /Admin/UpdateStatus/{id}
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

    // POST: /Admin/UpdateNotes/{id}
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

    [HttpGet("Admin/api/appointment/{id:int}")]
    public async Task<IActionResult> ApiGetAppointment(int id)
    {
        var r = await _db.ServiceRequests.FindAsync(id);
        if (r is null) return NotFound();
        return Json(MapToDetail(r));
    }

    [HttpPost("Admin/api/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApiUpdateStatus([FromBody] ApiStatusRequest req)
    {
        var r = await _db.ServiceRequests.FindAsync(req.Id);
        if (r is null) return NotFound();
        if (!Enum.IsDefined(typeof(ServiceStatus), req.Status)) return BadRequest();
        r.Status = (ServiceStatus)req.Status;
        await _db.SaveChangesAsync();
        return Json(new { success = true, newStatus = r.Status.ToString(), newStatusInt = (int)r.Status });
    }

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

    [HttpPost("Admin/api/quickcreate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApiQuickCreate([FromBody] QuickCreateViewModel vm)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false });

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

    [HttpGet("Admin/api/calendar")]
    public async Task<IActionResult> ApiCalendar(string weekStart)
    {
        if (!DateTime.TryParse(weekStart, out var start)) return BadRequest();
        start = start.Date;
        var end = start.AddDays(6);
        var appts = await _db.ServiceRequests
            .Where(r => r.ScheduledFor.HasValue
                     && r.ScheduledFor.Value.Date >= start
                     && r.ScheduledFor.Value.Date <= end)
            .ToListAsync();
        return Json(new CalendarWeekViewModel
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
        });
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

public record ApiStatusRequest(int Id, int Status);
public record ApiNotesRequest(int Id, string? Notes);
public record ApiScheduleRequest(int Id, DateTime? ScheduledFor, decimal? Price);
