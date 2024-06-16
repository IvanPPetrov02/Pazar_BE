using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CustomAuthorization
{
    public class IsTheUserOrAdminHandler : AuthorizationHandler<IsTheUserOrAdminRequirement>
    {
        private readonly ILogger<IsTheUserOrAdminHandler> _logger;

        public IsTheUserOrAdminHandler(ILogger<IsTheUserOrAdminHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsTheUserOrAdminRequirement requirement)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = context.User.FindAll(ClaimTypes.Role).Select(r => r.Value);
            if (context.Resource is Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext mvcContext)
            {
                var uuid = mvcContext.RouteData.Values["uuid"]?.ToString();
                _logger.LogDebug($"IsTheUserOrAdminHandler: User ID from token: {userId}, UUID from route: {uuid}");
                _logger.LogDebug($"IsTheUserOrAdminHandler: User roles: {string.Join(", ", roles)}");

                if (userId != null && (userId == uuid || roles.Contains("Admin")))
                {
                    _logger.LogDebug("IsTheUserOrAdminHandler: User is authorized.");
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogDebug("IsTheUserOrAdminHandler: User is not authorized.");
                }
            }
            return Task.CompletedTask;
        }
    }
}