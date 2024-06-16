using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CustomAuthorization
{
    public class AdminOrOwnerAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly ILogger<AdminOrOwnerAttribute> _logger;

        public AdminOrOwnerAttribute(ILogger<AdminOrOwnerAttribute> logger)
        {
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var authService = context.HttpContext.RequestServices.GetService<IAuthorizationService>();
            var user = context.HttpContext.User;

            var itemId = context.RouteData.Values["id"]?.ToString();
            _logger.LogDebug($"AdminOrOwnerAttribute: Item ID from route: {itemId}");

            if (string.IsNullOrEmpty(itemId))
            {
                _logger.LogDebug("AdminOrOwnerAttribute: Item ID is missing.");
                context.Result = new ForbidResult();
                return;
            }

            var requirement = new AdminOrOwnerRequirement();

            var authorizationResult = authService.AuthorizeAsync(user, context, requirement).Result;

            if (!authorizationResult.Succeeded)
            {
                _logger.LogDebug("AdminOrOwnerAttribute: Authorization failed.");
                context.Result = new ForbidResult();
            }
            else
            {
                _logger.LogDebug("AdminOrOwnerAttribute: Authorization succeeded.");
            }
        }
    }
}