# ASP.NET v5 Authorization Lab

This is walk through for an ASP.NET Authorization Lab.

[Authorization Documentation](https://docs.asp.net/en/latest/security/authorization/index.html).

Step 0: Preparation
===================

Create a new, blank, ASP.NET project.
-------------------------------------

* File > New Project > ASP.NET Web Application. 
* Select the Empty ASP.NET 5 Template.

Add MVC to the app. 
-------------------

* Right click on the project, choose `Manage NuGet Packages`, click Browse, search for `Microsoft.AspNet.Mvc` and install v6.0.0-rc1-final
* Edit `Startup.cs`  and add `services.AddMvc();` to the top of ConfigureServices();
* Edit the `Configure()` method and add `app.UseStaticFiles()` below the `UseIISPlatformHandler()` call. This should prompt you to add the `Microsoft.AspNet.StaticFiles` package.
* Remove the `app.Run` section in `Configure();` and replace it with the following to setup default routing;

```c#
app.UseMvc(routes =>
{
     routes.MapRoute(
          name: "default",
          template: "{controller=Home}/{action=Index}/{id?}");
});
```

* Create a `Controllers` folder and a `Views` folder.
* Create a `HomeController.cs` file in the Controllers directory, using the VS Controller template, or create it from scratch and ensure it inherits from `Controller` and has an `Index()` method which returns a View, for example

```c#

using Microsoft.AspNet.Mvc;

namespace AuthorizationLab.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
```

* Add a `Home` folder inside the `Views` folder. Add an `Index.cshtml` file inside that folder, and edit it to say Hello World.

Step 1: Setup authorization
===========================

* Add the `Microsoft.AspNet.Authorization` nuget package.
* Add the `Microsoft.AspNet.Authentication.Cookies nuget` package
* Add `services.AddAuthorization()` at the top of the `ConfigureServices()` method.
* Edit the Home controller and add the `[Authorize]` attribute to the controller.
* Run the project and panic. You get a blank page. Open the IE Dev Tools, click Network and refresh and you will see you get a 401.
* Add cookie middleware into Configure()

```c#
app.UseCookieAuthentication(options =>
{
    options.AuthenticationScheme = "Cookie";
    options.LoginPath = new PathString("/Account/Unauthorized/");
    options.AccessDeniedPath = new PathString("/Account/Forbidden/");
    options.AutomaticAuthenticate = true;
    options.AutomaticChallenge = true;
});
```

* Now create an `Account` controller. Create an `Unauthorized()` action and a `Forbidden()` action.

```c#
using Microsoft.AspNet.Mvc;

namespace AuthorizationLab.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Unauthorized()
        {
            return View();
        }

        public IActionResult Forbidden()
        {
            return View();
        }
    }
}
```

* Create an `Account` folder in the `View` folder and create corresponding views for the actions. Add some text so you know the right one is being hit.
* Change the `Unauthorized` action to create a principal and persist it.

```c#
public async Task<IActionResult> Unauthorized(string returnUrl = null)
{
    const string Issuer = "https://contoso.com";
    List<Claim> claims = new List<Claim>();
    claims.Add(new Claim(ClaimTypes.Name, "barry", ClaimValueTypes.String, Issuer));
    var userIdentity = new ClaimsIdentity("SuperSecureLogin");
    userIdentity.AddClaims(claims);
    var userPrincipal = new ClaimsPrincipal(userIdentity);

    await HttpContext.Authentication.SignInAsync("Cookie", userPrincipal,
        new AuthenticationProperties
        {
            ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
            IsPersistent = false,
            AllowRefresh = false
        });

    return RedirectToLocal(returnUrl);
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
```

* Finally edit the Index view to display the name claim for the identity. 

```
@using System.Security.Claims;

@if (!User.Identities.Any(u => u.IsAuthenticated))
{
    <h1>Hello World</h1>
}
else
{
    <h1>Hello @User.Identities.First(u => u.IsAuthenticated && 
                                     u.HasClaim(c => c.Type == ClaimTypes.Name))
                                    .FindFirst(ClaimTypes.Name).Value</h1>
}
```

Step 2: Authorize all the things
================================

* First remove the `Authorize` attribute from the `Home` controller.
* Change `ConfigureServices()` in `startup.cs` to add a default policy to thee MVC configuration

```c#
services.AddMvc(config =>
{
    var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    config.Filters.Add(new AuthorizeFilter(policy));
});
```

* Run and watch everything blow up into an infinite redirect loop. Why? You've made every page require authentication, even the login pages. 
* Now add `[AllowAnonymous]` to the `Account` controller, run again and see the user is logged in.

Step 3: Roles
=============

* Go to the `Home` controller and add an `Authorize` attribute with a role demand;

```c#
[Authorize(Roles = "Administrator")]
```

* Run the application and confirm you are redirected to the `Forbidden` view.
* Return to the `AccountController` and add a Role claim to the issued identity.

```c#
claims.Add(new Claim(ClaimTypes.Role, "Administrator", ClaimValueTypes.String, Issuer));
```

* Run the application and confirm you are logged in.

Step 4: Simple Policies
=======================
* Return to startup.cs and locate the services.AddAuthorization() call.
* Add a policy to the configuration 

```c#
services.AddAuthorization(options =>
{
    options.AddPolicy("AdministratorOnly", policy => policy.RequireRole("Administrator"));
});
```

* Now change the Home controller `Authorize` attribute to require a policy, rather than use the role parameter.

```c#
[Authorize(Policy = "AdministratorOnly")]
```

* Run the app and confirm you still see the home page.
* Now add a second policy, this time requiring a claim.

```c#
services.AddAuthorization(options =>
{
    options.AddPolicy("AdministratorOnly", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("EmployeeId", policy => policy.RequireClaim("EmployeeId"));
});
```

* Add a suitable claim to the `Unauthorized` action in the `Account` Controller.

```c#
claims.Add(new Claim("EmployeeId", string.Empty, ClaimValueTypes.String, Issuer));
```

* Run the app and ensure you can see the home page.
* This is a rather useless check though. Claims have values. You really want to check the values and not just the presence of a claim. Luckily there’s a parameter for that. Change the EmployeeId policy to require one of a number of values;

```c#
options.AddPolicy("EmployeeId", policy => policy.RequireClaim("EmployeeId", "123", "456"));
```

* Run the app again and the empty claim will be rejected and end up at the Forbidden page. 
* Add a suitable claim value to the Unauthorized action and try again.

Step 5: Code Based Policies
===========================

Code based policies consist of a requirement, implementing `IAuthorizationRequirement` and a handler for the requirement, implementing `AuthorizationHandler<T>` where T is the requirement.

* Add a date of birth claim to the user principal in the Unauthorized action in the Account Controller

```c#
claims.Add(new Claim(ClaimTypes.DateOfBirth, "1970-06-08", ClaimValueTypes.Date));
```

* Now create a custom requirement and handler class; called MinimumAgeRequirement

```c#
using System;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;

namespace AuthorizationLab
{
    public class MinimumAgeRequirement : AuthorizationHandler<MinimumAgeRequirement>, IAuthorizationRequirement
    {
        int _minimumAge;

        public MinimumAgeRequirement(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        protected override void Handle(AuthorizationContext context, MinimumAgeRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == ClaimTypes.DateOfBirth))
            {
                return;
            }

            var dateOfBirth = Convert.ToDateTime(
                context.User.FindFirst(c => c.Type == ClaimTypes.DateOfBirth).Value);

            int calculatedAge = DateTime.Today.Year - dateOfBirth.Year;
            if (dateOfBirth > DateTime.Today.AddYears(-calculatedAge))
            {
                calculatedAge--;
            }

            if (calculatedAge >= _minimumAge)
            {
                context.Succeed(requirement);
            }
        }
    }
}
```

* Create an Over21 policy in the AddAuthorization function;

```c#
options.AddPolicy("Over21Only", policy => policy.Requirements.Add(new MinimumAgeRequirement(21)));
```

* Apply it to the `Home` controller using the `Authorize` attribute.
* Run the app and ensure you can see the home page.
* Experiment with the date of birth value to make authorization fail.