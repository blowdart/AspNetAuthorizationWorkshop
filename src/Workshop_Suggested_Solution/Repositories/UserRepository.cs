using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AuthorizationWorkshop.Models;

namespace AuthorizationWorkshop.Repositories
{
    public class UserRepository : IUserRepository
    {
        List<User> _users = new List<User>
        {
            new User { UserName = "barryd", Company = CompanyNames.PaddyProductions, Role = "Administrator" },
            new User { UserName = "davidfowl", Company = CompanyNames.ToneDeafRecords },
            new User { UserName = "dedward", Company = CompanyNames.ToneDeafRecords, Role = "Administrator" }
        };

        public ClaimsPrincipal Get(string userName)
        {
            var user = (_users.FirstOrDefault(u => u.UserName.ToUpperInvariant() == userName.ToUpperInvariant()));

            if (user == null)
            {
                return null;
            }

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, userName, ClaimValueTypes.String, Constants.Issuer));
            claims.Add(new Claim(Constants.CompanyClaimType, user.Company, ClaimValueTypes.String, Constants.Issuer));
            if (!string.IsNullOrWhiteSpace(user.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role, ClaimValueTypes.String, Constants.Issuer));
            }

            var userIdentity = new ClaimsIdentity("AuthorizationLab");
            userIdentity.AddClaims(claims);

            var userPrincipal = new ClaimsPrincipal(userIdentity);

            return userPrincipal;
        }

        public bool ValidateLogin(string userName, string password)
        {
            if (_users.Exists(u => u.UserName.ToUpperInvariant() == userName.ToUpperInvariant()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
