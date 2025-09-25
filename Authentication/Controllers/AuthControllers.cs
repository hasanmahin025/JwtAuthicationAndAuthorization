using Microsoft.AspNetCore.Mvc;

namespace Authentication.Controllers;

public class AuthControllers : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}