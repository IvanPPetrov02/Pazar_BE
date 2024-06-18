using Microsoft.AspNetCore.Authorization;

namespace CustomAuthorization
{
    public class IsNotSellerRequirement : IAuthorizationRequirement
    {
    }
}
