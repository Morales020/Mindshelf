using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MindShelf_PL.Handlers
{
    public class CustomAuthorizationHandler : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if the user is authenticated
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                // If not authenticated, redirect to login
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            // Check if the user has the required authorization
            var authorizeAttribute = context.ActionDescriptor.EndpointMetadata
                .OfType<AuthorizeAttribute>()
                .FirstOrDefault();

            if (authorizeAttribute != null)
            {
                // If user is authenticated but not authorized, redirect to unauthorized page
                context.Result = new RedirectToActionResult("Unauthorized", "Error", null);
                return;
            }
        }
    }
}
