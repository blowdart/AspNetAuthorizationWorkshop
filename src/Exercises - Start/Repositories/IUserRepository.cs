using System.Security.Claims;

namespace AuthorizationLab.Repositories
{
    public interface IUserRepository
    {
        bool ValidateLogin(string userName, string password);
        ClaimsPrincipal Get(string userName);
    }
}
