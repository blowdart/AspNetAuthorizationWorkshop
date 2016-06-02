
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace AuthorizationLab
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdministratorOnly", policy => policy.RequireRole("Administrator"));
                options.AddPolicy("EmployeeId", policy => policy.RequireClaim("EmployeeId", "123", "456"));
                options.AddPolicy("Over21Only", policy => policy.Requirements.Add(new MinimumAgeRequirement(21)));
                options.AddPolicy("BuildingEntry", policy => policy.Requirements.Add(new OfficeEntryRequirement()));
            });

            services.AddSingleton<IAuthorizationHandler, HasBadgeHandler>();
            services.AddSingleton<IAuthorizationHandler, HasTemporaryPassHandler>();
            services.AddSingleton<IAuthorizationHandler, DocumentEditHandler>();

            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddSingleton<IDocumentRepository, DocumentRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "Cookie",
                LoginPath = new PathString("/Account/Login/"),
                AccessDeniedPath = new PathString("/Account/Forbidden/"),
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
