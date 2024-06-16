using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BLL.ManagerInterfaces;

namespace CustomAuthorization
{
    public class AdminOrOwnerHandler : AuthorizationHandler<AdminOrOwnerRequirement>
    {
        private readonly ILogger<AdminOrOwnerHandler> _logger;

        public AdminOrOwnerHandler(ILogger<AdminOrOwnerHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminOrOwnerRequirement requirement)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = context.User.FindAll(ClaimTypes.Role).Select(r => r.Value);

            _logger.LogDebug($"User ID: {userId}");
            _logger.LogDebug($"User Roles: {string.Join(", ", roles)}");

            // Check if user is admin
            if (roles.Contains("Admin"))
            {
                _logger.LogDebug("User is an admin.");
                context.Succeed(requirement);
                return;
            }

            // Check if user is the owner
            if (context.Resource is Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext mvcContext)
            {
                var itemId = mvcContext.RouteData.Values["id"]?.ToString();
                _logger.LogDebug($"Item ID from route: {itemId}");

                if (int.TryParse(itemId, out var id))
                {
                    var itemManager = mvcContext.HttpContext.RequestServices.GetService<IItemManager>();
                    var item = await itemManager.GetItemByIdAsync(id);

                    if (item != null)
                    {
                        _logger.LogDebug($"Item found: {item.Id}, Seller ID: {item.Seller.UUID}");
                        if (item.Seller.UUID.ToString() == userId)
                        {
                            _logger.LogDebug("User is the owner of the item.");
                            context.Succeed(requirement);
                            return;
                        }
                        else
                        {
                            _logger.LogDebug("User is not the owner of the item.");
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Item not found.");
                    }
                }
                else
                {
                    _logger.LogDebug("Invalid item ID.");
                }
            }

            _logger.LogDebug("Authorization failed.");
            context.Fail();
        }
    }
}
