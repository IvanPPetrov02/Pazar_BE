using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CustomAuthorization
{
    public class IsNotSellerAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        private readonly ILogger<IsNotSellerAttribute> _logger;

        public IsNotSellerAttribute(ILogger<IsNotSellerAttribute> logger)
        {
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
            var user = context.HttpContext.User;

            var requirement = new IsNotSellerRequirement();

            var authorizationResult = await authService.AuthorizeAsync(user, context, requirement);

            if (!authorizationResult.Succeeded)
            {
                _logger.LogDebug("IsNotSellerAttribute: Authorization failed.");
                context.Result = new ForbidResult();
            }
            else
            {
                _logger.LogDebug("IsNotSellerAttribute: Authorization succeeded.");
            }
        }
    }
}