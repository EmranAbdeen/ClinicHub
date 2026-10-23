using ClinicHub.Models.Clinic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ClinicHub.Filters
{
    public class AuthFilter : ActionFilterAttribute
    {
        public string[]? Roles { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;

            var userId = httpContext.Session.GetInt32("UserId");

            var isApiRequest = httpContext.Request.Path.StartsWithSegments("/api");

            if (userId == null)
            {
                if (isApiRequest)
                {
                    context.Result = new UnauthorizedObjectResult(new
                    {
                        message = "Please login first."
                    });
                }
                else
                {
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                }
                return;
            }

            if (Roles != null && Roles.Length > 0)
            {
                var userRole = httpContext.Session.GetString("UserRole");

                if (string.IsNullOrEmpty(userRole) || !Roles.Contains(userRole))
                {
                    if (isApiRequest)
                    {
                        context.Result = new ObjectResult(new
                        {
                            message = "You do not have permission to access this resource."
                        })
                        {
                            StatusCode = StatusCodes.Status403Forbidden
                        };
                    }
                    else
                    {
                        context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                    }

                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
