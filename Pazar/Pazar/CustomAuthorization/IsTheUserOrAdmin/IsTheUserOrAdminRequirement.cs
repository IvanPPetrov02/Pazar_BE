using Microsoft.AspNetCore.Authorization;

namespace CustomAuthorization
{
    public class IsTheUserOrAdminRequirement : IAuthorizationRequirement
    {
    }
}