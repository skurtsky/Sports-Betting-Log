using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SportsBettingTracker.Models;

namespace SportsBettingTracker.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        else
        {
            // Show the welcome page when not logged in
            return View();
        }
    }public IActionResult Privacy()
    {
        return View();
    }
    
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult Settings()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
