using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using portfolio.Models;

namespace portfolio.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? code)
    {
        ViewBag.Code = code ?? 500;
        ViewBag.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        Response.StatusCode = code ?? 500;
        return View();
    }
}