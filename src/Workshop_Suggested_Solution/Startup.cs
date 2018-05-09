using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using AuthorizationWorkshop.Repositories;

namespace AuthorizationWorkshop
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(Constants.MiddlewareScheme)
                .AddCookie(Constants.MiddlewareScheme,
                    options =>
                    {
                        options.LoginPath = new PathString("/Account/Login/");
                        options.AccessDeniedPath = new PathString("/Account/Forbidden/");
                    });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyNames.AdministratorsOnly,
                    policy => policy.RequireRole("Administrator"));

                options.AddPolicy(PolicyNames.CanEditAlbum,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireRole("Administrator");
                        policy.Requirements.Add(new AlbumOwnerRequirement());
                    }
                );

            });
            services.AddMvc();

            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IAlbumRepository, AlbumRepository>();

            services.AddSingleton<IAuthorizationHandler, AlbumOwnerAuthorizationHandler>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseAuthentication();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
