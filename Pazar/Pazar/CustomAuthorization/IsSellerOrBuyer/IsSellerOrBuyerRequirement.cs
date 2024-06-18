using Microsoft.AspNetCore.Authorization;

namespace CustomAuthorization
{
    public class IsSellerOrBuyerRequirement : IAuthorizationRequirement
    {
    }
}