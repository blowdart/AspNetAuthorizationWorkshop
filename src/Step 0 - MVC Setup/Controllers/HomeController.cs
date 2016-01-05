
using Microsoft.AspNet.Mvc;

namespace AuthorizationLab.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
