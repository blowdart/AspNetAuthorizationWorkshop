
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationLab.Controllers
{
    [Authorize(Policy = "AdministratorOnly")]
    [Authorize(Policy = "EmployeeId")]
    [Authorize(Policy = "Over21Only")]
    [Authorize(Policy = "BuildingEntry")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
