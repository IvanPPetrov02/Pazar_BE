using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BLL.ManagerInterfaces;

namespace CustomAuthorization
{
    public class IsSellerOrBuyerHandler : AuthorizationHandler<IsSellerOrBuyerRequirement>
    {
        private readonly ILogger<IsSellerOrBuyerHandler> _logger;
        private readonly IChatManager _chatManager;

        public IsSellerOrBuyerHandler(ILogger<IsSellerOrBuyerHandler> logger, IChatManager chatManager)
        {
            _logger = logger;
            _chatManager = chatManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IsSellerOrBuyerRequirement requirement)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (context.Resource is Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext mvcContext)
            {
                var chatId = mvcContext.RouteData.Values["chatId"]?.ToString();
                _logger.LogDebug($"IsSellerOrBuyerHandler: User ID from token: {userId}, Chat ID from route: {chatId}");

                if (!string.IsNullOrEmpty(chatId) && int.TryParse(chatId, out int id))
                {
                    var chat = await _chatManager.GetChatByIdAsync(id);
                    if (chat != null)
                    {
                        _logger.LogDebug($"IsSellerOrBuyerHandler: Chat found with ID: {chat.Id}, Seller ID: {chat.SellerId}, Buyer ID: {chat.BuyerId}");

                        if (userId == chat.SellerId || userId == chat.BuyerId)
                        {
                            _logger.LogDebug("IsSellerOrBuyerHandler: User is authorized as either seller or buyer.");
                            context.Succeed(requirement);
                        }
                        else
                        {
                            _logger.LogDebug("IsSellerOrBuyerHandler: User is not authorized as seller or buyer.");
                        }
                    }
                }
            }
            await Task.CompletedTask;
        }
    }
}
