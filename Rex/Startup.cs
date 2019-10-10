using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Rex
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(o => o.AddDefaultPolicy(c => c.AllowAnyOrigin()));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
            {
                o.Authority = Configuration["Authentication:Authority"];
                o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidAudiences = new List<string>
                    {
                        Configuration["Authentication:AppIdUri"],
                        Configuration["Authentication:ClientId"]
                    },
                };
            });

            services.AddAuthorization(o =>
            {
                o.AddPolicy("default", p =>
                    p.RequireAuthenticatedUser()
                        .RequireClaim("http://schemas.microsoft.com/identity/claims/objectidentifier"));

                foreach (var name in Scopes.All())
                    o.AddPolicy(name, p =>
                    p.RequireAuthenticatedUser()
                        .RequireClaim("http://schemas.microsoft.com/identity/claims/objectidentifier")
                        .RequireClaim("http://schemas.microsoft.com/identity/claims/scope")
                        .RequireAssertion(s => s.User.FindAll(c => c.Type == "http://schemas.microsoft.com/identity/claims/scope" && c.Value.Split(' ').Contains(name)).Any()));

                o.DefaultPolicy = o.GetPolicy("default");
            });

            services.AddResponseCompression()
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddModelRepresentersFromAssembly(typeof(Models.User).Assembly);

            switch (this.Configuration.GetValue<string>("Storage:Mode").ToUpperInvariant())
            {
                case "TABLESTORAGE":
                    services.AddSingleton<Stores.IHealthStore, Stores.MemoryHealthStore>()
                            .AddSingleton<Stores.IIdeaStore, Stores.TableStorageIdeaStore>()
                            .AddSingleton<Stores.ICollectionStore, Stores.TableStorageCollectionStore>()
                            .AddSingleton<Stores.IRoleAssignmentStore, Stores.TableStorageRoleAssignmentStore>();
                    break;
                case "MEMORY":
                default:
                    services.AddSingleton<Stores.IHealthStore, Stores.MemoryHealthStore>()
                            .AddSingleton<Stores.IIdeaStore, Stores.MemoryIdeaStore>()
                            .AddSingleton<Stores.ICollectionStore, Stores.MemoryCollectionStore>()
                            .AddSingleton<Stores.IRoleAssignmentStore, Stores.MemoryRoleAssignmentStore>();
                    break;
            }

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseCors(policy => policy.AllowAnyOrigin().WithMethods("GET"));

            app.UseCors();
            app.Use((context, next) =>
            {
                context.Items["__CorsMiddlewareInvoked"] = true;
                return next();
            });

            app.UseRouting()
                .UseResponseCompression()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(routes => routes.MapControllers());
        }
    }
}
