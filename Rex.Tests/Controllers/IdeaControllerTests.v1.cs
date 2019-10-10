using Rex.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;
using Xunit;
using System.Globalization;
using System.Net;
using FluentAssertions;
using Xunit.Abstractions;
using System.Net.Http;
using System;

namespace Rex.Tests.Controllers
{
    public class IdeaControllerV1Tests
        : IdeaControllerTests<Idea.Version1>
    {
        public IdeaControllerV1Tests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        protected override string Version => "v1";

        [Fact]
        public async Task TestGetIdea()
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection(Factory.PrincipalId, Factory.PrincipalId)).ConfigureAwait(false);
            var idea = await Factory.IdeaStore.StoreIdeaAsync(CreateNewIdea(collection.CollectionId)).ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient();

            var response = await client.GetAsync(new Uri($"/api/{Version}/idea/{idea.Id.ToString("N", CultureInfo.InvariantCulture)}", UriKind.Relative)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var view = await response.Content.ReadAsAsync<Idea.Version1>().ConfigureAwait(false);

            view.Should().NotBeNull();
        }

        [Fact]
        public async Task TestGetIdeaNotFound()
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient();

            var response = await client.GetAsync(new Uri($"/api/{Version}/idea/{Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture)}", UriKind.Relative)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            response.Content.Headers.ContentType.MediaType.Should().Be("application/problem+json");
        }
    }
}
