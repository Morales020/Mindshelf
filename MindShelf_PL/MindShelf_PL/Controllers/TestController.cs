using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MindShelf_PL.Controllers
{
    public class TestController : Controller
    {
        // This action requires authentication
        [Authorize]
        public IActionResult ProtectedAction()
        {
            return View();
        }

        // This action is public
        public IActionResult PublicAction()
        {
            return View();
        }

        // This action requires specific role (if you want to test role-based authorization)
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnlyAction()
        {
            return View();
        }
    }
}
