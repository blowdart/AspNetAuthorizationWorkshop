# ASP.NET v5 Authorization Lab

This is walk through for an ASP.NET Authorization Lab.

[Authorization Documentation](https://docs.asp.net/en/latest/security/authorization/index.html).

*Tip: When you stop the app always close the browser to clear the identity cookie.*

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

Step 6: Multiple handlers for a requirement
===========================================

Sometimes you may want multiple handlers for an Authorization Requirement, 
for example when there are multiple ways to fulfill a requirement. Microsoft's office doors 
open with your Microsoft badge, however on days you forget your badge you can go to 
reception and get a temporary pass and the receptionist will let you through the gates. 
This would be implemented as two handlers for a single requirement.

* First, write a new `IAuthorizationRequirement`, `OfficeEntryRequirement`

```c#
using Microsoft.AspNet.Authorization;

namespace AuthorizationLab
{
    public class OfficeEntryRequirement : IAuthorizationRequirement
    {
    }
}
```

* Now write an `AuthorizationHandler` that checks if the current identity has a badge number claim, 
issued by the employer, `HasBadgeHandler`

```c#
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
```

That takes care of people who remembered their badges. But what about those who forget and have 
a temporary badge? You could just put it all in one handler, but handlers and requirements are 
meant to be reusable. You could use the `HasBadgeHandler` above for other things, not just office entry 
(for example our code signing infrastructure needs the smartcard that is our badge to trigger jobs)

* To cope with temporary badges write another `AuthorizationHandler`, `HasTemporaryPassHandler`

```c#
using System;
using Microsoft.AspNet.Authorization;

namespace AuthorizationLab
{
    public class HasTemporaryPassHandler : AuthorizationHandler<OfficeEntryRequirement>
    {
        protected override void Handle(AuthorizationContext context, OfficeEntryRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "TemporaryBadgeExpiry" &&
                                            c.Issuer == "https://contoso.com"))
            {
                return;
            }

            var temporaryBadgeExpiry = 
                Convert.ToDateTime(context.User.FindFirst(
                                       c => c.Type == "TemporaryBadgeExpiry" &&
                                       c.Issuer == "https://contoso.com").Value);

            if (temporaryBadgeExpiry > DateTime.Now)
            {
                context.Succeed(requirement);
            }
        }
    }
}
```

* Next create a policy for the requirement, registering it in the `ConfigureServices()` in `startup.cs`, inside the authorization configuration

```c#
options.AddPolicy("BuildingEntry", policy => policy.Requirements.Add(new OfficeEntryRequirement()));
```

* Go back to the `Account` controller `Unauthorized` method and add a suitable badge ID claim.

```c#
claims.Add(new Claim("BadgeNumber", "123456", ClaimValueTypes.String, Issuer));
```

* Finally, apply the policy created to the `Index` view in the `Home` controller using the `Authorize` attribute.

```c#
[Authorize(Policy = "BuildingEntry")]
```

* Run the app and, oh dear, we get bounced to forbidden. Why?

Handlers are held in the ASP.NET DI container. In our previous sample we combined the requirement and the handler in one class, so the authorization system knew about it. Now we have separate handlers we need to register them in the DI container before they can be found.

* Open `startup.cs`, and inside `ConfigureServices()` register the handlers in the DI container.

```c#
services.AddSingleton<IAuthorizationHandler, HasBadgeHandler>();
services.AddSingleton<IAuthorizationHandler, HasTemporaryPassHandler>();
```

* Run the app again and you can see authorization works. 
* Experiment with commenting out the BadgeNumber claim and replacing it with a TemporaryBadgeExpiry claim

```c#
claims.Add(new Claim("TemporaryBadgeExpiry", 
	                 DateTime.Now.AddDays(1).ToString(), 
	                 ClaimValueTypes.String, 
	                 Issuer));
```

* Run the app, and you’re still authorized.
* Change the temporary badge claim so it has expired; 

```c#
claims.Add(new Claim("TemporaryBadgeExpiry", 
	                 DateTime.Now.AddDays(-1).ToString(), 
	                 ClaimValueTypes.String, 
	                 Issuer));
```

* Rerun the app and you’ll see you’re forbidden.
* Remove the temporary badge claim and uncomment the badgenumber claim.

Step 7: Resource Based Requirements
===================================

So far we’ve covered requirements that are based only on a user’s identity. However often authorization requires the resource being accessed. For example a Document class may have an author and only authors can edit the document, whilst others can view it.

* Create a resource class, `Document` with an int ID property and a string Author property.

```c#
namespace AuthorizationLab
{
    public class Document
    {
        public int Id { get; set; }
        public string Author { get; set; }
    }
}
```

* Create a repository interface for the Document class, `IDocumentRepository`

```c#
using System.Collections.Generic;

namespace AuthorizationLab
{
    public interface IDocumentRepository
    {
        IEnumerable<Document> Get();

        Document Get(int id);
    }
}
```

Create an implementation of the repository, with some test documents, `DocumentRepository.cs`

```c#
using System.Collections.Generic;
using System.Linq;

namespace AuthorizationLab
{
    public class DocumentRepository : IDocumentRepository
    {
        static List<Document> _documents = new List<Document> {
            new Document { Id = 1, Author = "barry" },
            new Document { Id = 2, Author = "someoneelse" }
        };

        public IEnumerable<Document> Get()
        {
            return _documents;
        }

        public Document Get(int id)
        {
            return (_documents.FirstOrDefault(d => d.Id == id));
        }
    }
}
```

*Finally register the document repository in the services collection through the ConfigureServices() method in startup.cs
services.AddSingleton<IDocumentRepository, DocumentRepository>();

Now we can create a suitable controller and views to display a list of documents and the document itself.

* First create a Document controller in the Controllers folder, DocumentController.cs

```c#
using Microsoft.AspNet.Mvc;

namespace AuthorizationLab.Controllers
{
    public class DocumentController : Controller
    {
        IDocumentRepository _documentRepository;

        public DocumentController(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        public IActionResult Index()
        {
            return View(_documentRepository.Get());
        }

        public IActionResult Edit(int id)
        {
            var document = _documentRepository.Get(id);

            if (document == null)
            {
                return new HttpNotFoundResult();
            }

            return View(document);
        }
    }
}
```

* Create an Document folder underneath the views folder and create Index view, index.cshtml

```
@using AuthorizationLab
@model IEnumerable<Document>

<h1>Document Library</h1>
@foreach (var document in Model)
{
    <p>
        @Html.ActionLink("Document #"+document.Id, "Edit",  new { id = document.Id })
    </p>
}
```

* Create an Edit view in the Document view folder, Edit.cshtml

```
@using AuthorizationLab
@model Document

<h1>Document #@Model.Id</h1>
<h2>Author: @Model.Author</h2>
```

* Run the app and load the /Document URL. Ensure you see a list of documents and you can click into each one.

Now we need to define operations to authorize against. For a document this might be Read, Write, Edit and Delete. We provide a base class, OperationAuthorizationRequirement which you can use as a starting point, but it’s optional.

* Define an requirement for editing, `EditRequirement.cs`

```c#
using Microsoft.AspNet.Authorization;

namespace AuthorizationLab
{
    public class EditRequirement : IAuthorizationRequirement
    {
    }
}
```

Now, as before, we write a handler, but this time we write a handler which takes a resource. 

* Create a DocumentEditHandler, `DocumentEditHandler.cs`. This time specify a resource type as well as the requirement in the class definition.

``` c#
using System.Security.Claims;
using Microsoft.AspNet.Authorization;

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
```

* Finally register the handler in `ConfigureServices()` in `Startup.cs`

```c#
services.AddSingleton<IAuthorizationHandler, DocumentEditHandler>();
```

We cannot use resource handlers in attributes, because binding hasn’t happened at that point and we need the resource. So we must call the authorization service directly.

* Return to the Document controller and edit the constructor to include IAuthorizationService as one of its parameters and store it in a local variable.

```c#
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace AuthorizationLab.Controllers
{
    public class DocumentController : Controller
    {
        IDocumentRepository _documentRepository;
        IAuthorizationService _authorizationService;

        public DocumentController(IDocumentRepository documentRepository, 
                                  IAuthorizationService authorizationService)
        {
            _documentRepository = documentRepository;
            _authorizationService = authorizationService;
        }

        public IActionResult Index()
        {
            return View(_documentRepository.Get());
        }

        public IActionResult Edit(int id)
        {
            var document = _documentRepository.Get(id);

            if (document == null)
            {
                return new HttpNotFoundResult();
            }

            return View(document);
        }
    }
}
```

Finally we can call the service. 

* In the Edit action change it to be async, returning a `Task<IActionResult>` and call the `_authorizeService.AuthorizeAsync` method with the user, resource and the requirement. If the authorization call fails you should return a `ChallengeResult();`

```c#
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace AuthorizationLab.Controllers
{
    public class DocumentController : Controller
    {
        IDocumentRepository _documentRepository;
        IAuthorizationService _authorizationService;

        public DocumentController(IDocumentRepository documentRepository, 
                                  IAuthorizationService authorizationService)
        {
            _documentRepository = documentRepository;
            _authorizationService = authorizationService;
        }

        public IActionResult Index()
        {
            return View(_documentRepository.Get());
        }

        public async Task<IActionResult> Edit(int id)
        {
            var document = _documentRepository.Get(id);

            if (document == null)
            {
                return new HttpNotFoundResult();
            }

            if (await _authorizationService.AuthorizeAsync(User, document, new EditRequirement()))
            {
                return View(document);
            }
            else
            {
                return new ChallengeResult();
            }
        }
    }
}
```

* Run the app and go to the Document URL. You should be able to click through and see Document 1 but not Document 2.

Step 8: Authorizing in Views
============================

For resource links and other UI elements you probably want to not show those to users in the UI (but you still want to keep authorization checks in the Controller – never rely solely on UI element removal as a security mechanism). ASP.NET 5 allows DI within views, so you can use the same approach in Step 7 to hide documents in the document list the current user cannot access.

* Open the Index view file, `index.cshtml` in the `Documents` folder. 
* Add an @using statement for `Microsoft.AspNet.Authorization` and inject the `AuthorizationService` using the `@inject` command

```
@using Microsoft.AspNet.Authorization
@using AuthorizationLab
@model IEnumerable<Document>
@inject IAuthorizationService AuthorizationService

<h1>Document Library</h1>
@foreach (var document in Model)
{
    <p>
        @Html.ActionLink("Document #"+document.Id, "Edit",  new { id = document.Id })
    </p>
}
```

* Now within the `foreach` loop in the view you can call the `AuthorizationService` in the same way you did with a controller.

```
@using Microsoft.AspNet.Authorization
@using AuthorizationLab

@model IEnumerable<Document>
@inject IAuthorizationService AuthorizationService

<h1>Document Library</h1>
@foreach (var document in Model)
{
    <p>
        @if (await AuthorizationService.AuthorizeAsync(User, document, new EditRequirement()))
        {
            @Html.ActionLink("Document #" + document.Id, "Edit", new { id = document.Id })
        }
    </p>
}
```

* Run the app and browser to the Document URL, and you will see that you now only have a link to Document #1.

Applying what you've learned
============================

Open the `Exercises - Start` folder. 
	
This is a sample web site for inventory control. The site allows record label employees to update the details of albums.

There are 3 users, barryd, davidfowl and dedwards. barryd is an administrator for Paddy Productions. dewards is an administrator for ToneDeaf Records. davidfowl is an employee of ToneDeaf Records, but not an administrator. Administrators are part of the Administrator role.

A User repository has been provided for you and is injected into the `Account` controller. You should use the `ValidateLogin()` function to first check if the login is correct, then retrieve a suitable user principal using the `Get()` method. 

The cookie authentication middleware is already configured, the instance name is available from the `Constants.MiddlewareScheme` field.

Change the site to include the following functionality

1. Change the `AccountController` `Login` action to create a cookie for the user logging in using the already configured cookie middleware.
2. Make the entire site require a login, excluding the `Login` action in the `AccountController`.
3. Make the `Edit` action in the `HomeController` only available to Administrators for any company.
4. Make the Edit functionality only available to Administrators for the company that has issued the album.
5. Change the `Index` action in the `HomeController` so it only lists albums from the company the current user belongs to and the edit link is only shown for administrators.

A sample solution is containing in the `Exercises - Suggest Solution` folder.
