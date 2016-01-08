using AuthorizationLab.Models;
using Microsoft.AspNet.Authorization;

namespace AuthorizationLab
{
    public class AlbumOwnerAuthorizationHandler : AuthorizationHandler<AlbumOwnerRequirement, Album>
    {
        protected override void Handle(AuthorizationContext context, AlbumOwnerRequirement requirement, Album resource)
        {
            if (resource.Publisher == context.User.FindFirst(Constants.CompanyClaimType).Value)
            {
                context.Succeed(requirement);
            }
        }
    }
}
