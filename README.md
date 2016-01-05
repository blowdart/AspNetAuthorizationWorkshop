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