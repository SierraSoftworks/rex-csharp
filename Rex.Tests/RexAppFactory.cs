
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Rex.Stores;

namespace Rex.Tests
{
    public class RexAppFactory : WebApplicationFactory<Startup>
    {
        private readonly MemoryIdeaStore ideaStore = new MemoryIdeaStore();

        private readonly MemoryRoleAssignmentStore roleAssignmentStore = new MemoryRoleAssignmentStore();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            base.ConfigureWebHost(builder);

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IIdeaStore>(ideaStore);
                services.AddSingleton<IRoleAssignmentStore>(roleAssignmentStore);
            });
        }
    }
}
