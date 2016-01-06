
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace AuthorizationLab.Controllers
{
    [Authorize(Policy = "AdministratorOnly")]
    [Authorize(Policy = "EmployeeId")]
    [Authorize(Policy = "Over21Only")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
