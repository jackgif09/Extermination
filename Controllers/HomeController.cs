using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Extermination.Models;

namespace Extermination.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()               => View();
    public IActionResult ServiceArea()         => View();
    public IActionResult Rates()               => View();
    public IActionResult CommercialLicenses()  => View();
    public IActionResult Reviews()             => View();
    public IActionResult Contact()             => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
