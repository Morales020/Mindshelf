using Microsoft.AspNetCore.Mvc;

namespace MindShelf_PL.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/Unauthorized")]
        public new IActionResult Unauthorized()
        {
            return View("~/Views/Shared/Unauthorized.cshtml");
        }

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 401:
                    return RedirectToAction("Unauthorized");
                case 403:
                    return RedirectToAction("Unauthorized");
                case 404:
                    return View("NotFound");
                default:
                    return View("Error");
            }
        }

        [Route("Error")]
        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
