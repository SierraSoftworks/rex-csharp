
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

        public async Task ClearAsync()
        {
            await ((IdeaStore as MemoryIdeaStore)?.ClearAsync() ?? Task.Delay(0)).ConfigureAwait(false);
            await ((CollectionStore as MemoryCollectionStore)?.ClearAsync() ?? Task.Delay(0)).ConfigureAwait(false);
            await ((RoleAssignmentStore as MemoryRoleAssignmentStore)?.ClearAsync() ?? Task.Delay(0)).ConfigureAwait(false);
        }

        public Guid PrincipalId => Guid.Parse("d6cf5e7f-b12a-444d-8c7f-6790b77e49a9");

        public string AuthToken => "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJhdWQiOiJ0ZXN0cyIsImlzcyI6InRlc3RzIiwiaWF0IjoxNTAwMDAwMDAwLCJuYmYiOjE1MDAwMDAwMDAsImV4cCI6MjUwMDAwMDAwMCwiYWNyIjoiMSIsImFtciI6WyJwd2QiLCJtZmEiXSwiYXBwaWQiOiI3YTZkNmVhZS02Y2VlLTQzZDYtYWY1Ny0zZGE5MDhiMGYxMjAiLCJhcHBpZGFjciI6IjAiLCJkZXZpY2VpZCI6IjYxMzRjOWM5LWI4OTMtNDY1MS05ZmQxLTM2MThkYjhkMWZjNCIsImZhbWlseV9uYW1lIjoiTWNUZXN0ZXJzb24iLCJnaXZlbl9uYW1lIjoiVGVzdHkiLCJuYW1lIjoiVGVzdHkgTWNUZXN0ZXJzb24iLCJvaWQiOiJkNmNmNWU3Zi1iMTJhLTQ0NGQtOGM3Zi02NzkwYjc3ZTQ5YTkiLCJyb2xlcyI6WyJBZG1pbmlzdHJhdG9yIl0sInNjcCI6InVzZXJfaW1wZXJzb25hdGlvbiBJZGVhcy5SZWFkIElkZWFzLldyaXRlIENvbGxlY3Rpb25zLlJlYWQgQ29sbGVjdGlvbnMuV3JpdGUgUm9sZUFzc2lnbm1lbnRzLldyaXRlIiwic3ViIjoiWHdxMnNRSkVZVWJ4a3dWXzBWOUdnX25JQVcybVdYOXRKbnRfR3Fya2RibSIsInRpZCI6IjZhZGEwNjFiLTE2NmMtNGE3My1iMzZkLWM3N2Q2Y2M4Y2FhNCIsInVuaXF1ZV9uYW1lIjoidGVzdHlAdGVzdGVyc29uLmNvbSIsInVwbiI6InRlc3R5QHRlc3RlcnNvbi5jb20iLCJ2ZXIiOiIxLjAifQ.raXl7qG0q-YeiLoNCPFnK2DVSpHtLR1STUoVyZFLH6U";

        public HttpClient CreateAuthenticatedClient()
        {
            var client = this.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.AuthToken);

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
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-256-bit-secret")),
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
