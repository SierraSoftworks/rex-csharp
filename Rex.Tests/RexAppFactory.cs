using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Rex.Stores;

namespace Rex.Tests;

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
        await ((IdeaStore as MemoryIdeaStore)?.ClearAsync() ?? Task.Delay(0)).ConfigureAwait(true);
        await ((CollectionStore as MemoryCollectionStore)?.ClearAsync() ?? Task.Delay(0)).ConfigureAwait(true);
        await ((RoleAssignmentStore as MemoryRoleAssignmentStore)?.ClearAsync() ?? Task.Delay(0)).ConfigureAwait(true);
        await ((UserStore as MemoryUserStore)?.ClearAsync() ?? Task.Delay(0)).ConfigureAwait(true);
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
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        builder.ConfigureAppConfiguration((builder, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?> {
                ["AllowedHosts:0"]="https://rex.sierrasoftworks.com",
                ["Logging:LogLevel:Default"]="Warning",
                ["Storage:Mode"]="Memory",
            });
        })
        .ConfigureLogging((builder, config) =>
        {
            config
                .AddProvider(new XunitLoggerProvider(this.testOutputHelper))
                .SetMinimumLevel(LogLevel.Warning);
        });

        return base.CreateHost(builder);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5404", Justification = "We rely on being able to validate test tokens without worrying about the lifetime constraint.")]
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

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
