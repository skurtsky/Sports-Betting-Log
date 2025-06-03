using Microsoft.AspNetCore.Mvc;

namespace SportsBettingTracker.Extensions
{
    public static class ControllerExtensions
    {
        public static IActionResult Forbid(this Controller controller)
        {
            return controller.StatusCode(403, controller.View("Forbidden"));
        }
    }
}
