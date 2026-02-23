using Extermination.Data;
using Extermination.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Extermination.Controllers;

public class ServiceRequestController : Controller
{
    private readonly AppDbContext _db;

    public ServiceRequestController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /ServiceRequest/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /ServiceRequest/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("CustomerName,Phone,Email,Address,PestType,Description,PreferredDate")] ServiceRequest model)
    {
        if (!ModelState.IsValid)
            return View(model);

        model.CreatedAt = DateTime.UtcNow;
        model.Status = ServiceStatus.New;

        _db.ServiceRequests.Add(model);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Confirmation), new { id = model.Id });
    }

    // GET: /ServiceRequest/Confirmation/{id}
    public async Task<IActionResult> Confirmation(int id)
    {
        var request = await _db.ServiceRequests.FindAsync(id);
        if (request is null)
            return NotFound();

        return View(request);
    }
}
