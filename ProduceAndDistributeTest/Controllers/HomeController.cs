using Microsoft.AspNetCore.Mvc;
using ProduceAndDistributeTest.Models;

namespace ProduceAndDistributeTest.Controllers
{
    public class HomeController : Controller
    {
        public Setup InputSetup = new Setup();
        public IActionResult Index()
        {
            return View(InputSetup);
        }
        [HttpPost]
        public IActionResult ValidateSetup(Setup userSetup)
        {
            if (!ModelState.IsValid)
            {
                ViewResult result = View("Index", userSetup);
                result.StatusCode = 409;
                return result;
            }
            return View("Index", userSetup);
        }
    }
}
