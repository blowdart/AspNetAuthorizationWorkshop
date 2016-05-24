using System.Security.Claims;

namespace AuthorizationWorkshop.Repositories
{
    public interface IUserRepository
    {
        bool ValidateLogin(string userName, string password);
        ClaimsPrincipal Get(string userName);
    }
}
