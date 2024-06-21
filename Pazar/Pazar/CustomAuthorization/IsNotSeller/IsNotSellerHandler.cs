using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using BLL.ManagerInterfaces;
using System.IO;
using System.Text.Json;

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

    if (string.IsNullOrEmpty(userId))
    {
        _logger.LogDebug("IsNotSellerHandler: User ID claim is missing.");
        context.Fail();
        return;
    }

    if (context.Resource is AuthorizationFilterContext mvcContext)
    {
        _logger.LogDebug("IsNotSellerHandler: AuthorizationFilterContext detected.");
        mvcContext.HttpContext.Request.EnableBuffering();

        string body;
        using (var reader = new StreamReader(mvcContext.HttpContext.Request.Body, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            mvcContext.HttpContext.Request.Body.Position = 0;
        }

        _logger.LogDebug($"IsNotSellerHandler: Request Body: {body}");

        try
        {
            var requestBody = JsonDocument.Parse(body);
            if (requestBody.RootElement.TryGetProperty("itemSoldId", out JsonElement itemSoldIdElement))
            {
                var itemId = itemSoldIdElement.GetInt32();
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
                    context.Fail();
                    return;
                }

                _logger.LogDebug("IsNotSellerHandler: User is not the seller of the item.");
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogDebug("IsNotSellerHandler: itemSoldId not found in the request body.");
                context.Fail();
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "IsNotSellerHandler: JSON parsing error.");
            context.Fail();
        }
    }
    else
    {
        _logger.LogDebug("IsNotSellerHandler: Invalid request context.");
        context.Fail();
    }

    await Task.CompletedTask;
}

    }
}
