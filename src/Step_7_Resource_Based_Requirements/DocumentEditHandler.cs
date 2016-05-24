using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace AuthorizationLab
{
    public class DocumentEditHandler : AuthorizationHandler<EditRequirement, Document>
    {
        protected override void Handle(AuthorizationContext context,
                                       EditRequirement requirement,
                                       Document resource)
        {
            if (resource.Author == context.User.FindFirst(ClaimTypes.Name).Value)
            {
                context.Succeed(requirement);
            }
        }
    }
}