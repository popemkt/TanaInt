using Microsoft.AspNetCore.Mvc;

namespace TanaInt.Api.Controllers;

public class HomeController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}