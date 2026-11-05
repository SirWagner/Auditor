using Auditor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Auditor.Controllers
{
    public class HomeController : Controller
    {
        // Old links (/Home, /Template/Dashboard) land on the dashboard.
        [HttpGet("/Home")]
        [HttpGet("/Home/Index")]
        [HttpGet("/Template/Dashboard")]
        public IActionResult Index() => RedirectToAction("Index", "Dashboard");

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("/error")]
        public IActionResult Error(int? statusCode = null)
        {
            ViewBag.StatusCode = statusCode;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
