
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Rex.Stores;
using Xunit;
using Xunit.Abstractions;

namespace Rex.Tests
{
    public class RexAppFactory : WebApplicationFactory<Startup>
    {
        private readonly ITestOutputHelper testOutputHelper;

        public RexAppFactory(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        public IIdeaStore IdeaStore => this.Services.GetRequiredService<IIdeaStore>();

        public ICollectionStore CollectionStore => this.Services.GetRequiredService<ICollectionStore>();

        public IRoleAssignmentStore RoleAssignmentStore => this.Services.GetRequiredService<IRoleAssignmentStore>();

        public IUserStore UserStore => this.Services.GetRequiredService<IUserStore>();

        public async Task ClearAsync()
        {
            await ((IdeaStore as MemoryIdeaStore)?.ClearAsync() ?? Task.Delay(0)).ConfigureAwait(false);
            await ((CollectionStore as MemoryCollectionStore)?.ClearAsync() ?? Task.Delay(0)).ConfigureAwait(false);
            await ((RoleAssignmentStore as MemoryRoleAssignmentStore)?.ClearAsync() ?? Task.Delay(0)).ConfigureAwait(false);
            await ((UserStore as MemoryUserStore)?.ClearAsync() ?? Task.Delay(0)).ConfigureAwait(false);
        }

        public HttpClient CreateAuthenticatedClient(string role, params string[] scopes)
        {
            return this.CreateAuthenticatedClient(new[] { role }, scopes);
        }

        public HttpClient CreateAuthenticatedClient(IEnumerable<string>? roles = null, IEnumerable<string>? scopes = null)
        {
            var client = this.CreateClient();
            var token = TestTokens.GetToken(roles, scopes);
            this.testOutputHelper.WriteLine("Using authentication token {0}", token);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ConfigureAppConfiguration((builder, config) =>
            {
                config.AddInMemoryCollection(new[] {
                    new KeyValuePair<string, string>("AllowedHosts:0", "https://rex.sierrasoftworks.com"),
                    new KeyValuePair<string, string>("Logging:LogLevel:Default", "Information"),
                    new KeyValuePair<string, string>("Storage:Mode", "Memory"),
                });
            })
            .ConfigureLogging((builder, config) =>
            {
                config
                    .AddProvider(new XunitLoggerProvider(this.testOutputHelper))
                    .SetMinimumLevel(LogLevel.Debug);
            });

            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            base.ConfigureWebHost(builder);

            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(opts =>
                {
                    opts.DefaultAuthenticateScheme = "Test";
                    opts.DefaultChallengeScheme = "Test";
                }).AddJwtBearer("Test", o =>
                {
                    Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
                    o.RequireHttpsMetadata = false;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestTokens.SigningKey)),
                        RequireSignedTokens = true,

                        ValidIssuer = "tests",
                        ValidateIssuer = true,

                        ValidAudience = "tests",
                        ValidateAudience = true,

                        ValidateLifetime = false,
                    };
                });
            });
        }
    }
}
