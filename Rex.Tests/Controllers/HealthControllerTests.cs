using FluentAssertions;
using Rex.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using SierraLib.API.Views;

namespace Rex.Tests.Controllers
{
    public abstract class HealthControllerTests<TView>
        : IClassFixture<WebApplicationFactory<Startup>>
        where TView : class, IView<Health>
    {
        public HealthControllerTests(WebApplicationFactory<Startup> factory)
        {
            this.Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.Representer = factory.Services.GetService<IRepresenter<Health, TView>>();
        }

        protected abstract string Version { get; }

        WebApplicationFactory<Startup> Factory { get; }

        public IRepresenter<Health, TView> Representer { get; }

        [Fact]
        public async Task TestGetHealth()
        {
            var client = Factory.CreateClient();

            var response = await client.GetAsync(new Uri($"/api/{Version}/health", UriKind.Relative)).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");

            var view = await response.Content.ReadAsAsync<TView>().ConfigureAwait(false);
            var model = Representer.ToModel(view);

            model.StartedAt.Should().BeCloseTo(DateTime.UtcNow);
        }
    }
}
