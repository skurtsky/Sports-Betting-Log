using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SportsBettingTracker.Models;
using System;
using System.Threading.Tasks;

namespace SportsBettingTracker.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class DemoUserRestrictionAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userManager = context.HttpContext.RequestServices
                .GetService(typeof(UserManager<ApplicationUser>)) as UserManager<ApplicationUser>;
            
            if (userManager == null)
            {
                await next();
                return;
            }
            
            var user = await userManager.GetUserAsync(context.HttpContext.User);
              if (user != null && user.IsDemoUser)
            {
                // For GET requests, allow access but add a warning message
                if (context.HttpContext.Request.Method == "GET")
                {
                    // Set Items that can be accessed in the view
                    context.HttpContext.Items["IsDemoUser"] = true;
                    await next();
                    return;
                }
                  // For POST requests that would modify data, redirect to demo restricted page
                context.Result = new RedirectToActionResult("DemoRestricted", "Account", null);
                return;
            }
            
            await next();
        }
    }
}
