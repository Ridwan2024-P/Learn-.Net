using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SignalRMvcDemo.Models;

namespace SignalRMvcDemo.Controllers;

public class HomeController : Controller
{
    
    public IActionResult Index()
    {
        return View();
    }
}
