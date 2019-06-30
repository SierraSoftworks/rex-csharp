using FluentAssertions;
using Rex.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Rex.Tests.Controllers
{
    public abstract class HealthControllerTests<TView, TRepresenter>
        : IClassFixture<WebApplicationFactory<Startup>>
        where TView : IView<Health>
        where TRepresenter : IRepresenter<Health, TView>, new()
    {
        public HealthControllerTests(WebApplicationFactory<Startup> factory)
        {
            this.Factory = factory;
            this.Representer = new TRepresenter();
        }

        protected abstract string Version { get; }

        WebApplicationFactory<Startup> Factory { get; }

        public IRepresenter<Health, TView> Representer { get; }

        [Fact]
        public async Task TestGetHealth()
        {
            var client = Factory.CreateClient();

            var response = await client.GetAsync($"/api/{Version}/health");
            response.EnsureSuccessStatusCode();

            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");

            var view = await response.Content.ReadAsAsync<TView>();
            var model = Representer.ToModel(view);

            model.StartedAt.Should().BeCloseTo(DateTime.UtcNow);
        }
    }
}
