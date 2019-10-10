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
using System.Globalization;

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
        [InlineData("https://rex.sierrasoftworks.com")]
        [InlineData("https://example.com")]
        public async Task TestCors(string origin)
        {
            var client = Factory.CreateAuthenticatedClient("Administrator", Scopes.IdeasRead);

            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/{Version}/ideas"))
            {
                request.Headers.Add("Origin", origin);
                var response = await client.SendAsync(request).ConfigureAwait(false);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                response.Headers.GetValues("Access-Control-Allow-Origin").Should().Contain("*");
            }
        }

        [Theory]
        [InlineData("Administrator", Scopes.IdeasRead)]
        [InlineData("User", Scopes.IdeasRead)]
        public async Task TestGetIdea(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection(Tokens.PrincipalId, Tokens.PrincipalId)).ConfigureAwait(false);
            var idea = await Factory.IdeaStore.StoreIdeaAsync(CreateNewIdea(collection.CollectionId)).ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.GetAsync(GetIdeaUri(idea)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var view = await response.Content.ReadAsAsync<TView>().ConfigureAwait(false);

            view.Should().NotBeNull().And.BeEquivalentTo(this.Representer.ToView(idea));
        }

        [Theory]
        [InlineData("Administrator", Scopes.IdeasRead)]
        [InlineData("User", Scopes.IdeasRead)]
        public async Task TestGetIdeaNotFound(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);
            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection(Tokens.PrincipalId, Tokens.PrincipalId)).ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.GetAsync(GetIdeaUri(CreateNewIdea(collection.CollectionId))).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            response.Content.Headers.ContentType.MediaType.Should().Be("application/problem+json");
        }

        [Theory]
        [InlineData("Administrator")]
        [InlineData("User")]
        [InlineData("Administrator", Scopes.IdeasWrite, Scopes.CollectionsRead, Scopes.CollectionsWrite, Scopes.RoleAssignmentsWrite)]
        [InlineData("User", Scopes.IdeasWrite, Scopes.CollectionsRead, Scopes.CollectionsWrite, Scopes.RoleAssignmentsWrite)]
        public async Task TestGetIdeaInvalidScopes(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.GetAsync(GetIdeaUri(CreateNewIdea())).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        protected virtual Uri GetIdeaUri(Idea idea)
        {
            if (idea is null)
            {
                throw new ArgumentNullException(nameof(idea));
            }

            return new Uri($"/api/{Version}/idea/{idea.Id.ToString("N", CultureInfo.InvariantCulture)}", UriKind.Relative);
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
