
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace AuthorizationLab.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
