using Microsoft.AspNetCore.Mvc;

namespace HR_Payroll.Web.Controllers
{
    public class HRController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
