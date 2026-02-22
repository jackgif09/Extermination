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
        var requests = await _db.ServiceRequests
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var vm = new AdminIndexViewModel
        {
            TotalCount = requests.Count,
            NewCount = requests.Count(r => r.Status == ServiceStatus.New),
            ScheduledCount = requests.Count(r => r.Status == ServiceStatus.Scheduled),
            CompletedCount = requests.Count(r => r.Status == ServiceStatus.Completed),
            Requests = requests
        };

        return View(vm);
    }

    // GET: /Admin/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var request = await _db.ServiceRequests.FindAsync(id);
        if (request is null)
            return NotFound();

        return View(request);
    }

    // POST: /Admin/UpdateStatus/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ServiceStatus status)
    {
        var request = await _db.ServiceRequests.FindAsync(id);
        if (request is null)
            return NotFound();

        request.Status = status;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }
}
