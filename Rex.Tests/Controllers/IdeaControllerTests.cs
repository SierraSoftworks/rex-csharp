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

            using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/{Version}/ideas");
            request.Headers.Add("Origin", origin);
            var response = await client.SendAsync(request).ConfigureAwait(false);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Headers.GetValues("Access-Control-Allow-Origin").Should().Contain("*");
        }

        [Theory]
        [InlineData("Administrator", Scopes.IdeasRead)]
        [InlineData("User", Scopes.IdeasRead)]
        public async Task TestGetIdea(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection()).ConfigureAwait(false);
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
            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection()).ConfigureAwait(false);

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

        [Theory]
        [InlineData("Administrator", Scopes.IdeasWrite)]
        [InlineData("User", Scopes.IdeasWrite)]
        public async Task TestStoreIdea(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection()).ConfigureAwait(false);
            var idea = await Factory.IdeaStore.StoreIdeaAsync(CreateNewIdea(collection.CollectionId)).ConfigureAwait(false);

            idea.Description = "This is an updated description";
            idea.Completed = true;

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.PutAsJsonAsync(GetIdeaUri(idea), this.Representer.ToView(idea)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var view = await response.Content.ReadAsAsync<TView>().ConfigureAwait(false);

            view.Should().NotBeNull().And.BeEquivalentTo(this.Representer.ToView(idea));
        }

        [Theory]
        [InlineData("Administrator", Scopes.IdeasWrite)]
        [InlineData("User", Scopes.IdeasWrite)]
        public async Task TestStoreIdeaNotFound(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);
            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection()).ConfigureAwait(false);
            var idea = CreateNewIdea(collection.CollectionId);
            idea.Id = Guid.NewGuid();

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.PutAsJsonAsync(GetIdeaUri(idea), this.Representer.ToView(idea)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var view = await response.Content.ReadAsAsync<TView>().ConfigureAwait(false);

            view.Should().NotBeNull().And.BeEquivalentTo(this.Representer.ToView(idea));
        }

        [Theory]
        [InlineData("Administrator")]
        [InlineData("User")]
        [InlineData("Administrator", Scopes.IdeasRead, Scopes.CollectionsRead, Scopes.CollectionsWrite, Scopes.RoleAssignmentsWrite)]
        [InlineData("User", Scopes.IdeasRead, Scopes.CollectionsRead, Scopes.CollectionsWrite, Scopes.RoleAssignmentsWrite)]
        public async Task TestStoreIdeaInvalidScopes(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);
            var idea = CreateNewIdea();

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.PutAsJsonAsync(GetIdeaUri(idea), this.Representer.ToView(idea)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData("Administrator", Scopes.IdeasRead)]
        [InlineData("User", Scopes.IdeasRead)]
        public async Task TestGetRandomIdea(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection()).ConfigureAwait(false);
            var idea = await Factory.IdeaStore.StoreIdeaAsync(CreateNewIdea(collection.CollectionId)).ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.GetAsync(GetRandomIdeaUri(collection)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var view = await response.Content.ReadAsAsync<TView>().ConfigureAwait(false);

            view.Should().NotBeNull().And.BeEquivalentTo(this.Representer.ToView(idea));
        }

        [Theory]
        [InlineData("Administrator", Scopes.IdeasRead)]
        [InlineData("User", Scopes.IdeasRead)]
        public async Task TestGetRandomIdeaNotFound(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);
            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection()).ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.GetAsync(GetRandomIdeaUri(collection)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            response.Content.Headers.ContentType.MediaType.Should().Be("application/problem+json");
        }

        [Theory]
        [InlineData("Administrator")]
        [InlineData("User")]
        [InlineData("Administrator", Scopes.IdeasWrite, Scopes.CollectionsRead, Scopes.CollectionsWrite, Scopes.RoleAssignmentsWrite)]
        [InlineData("User", Scopes.IdeasWrite, Scopes.CollectionsRead, Scopes.CollectionsWrite, Scopes.RoleAssignmentsWrite)]
        public async Task TestGetRandomIdeaInvalidScopes(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.GetAsync(GetRandomIdeaUri(CreateCollection())).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData("Administrator", Scopes.IdeasRead)]
        [InlineData("User", Scopes.IdeasRead)]
        public async Task TestGetIdeas(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection()).ConfigureAwait(false);
            var idea = await Factory.IdeaStore.StoreIdeaAsync(CreateNewIdea(collection.CollectionId)).ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.GetAsync(GetIdeasUri(collection)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var view = await response.Content.ReadAsAsync<TView[]>().ConfigureAwait(false);

            view.Should().NotBeNull().And.ContainEquivalentOf(this.Representer.ToView(idea));
        }

        [Theory]
        [InlineData("Administrator")]
        [InlineData("User")]
        [InlineData("Administrator", Scopes.IdeasWrite, Scopes.CollectionsRead, Scopes.CollectionsWrite, Scopes.RoleAssignmentsWrite)]
        [InlineData("User", Scopes.IdeasWrite, Scopes.CollectionsRead, Scopes.CollectionsWrite, Scopes.RoleAssignmentsWrite)]
        public async Task TestGetIdeasInvalidScopes(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.GetAsync(GetIdeasUri(CreateCollection())).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData("Administrator", Scopes.IdeasWrite)]
        [InlineData("User", Scopes.IdeasWrite)]
        public async Task TestAddIdea(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection()).ConfigureAwait(false);
            var idea = CreateNewIdea(collection.CollectionId);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.PostAsJsonAsync(GetIdeasUri(collection), this.Representer.ToView(idea)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var view = await response.Content.ReadAsAsync<TView>().ConfigureAwait(false);

            view.Should().NotBeNull().And.BeEquivalentTo(this.Representer.ToView(idea));

            var ideas = await Factory.IdeaStore.GetIdeasAsync(collection.CollectionId).ToEnumerable().ConfigureAwait(false);

            var lossyIdea = this.Representer.ToModel(this.Representer.ToView(idea));
            lossyIdea.CollectionId = collection.CollectionId;

            ideas.Should().ContainEquivalentOf(lossyIdea);
        }

        [Theory]
        [InlineData("Administrator")]
        [InlineData("User")]
        [InlineData("Administrator", Scopes.IdeasRead, Scopes.CollectionsRead, Scopes.CollectionsWrite, Scopes.RoleAssignmentsWrite)]
        [InlineData("User", Scopes.IdeasRead, Scopes.CollectionsRead, Scopes.CollectionsWrite, Scopes.RoleAssignmentsWrite)]
        public async Task TestAddIdeaInvalidScopes(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.PostAsJsonAsync(GetIdeasUri(CreateCollection()), this.Representer.ToView(CreateNewIdea())).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData("Administrator", Scopes.IdeasWrite)]
        [InlineData("User", Scopes.IdeasWrite)]
        public async Task TestRemoveIdea(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection()).ConfigureAwait(false);
            var idea = await Factory.IdeaStore.StoreIdeaAsync(CreateNewIdea(collection.CollectionId)).ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.DeleteAsync(GetIdeaUri(idea)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var storedIdea = await Factory.IdeaStore.GetIdeaAsync(collection.CollectionId, idea.Id).ConfigureAwait(false);
            storedIdea.Should().BeNull();
        }

        [Theory]
        [InlineData("Administrator", Scopes.IdeasWrite)]
        [InlineData("User", Scopes.IdeasWrite)]
        public async Task TestRemoveIdeaNotFound(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);
            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection()).ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.DeleteAsync(GetIdeaUri(CreateNewIdea(collection.CollectionId))).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            response.Content.Headers.ContentType.MediaType.Should().Be("application/problem+json");
        }

        [Theory]
        [InlineData("Administrator")]
        [InlineData("User")]
        [InlineData("Administrator", Scopes.IdeasRead, Scopes.CollectionsRead, Scopes.CollectionsWrite, Scopes.RoleAssignmentsWrite)]
        [InlineData("User", Scopes.IdeasRead, Scopes.CollectionsRead, Scopes.CollectionsWrite, Scopes.RoleAssignmentsWrite)]
        public async Task TestRemoveIdeaInvalidScopes(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.DeleteAsync(GetIdeaUri(CreateNewIdea())).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        protected virtual Uri GetIdeasUri(Collection collection)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            return new Uri($"/api/{Version}/ideas", UriKind.Relative);
        }

        protected virtual Uri GetIdeaUri(Idea idea)
        {
            if (idea is null)
            {
                throw new ArgumentNullException(nameof(idea));
            }

            return new Uri($"/api/{Version}/idea/{idea.Id.ToString("N", CultureInfo.InvariantCulture)}", UriKind.Relative);
        }

        protected virtual Uri GetRandomIdeaUri(Collection collection)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            return new Uri($"/api/{Version}/idea/random", UriKind.Relative);
        }

        protected Collection CreateCollection(Guid? principalId = null, Guid? collectionId = null)
        {
            return new Collection
            {
                CollectionId = collectionId ?? principalId ?? Tokens.PrincipalId,
                PrincipalId = principalId ?? Tokens.PrincipalId,
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
