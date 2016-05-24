using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using AuthorizationWorkshop.Models;

namespace AuthorizationWorkshop
{
    public class AlbumOwnerAuthorizationHandler : AuthorizationHandler<AlbumOwnerRequirement, Album>
    {
        private readonly ILogger<AlbumOwnerAuthorizationHandler> _logger;

        public AlbumOwnerAuthorizationHandler(ILogger<AlbumOwnerAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override void Handle(AuthorizationContext context, AlbumOwnerRequirement requirement, Album resource)
        {
            if (resource.Publisher == context.User.FindFirst(Constants.CompanyClaimType).Value)
            {
                context.Succeed(requirement);
            }
        }
    }
}
