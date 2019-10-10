using FluentAssertions;
using Rex.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using SierraLib.API.Views;
using System.Net;
using Xunit.Abstractions;

namespace Rex.Tests.Controllers
{
    public abstract class IdeaControllerTests<TView>
        where TView : class, IView<Idea>
    {
        public IdeaControllerTests(ITestOutputHelper testOutputHelper)
        {
            this.Factory = new RexAppFactory(testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper)));
            this.Representer = Factory.Services.GetService<IRepresenter<Idea, TView>>();
        }

        protected abstract string Version { get; }

        protected RexAppFactory Factory { get; }

        public IRepresenter<Idea, TView> Representer { get; }

        [Theory]
        [InlineData("/api/v1/ideas", "https://rex.sierrasoftworks.com")]
        [InlineData("/api/v1/ideas", "https://example.com")]
        public async Task TestCors(string endpoint, string origin)
        {
            var client = Factory.CreateAuthenticatedClient();

            using (var request = new HttpRequestMessage(HttpMethod.Get, endpoint))
            {
                request.Headers.Add("Origin", origin);
                var response = await client.SendAsync(request).ConfigureAwait(false);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                response.Headers.GetValues("Access-Control-Allow-Origin").Should().Contain("*");
            }
        }

        protected Collection CreateCollection(Guid principalId, Guid? collectionId = null)
        {
            return new Collection
            {
                CollectionId = collectionId ?? Guid.NewGuid(),
                PrincipalId = principalId,
                Name = "Test Collection",
            };
        }

        protected Idea CreateNewIdea(Guid? collectionId = null)
        {
            return new Idea
            {
                Id = Guid.NewGuid(),
                CollectionId = collectionId ?? Guid.Empty,
                Name = "Test Idea",
                Description = "This is an idea used to test the service.",
                Completed = false,
                Tags = new System.Collections.Generic.HashSet<string>() { "test" },
            };
        }
    }
}
