using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;
using BLL.ManagerInterfaces;
using System.Security.Claims;

namespace CustomAuthorization
{
    public class IsSellerOrBuyerAttribute : Attribute, IAsyncActionFilter
    {
        private readonly IChatManager _chatManager;

        public IsSellerOrBuyerAttribute(IChatManager chatManager)
        {
            _chatManager = chatManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userId = userIdClaim.Value;

            // Extract chatId from route values
            if (!context.ActionArguments.TryGetValue("id", out var idValue) &&
                !context.ActionArguments.TryGetValue("chatId", out idValue))
            {
                context.Result = new BadRequestObjectResult(new { message = "Chat ID not provided." });
                return;
            }

            if (!(idValue is int chatId))
            {
                context.Result = new BadRequestObjectResult(new { message = "Invalid Chat ID." });
                return;
            }

            try
            {
                var chat = await _chatManager.GetChatByIdAsync(chatId);

                if (chat == null)
                {
                    context.Result = new NotFoundObjectResult(new { message = "Chat not found." });
                    return;
                }

                if (chat.BuyerId != userId && chat.SellerId != userId)
                {
                    context.Result = new ForbidResult();
                    return;
                }

                await next();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authorization error for Chat ID: {chatId}, Error: {ex.Message}");
                context.Result = new StatusCodeResult(500);
            }
        }
    }
}
