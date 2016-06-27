using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace AuthorizationLab
{
    public class HasBadgeHandler : AuthorizationHandler<OfficeEntryRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OfficeEntryRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "BadgeNumber" &&
                                            c.Issuer == "https://contoso.com"))
            {
                return Task.FromResult(0);
            }

            context.Succeed(requirement);
            return Task.FromResult(0);
        }
    }
}