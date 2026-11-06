using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignalRMvcDemo.Models;

namespace SignalRMvcDemo.Controllers;

[Authorize]
public class HomeController : Controller
{
    
    public IActionResult Index()
    {
        return View();
    }
}
