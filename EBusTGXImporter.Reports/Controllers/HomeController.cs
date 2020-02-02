using EBusTGXImporter.Reports.Helper;
using EBusTGXImporter.Reports.Models;
using System.Web.Mvc;

namespace EBusTGXImporter.Reports.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Login()
        {
            ViewBag.Page = "Login";
            LoginModel model = new LoginModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            model.IsLoggedIn = false;
            if (model.UserName == Configuration.ADMIN_USER_NAME && model.Password == Configuration.ADMIN_PASSWORD)
            {
                model.IsLoggedIn = true;
                return RedirectToAction("SmartCardTransaction", "Reports");
            }
            return View(model);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
