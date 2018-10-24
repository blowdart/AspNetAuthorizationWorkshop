# ASP.NET Core Authorization Lab

This is walk through for an ASP.NET Core Authorization Lab, now updated for ASP.NET Core 2.1 and VS2017. (If you're still using 1.x then the older version of the labs are available in the [Core1x](https://github.com/blowdart/AspNetAuthorizationWorkshop/tree/Core1x) branch.)

This lab uses the Model-View-Controller template as that's what everyone has been using up until now and it's the most familiar starting point for the vast majority of people.

Official [authorization documentation](https://docs.asp.net/en/latest/security/authorization/index.html) is at https://docs.asp.net/en/latest/security/authorization/index.html.

*Tip: When you stop finish running the app at each stage always close the browser to clear the identity cookie.*

Step 0: Preparation
===================

Create a new, blank, ASP.NET project.
-------------------------------------

* File > New Project > Visual C# > .NET Core 
* Select ASP.NET Core Web Application, give the project a name of `AuthorizationLab` and click OK.
* Select the Empty Template, ensure that ASP.NET Core 2.1 is selected in the drop down above the project types and click OK.

Add MVC to the app. 
-------------------

* Edit `Startup.cs`  and add `services.AddMvc();` to the top of the `ConfigureServices()` method;
* Edit the `Configure()` method, delete the existing code.
* In the now empty `Configure();` add the following code to setup MVC default routing;

```c#
app.UseMvc(routes =>
{
     routes.MapRoute(
          name: "default",
          template: "{controller=Home}/{action=Index}/{id?}");
});
```

* Create a `Controllers` folder 
* Create a `HomeController.cs` file in the Controllers directory, using the VS Controller template, or create it from scratch and ensuring inherits from `Controller` and has an `Index()` method which returns a View, for example

```c#

using Microsoft.AspNetCore.Mvc;

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

* Create a `Views` folder.
* Create a `Home` folder under the `Views`.
* Create an `Index.cshtml` file inside the `Views\Home` folder, and edit it to contain Hello World.
* Run your application and ensure you see Hello World.

Step 1: Setup authentication
============================

* In ASP.NET Core 2.1 the `Microsoft.AspNetCore.App` meta package contains all the authentication and authorization packages, so you don't need to add any extra packages or references.
* Open `startup.cs`
* Add `app.UseAuthentication();` at the top of the `Configure()` method.
* Add Cookie middleware to the authentication service by adding the following to the top of the `ConfigureServices()` method.

```c#
services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
        options => 
        {
                options.LoginPath = new PathString("/Account/Login/");
                options.AccessDeniedPath = new PathString("/Account/Forbidden/");
        });
```

* Edit the Home controller and add the `[Authorize]` attribute to the controller.
* Run the project and panic. You get a 404 error. you have no login page.
* Now create an `Account` controller, `AccountController.cs` in the `Controllers` folder. Create an `Login()` action and a `Forbidden()` action.

```c#
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationLab.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
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

* Create an `Account` folder under the `Views` folder and create corresponding views for the actions, `Login.cshtml` and `Forbidden.cshtml`. 
* Add some text to each view so you can tell which view is being displayed, and run your project. You will see you end up at the Login view.
* Return to the `Account` controller. Change the `Login` action to create a `Principal` and persist it using the code below. This will create user information and put it inside a cookie. This fakes what would normally happen in a forms based login system.

```c#
public async Task<IActionResult> Login(string returnUrl = null)
{
    const string Issuer = "https://contoso.com";
    var claims = new List<Claim>();
    claims.Add(new Claim(ClaimTypes.Name, "barry", ClaimValueTypes.String, Issuer));
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

* Finally edit the `Home\Index` view to display the name claim for the identity. Replace the contents of that view with the following code. This code is a little over complicated on purpose, as a user principal can contain more than one authenticated identity. This rarely happens, and frankly you'll know if you've written code to do this, but you should be aware of this edge case.

```
@using System.Security.Claims;

@if (!User.Identities.Any(u => u.IsAuthenticated))
{
    <h1>Hello World</h1>
}
else
{
    <h1>Hello @User.Identities.First(
      u => u.IsAuthenticated && 
      u.HasClaim(c => c.Type == ClaimTypes.Name)).FindFirst(ClaimTypes.Name).Value</h1>
}
```

* Run the project and see what happens. You should see 'Hello barry'. The code `claims.Add(new Claim(ClaimTypes.Name, "barry", ClaimValueTypes.String, Issuer));` inside the `Login()` method in the account controller is what is setting the name claim, and is what is being displayed by the view.

*Remember to close the browser to clear the identity cookie before moving on to the next step.*

Step 2: Authorize all the things
================================

* First remove the `Authorize` attribute from the `Home` controller.
* Change the `AddMvc()` call in `ConfigureServices()` in `Startup.cs` to add a default authorization policy to the MVC configuration.

```c#
services.AddMvc(config =>
{
    var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    config.Filters.Add(new AuthorizeFilter(policy));
});
```

* Run and watch everything blow up into an infinite redirect loop. Why? You've made every page require authentication. This even includes the login pages. 
* Now add the `[AllowAnonymous]` attribute to the `Account` controller, run again and see the user is logged in. `AllowAnonymous` allows you to mark a controller or an action method as not requiring authentication, even if you require authentication elsewhere.

*Remember to close the browser to clear the identity cookie before moving on to the next step.*

Step 3: Roles
=============

* Go to the `Home` controller and add an `Authorize` attribute with a role demand to either the controller, or the Index action method;

```c#
[Authorize(Roles = "Administrator")]
```

* Run the application and confirm you are redirected to the `Forbidden` view. This happens because you have an identity, but it's not part of the Administrator role.
* Close the browser to clear the identity cookie.
* Return to the `AccountController` and add a second `claims.Add()` line, as shown below. This adds a Role claim with the value of Administrator to the issued identity.

```c#
claims.Add(new Claim(ClaimTypes.Role, "Administrator", ClaimValueTypes.String, Issuer));
```

* Run the application and confirm you are logged in.

*Remember to close the browser to clear the identity cookie before moving on to the next step.*

Step 4: Simple Policies
=======================
* Return to `Startup.cs` and locate the `services.AddAuthentication()` call in `ConfigureServices()` call.
* After `services.AddAuthentication()` add a call to `services.AddAuthorization()` and create a simple policy as shown below.

```c#
services.AddAuthorization(options =>
{
    options.AddPolicy("AdministratorOnly", policy => policy.RequireRole("Administrator"));
});
```

* This policy is the equivalent of the Role check you used in the `Authorize` attribute parameters in Step 3. 
* Now change the Home controller `Authorize` attribute to require a policy, rather than use the role parameter.

```c#
[Authorize(Policy = "AdministratorOnly")]
```

* Run the app and confirm you still see the home page. All that has changed is how you're specifying your requirements. Instead of embedding the role name in the attribute you've written a policy which specifies the role name.
* Close your browser to clear your identity cookie.
* Now add a second policy, this time requiring a claim.

```c#
services.AddAuthorization(options =>
{
    options.AddPolicy("AdministratorOnly", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("EmployeeId", policy => policy.RequireClaim("EmployeeId"));
});
```

* Add a suitable claim to the identity issued by `Login` action in the `Account` Controller.

```c#
claims.Add(new Claim("EmployeeId", string.Empty, ClaimValueTypes.String, Issuer));
```
* Add a new `Authorize` attribute to the Home controller, using the new policy name.

```c#
[Authorize(Policy = "EmployeeId")]
```

* Run the app and ensure you can see the home page.
* This is a rather useless check, though. Claims are made up of a claim name and a claim values. You really want to check the values and not just the presence of a claim. Luckily there's a parameter for that. Change the `EmployeeId` policy to require one of a number of values;

```c#
options.AddPolicy("EmployeeId", policy => policy.RequireClaim("EmployeeId", "123", "456"));
```

* Run the app again and the empty claim will be rejected and you will end up at the Forbidden page. 
* Close your browser to clear the identity cookie.
* Change the identity issuing code to have a suitable claim value to the `Login` action, as shown below, and try again.

```c#
claims.Add(new Claim("EmployeeId", "123", ClaimValueTypes.String, Issuer));
```

If a policy has multiple claim requirements all the claim requirements must be fulfilled for authorization to succeed.

*Remember to close the browser to clear the identity cookie before moving on to the next step.*

Step 5: Code Based Policies
===========================

Code based policies consist of a requirement, implementing `IAuthorizationRequirement` and a handler for the requirement, implementing `AuthorizationHandler<T>` where T is the requirement.

* Add a date of birth claim to the user principal in the `Login` action in the `Account` controller.

```c#
claims.Add(new Claim(ClaimTypes.DateOfBirth, "1970-06-08", ClaimValueTypes.Date));
```

* Now create a custom requirement and handler class; called MinimumAgeRequirement. Here we're going to use a single class for both the requirement and the handler, for simplicity's sake.

```c#
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AuthorizationLab
{
    public class MinimumAgeRequirement : AuthorizationHandler<MinimumAgeRequirement>, IAuthorizationRequirement
    {
        int _minimumAge;

        public MinimumAgeRequirement(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            MinimumAgeRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == ClaimTypes.DateOfBirth))
            {
                return Task.CompletedTask;
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

            return Task.CompletedTask;
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
* Experiment with the date of birth value (for example, make the year last year) to make authorization fail. Don't forget to set it back to a passing value before you move on.

*Remember to close the browser to clear the identity cookie before moving on to the next step.*

Step 6: Multiple handlers for a requirement
===========================================

You may have noticed what a handler returns, nothing at all (Strictly we're returning `Task.CompletedTask;`, which is effectively nothing). 
Handlers inform the authorization service they have succeeded by calling `context.Succeed(requirement);`. 
You may be asking yourself if there is a `context.Succeed()` is there a `context.Fail()`? There is, but if your requirement isn't met you
shouldn't touch the context at all. Now you may be asking why not? Well ...

Sometimes you may want multiple handlers for an Authorization Requirement, 
for example when there are multiple ways to fulfill a requirement. Microsoft's office doors 
open with your Microsoft badge, however on days you forget your badge you can go to 
reception and get a temporary pass and the receptionist will let you through the gates. 
Thus there are two ways to fulfill the single entry requirement.
In the ASP.NET Core authorization model this would be implemented as two handlers for a single requirement.

* First, write a new `IAuthorizationRequirement`, `OfficeEntryRequirement`.

```c#
using Microsoft.AspNetCore.Authorization;

namespace AuthorizationLab
{
    public class OfficeEntryRequirement : IAuthorizationRequirement
    {
    }
}
```

* Now write an `AuthorizationHandler` that checks if the current identity has a badge number claim, 
issued by the employer, `HasBadgeHandler`.

```c#
using Microsoft.AspNetCore.Authorization;

namespace AuthorizationLab
{
    public class HasBadgeHandler : AuthorizationHandler<OfficeEntryRequirement>
    {
        protected override Task HandleRequirementAsync(
          AuthorizationHandlerContext context, 
          OfficeEntryRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "BadgeNumber" && 
                                            c.Issuer == "https://contoso.com"))
            {
                return Task.CompletedTask;
            }

            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
```

That takes care of people who remembered their badges, issued by the right company (after all multiple companies have entry cards,
so you want to check that the card is issued by the company you expect. The `Claims` class has an issuer property which details who
issued the claim, so in our case it's who issued the badge). 

But what about those who forget and have 
a temporary badge? You could just put it all in one handler, but handlers and requirements are 
meant to be reusable. You could use the `HasBadgeHandler` shown above for other things, not just office entry 
(for example the Microsoft code signing infrastructure needs the smart card that is our office badge to trigger jobs).

* To cope with temporary badges write another `AuthorizationHandler`, `HasTemporaryPassHandler`

```c#
using System;
using Microsoft.AspNetCore.Authorization;

namespace AuthorizationLab
{
    public class HasTemporaryPassHandler : AuthorizationHandler<OfficeEntryRequirement>
    {
        protected override Task HandleRequirementAsync(
          AuthorizationHandlerContext context, 
          OfficeEntryRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "TemporaryBadgeExpiry" &&
                                            c.Issuer == "https://contoso.com"))
            {
                return Task.CompletedTask;
            }

            var temporaryBadgeExpiry = 
                Convert.ToDateTime(context.User.FindFirst(
                                       c => c.Type == "TemporaryBadgeExpiry" &&
                                       c.Issuer == "https://contoso.com").Value);

            if (temporaryBadgeExpiry > DateTime.Now)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
```

Note that neither handler calls context.Fail(). 
context.Fail() is there for occasions when authorization cannot continue, even if there's another handler,
for example, "My Entire User Database is on fire." or "The user I'm looking at has just been blocked, but other back-end systems 
may not yet be updated."

* Next create a policy for the requirement, registering it in the `ConfigureServices()` in `Startup.cs`, inside the authorization configuration.

```c#
options.AddPolicy("BuildingEntry", policy => policy.Requirements.Add(new OfficeEntryRequirement()));
```

* Go back to the `Account` controller's `Login` method and add a suitable badge ID claim.

```c#
claims.Add(new Claim("BadgeNumber", "123456", ClaimValueTypes.String, Issuer));
```

* Finally, apply the policy created to the `Index` view in the `Home` controller using the `Authorize` attribute.

```c#
[Authorize(Policy = "BuildingEntry")]
```

* Run the app and, oh dear, we get bounced to forbidden. Why?

Handlers are held in the ASP.NET DI container. In our previous sample we combined the requirement and the handler in one class, so the authorization system knew about it without having to manually register it in DI. Now we have separate handlers we need to register them in the DI container before they can be found.

* Open `Startup.cs`, and inside `ConfigureServices()` register the handlers in the DI container by adding the following to the bottom of the `ConfigureServices()` method. Note that they don't have to be singletons, you can use the DI system to inject constructor parameters into handlers, so, for example, if you're injecting an EF repository you may want to add your handler as scoped to a request.

```c#
services.AddSingleton<IAuthorizationHandler, HasBadgeHandler>();
services.AddSingleton<IAuthorizationHandler, HasTemporaryPassHandler>();
```

* Run the app again and you can see authorization works. 
* Experiment with commenting out the BadgeNumber claim and replacing it with a TemporaryBadgeExpiry claim, remembering to close the browser each time to clear the identity cookie so it will be recreated with your new claims.

```c#
claims.Add(new Claim("TemporaryBadgeExpiry", 
                     DateTime.Now.AddDays(1).ToString(), 
                     ClaimValueTypes.String, 
                     Issuer));
```

* Run the app, and you're still authorized because now the handler for temporary badges fulfills the building entry requirement.
* Change the temporary badge claim so it has expired; remembering to close the browser to clear the identity cookie before running your new code.

```c#
claims.Add(new Claim("TemporaryBadgeExpiry", 
                     DateTime.Now.AddDays(-1).ToString(), 
                     ClaimValueTypes.String, 
                     Issuer));
```

* Rerun the app and you'll see you're forbidden.
* Remove the temporary badge claim and uncomment the BadgeNumber claim code, so you're back to being authorized, close your browser to clear the identity cookie and rerun the app to make sure you are no longer forbidden.

Step 7: Resource Based Requirements
===================================

So far we've covered requirements that are based only on a user's identity. However often authorization requires the resource being accessed. For example a Document class may have an author and only authors can edit the document, whilst others can view it.

* Create a resource class, `Document` with an `int` ID property and a `string` Author property.

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

* Finally register the document repository in the services collection through the `ConfigureServices()` method in `Startup.cs`

```c#
services.AddSingleton<IDocumentRepository, DocumentRepository>();
```

Now we can create a suitable controller and views to display a list of documents and the document itself.

* First create a Document controller in the Controllers folder, `DocumentController.cs`.

```c#
using Microsoft.AspNetCore.Mvc;

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
                return new NotFoundResult();
            }

            return View(document);
        }
    }
}
```

* Create a `Document` folder underneath the `Views` folder and create an Index view, `Index.cshtml`

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

* Create an Edit view in the Document view folder, `Edit.cshtml`

```
@using AuthorizationLab
@model Document

<h1>Document #@Model.Id</h1>
<h2>Author: @Model.Author</h2>
```

* Run the app and load the `/Document` URL. Ensure you see a list of documents and you can click into each one.

Now we need to define operations to authorize against. For a document this might be Read, Write, Edit and Delete. We provide a base class, OperationAuthorizationRequirement which you can use as a starting point, but it's optional.

* Define a requirement for editing, `EditRequirement.cs`

```c#
using Microsoft.AspNetCore.Authorization;

namespace AuthorizationLab
{
    public class EditRequirement : IAuthorizationRequirement
    {
    }
}
```

Now, as before, we write a handler for the requirement, but this time we write a handler which takes a resource. 

* Create a DocumentEditHandler, `DocumentEditHandler.cs`. This time specify a resource parameter as well as the requirement in the class definition.

``` c#
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace AuthorizationLab
{
    public class DocumentEditHandler : AuthorizationHandler<EditRequirement, Document>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            EditRequirement requirement, 
            Document resource)
        {
            if (resource.Author == context.User.FindFirst(ClaimTypes.Name).Value)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
```

* Finally register the handler in `ConfigureServices()` in `Startup.cs`

```c#
services.AddSingleton<IAuthorizationHandler, DocumentEditHandler>();
```

We cannot use resource handlers in attributes, because binding hasn't happened at that point and we need the resource. The resource only becomes available inside the action method. So we must call the authorization service directly.

* Return to the Document controller and edit the constructor to include `IAuthorizationService` as one of its parameters and store it in a local variable.

```c#
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                return new NotFoundResult();
            }

            return View(document);
        }
    }
}
```

Finally we can call the service inside an action method. 

* In the Edit action change it to be async, returning a `Task<IActionResult>`, add an `Authorize` attribute to ensure we have a user to check, and finally call the `_authorizeService.AuthorizeAsync` method with the user, resource and the requirement. 
If the authorization call fails you should return a `ForbidResult();`, as the current user is forbidden to perform the action.

```c#
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var document = _documentRepository.Get(id);

            if (document == null)
            {
                return new NotFoundResult();
            }


            var authorizationResult = await _authorizationService.AuthorizeAsync(User, document, new EditRequirement());
            if (authorizationResult.Succeeded)
            {
                return View(document);
            }
            else
            {
                return new ForbidResult();
            }
        }
    }
}
```

* Run the app and go to the Document URL. You should be able to click through on each document and see Document 1 but not Document 2, because you don't have access to it. 

Step 8: Authorizing in Views
============================

For resource links and other UI elements you probably want to not show those links to users in the UI, so as to reduce temptation. 
You still want to keep authorization checks in the Controller - never rely solely on UI element removal as a security mechanism. 
ASP.NET Core allows DI within views, so you can use the same approach in Step 7 to hide documents in the document list the current user cannot access.

* Open the Index view file, `Index.cshtml` in the `Documents` folder. 
* Add an @using statement for `Microsoft.AspNetCore.Authorization` and inject the `AuthorizationService` using the `@inject` command

```
@using Microsoft.AspNetCore.Authorization
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
@using Microsoft.AspNetCore.Authorization
@using AuthorizationLab

@model IEnumerable<Document>
@inject IAuthorizationService AuthorizationService

<h1>Document Library</h1>
@{ 
    var requirement = new EditRequirement();
    foreach (var document in Model)
    {
        var authorizationResult = await AuthorizationService.AuthorizeAsync(User, document, requirement);
        if (authorizationResult.Succeeded)
        {
        <p>@Html.ActionLink("Document #" + document.Id, "Edit", new { id = document.Id })</p>
        }
    }
}
```

* Run the app and browse to the Document URL, and you will see that you now only have a link to Document #1. You can still manually attempt to access document #2, but because you've kept the controller checks as well you won't get access.

Applying what you've learnt
===========================

Open the `Workshop_Start` folder. 
    
This is a sample web site for inventory control. The site allows record label employees to update the details of albums.

There are 3 users, barryd, davidfowl and dedwards. barryd is an administrator for Paddy Productions. dewards is an administrator for ToneDeaf Records. davidfowl is an employee of ToneDeaf Records, but not an administrator. Administrators are part of the Administrator role.

A User repository has been provided for you and is injected into the `Account` controller. You should use the `ValidateLogin()` function to first check if the login is correct, then retrieve a suitable user principal using the `Get()` method. 

The cookie authentication middleware is already configured, the scheme name is available from the `Constants.MiddlewareScheme` field.

Change the site to include the following functionality:

1. Change the `AccountController` `Login` action to create a cookie for the user logging in using the already configured cookie middleware.
2. Make the entire site require a login, excluding the `Login` action in the `AccountController`.
3. Make the `Edit` action in the `HomeController` only available to logged in Administrators for any company.
4. Make the Edit functionality only available to Administrators for the company that has issued the album.
5. Change the `Index` action in the `HomeController` so it only lists all albums and but the edit link is only shown for administrators for the company that issued the album.

A sample solution is contained in the `Workshop_Suggested_Solution` folder.
