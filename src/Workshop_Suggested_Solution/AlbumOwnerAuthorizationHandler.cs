using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using AuthorizationWorkshop.Models;
using System.Threading.Tasks;

namespace AuthorizationWorkshop
{
    public class AlbumOwnerAuthorizationHandler : AuthorizationHandler<AlbumOwnerRequirement, Album>
    {
        private readonly ILogger<AlbumOwnerAuthorizationHandler> _logger;

        public AlbumOwnerAuthorizationHandler(ILogger<AlbumOwnerAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AlbumOwnerRequirement requirement, Album resource)
        {
            if (resource.Publisher == context.User.FindFirst(Constants.CompanyClaimType).Value)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
