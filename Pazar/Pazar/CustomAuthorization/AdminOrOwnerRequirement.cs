using Microsoft.AspNetCore.Authorization;

namespace CustomAuthorization
{
    public class AdminOrOwnerRequirement : IAuthorizationRequirement
    {
    }
}