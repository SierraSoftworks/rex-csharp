using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;

namespace Rex;

public static class Program
{
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((ctx, config) =>
            {
                // Disable file system monitoring
                foreach (var configSource in config.Sources.OfType<Microsoft.Extensions.Configuration.Json.JsonConfigurationSource>())
                {
                    configSource.ReloadOnChange = false;
                }

                var builtConfig = config.Build();

                if (!string.IsNullOrWhiteSpace(builtConfig.GetConnectionString("ConfigurationKeyVault")))
                {
                    var azureServiceTokenProvider = new AzureServiceTokenProvider();
                    var keyVaultClient = new KeyVaultClient(
                        new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback)
                    );

                    config.AddAzureKeyVault(
                        builtConfig.GetConnectionString("ConfigurationKeyVault"),
                        keyVaultClient,
                        new DefaultKeyVaultSecretManager()
                    );
                }
            })
            .ConfigureWebHostDefaults(webBuilder =>
                webBuilder
                    .UseStartup<Startup>()
                    .UseSentry("https://b7ca8a41e8e84fef889e4f428071dab2@sentry.io/1415519"));
}
