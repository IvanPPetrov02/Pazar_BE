using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace CustomAuthorization
{
    public class IsTheUserOrAdminAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly ILogger<IsTheUserOrAdminAttribute> _logger;

        public IsTheUserOrAdminAttribute(ILogger<IsTheUserOrAdminAttribute> logger)
        {
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var authService = context.HttpContext.RequestServices.GetService<IAuthorizationService>();
            var user = context.HttpContext.User;

            var uuid = context.RouteData.Values["uuid"]?.ToString();
            _logger.LogDebug($"IsTheUserOrAdminAttribute: UUID from route: {uuid}");

            if (string.IsNullOrEmpty(uuid))
            {
                _logger.LogDebug("IsTheUserOrAdminAttribute: UUID is missing.");
                context.Result = new ForbidResult();
                return;
            }

            var requirement = new IsTheUserOrAdminRequirement();

            var authorizationResult = authService.AuthorizeAsync(user, context, requirement).Result;

            if (!authorizationResult.Succeeded)
            {
                _logger.LogDebug("IsTheUserOrAdminAttribute: Authorization failed.");
                context.Result = new ForbidResult();
            }
            else
            {
                _logger.LogDebug("IsTheUserOrAdminAttribute: Authorization succeeded.");
            }
        }
    }
}