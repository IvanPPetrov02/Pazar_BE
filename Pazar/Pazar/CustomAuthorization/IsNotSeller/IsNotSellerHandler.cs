using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using BLL.ManagerInterfaces;

namespace CustomAuthorization
{
    public class IsNotSellerHandler : AuthorizationHandler<IsNotSellerRequirement>
    {
        private readonly ILogger<IsNotSellerHandler> _logger;
        private readonly IItemManager _itemManager;

        public IsNotSellerHandler(ILogger<IsNotSellerHandler> logger, IItemManager itemManager)
        {
            _logger = logger;
            _itemManager = itemManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IsNotSellerRequirement requirement)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (context.Resource is AuthorizationFilterContext mvcContext)
            {
                mvcContext.HttpContext.Request.EnableBuffering();

                string body;
                using (var reader = new StreamReader(mvcContext.HttpContext.Request.Body, leaveOpen: true))
                {
                    body = await reader.ReadToEndAsync();
                    mvcContext.HttpContext.Request.Body.Position = 0;
                }

                var requestBody = System.Text.Json.JsonDocument.Parse(body);
                var itemId = requestBody.RootElement.GetProperty("itemSoldId").GetInt32();

                var item = await _itemManager.GetItemByIdAsync(itemId);

                if (item == null)
                {
                    _logger.LogDebug("IsNotSellerHandler: Item not found.");
                    context.Fail();
                    return;
                }

                if (item.Seller.UUID.ToString() == userId)
                {
                    _logger.LogDebug("IsNotSellerHandler: User is the seller of the item.");
                    return;
                }

                _logger.LogDebug("IsNotSellerHandler: User is not the seller of the item.");
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogDebug("IsNotSellerHandler: Invalid request context.");
            }

            await Task.CompletedTask;
        }
    }
}
