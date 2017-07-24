using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AuthorizationLab.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            const string Issuer = "https://contoso.com";
            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.Name, "barry", ClaimValueTypes.String, Issuer));
            claims.Add(new Claim(ClaimTypes.Role, "Administrator", ClaimValueTypes.String, Issuer));
            claims.Add(new Claim("EmployeeId", "123", ClaimValueTypes.String, Issuer));
            claims.Add(new Claim(ClaimTypes.DateOfBirth, "1970-06-08", ClaimValueTypes.Date));
            claims.Add(new Claim("BadgeNumber", "123456", ClaimValueTypes.String, Issuer));

            var userIdentity = new ClaimsIdentity("SuperSecureLogin");
            userIdentity.AddClaims(claims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                userPrincipal,
                new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
                    IsPersistent = false,
                    AllowRefresh = false
                });

            return RedirectToLocal(returnUrl);
        }

        public IActionResult Forbidden()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
