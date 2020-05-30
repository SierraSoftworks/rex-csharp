using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentry.AspNetCore;

namespace Rex
{
    public static class Program
    {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, config) =>
                {
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
}
