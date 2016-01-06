using Microsoft.AspNet.Authorization;

namespace AuthorizationLab
{
    public class HasBadgeHandler : AuthorizationHandler<OfficeEntryRequirement>
    {
        protected override void Handle(AuthorizationContext context, OfficeEntryRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "BadgeNumber" && 
                                            c.Issuer == "https://contoso.com"))
            {
                return;
            }

            context.Succeed(requirement);
        }
    }
}
