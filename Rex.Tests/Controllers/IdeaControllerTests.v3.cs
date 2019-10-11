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
    public class IdeaControllerV3Tests
        : IdeaControllerTests<Idea.Version3>
    {
        public IdeaControllerV3Tests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        protected override string Version => "v3";

        [Theory]
        [InlineData("Administrator", RoleAssignment.Owner, Scopes.IdeasRead)]
        [InlineData("User", RoleAssignment.Owner, Scopes.IdeasRead)]
        [InlineData("Administrator", RoleAssignment.Contributor, Scopes.IdeasRead)]
        [InlineData("User", RoleAssignment.Contributor, Scopes.IdeasRead)]
        [InlineData("Administrator", RoleAssignment.Viewer, Scopes.IdeasRead)]
        [InlineData("User", RoleAssignment.Viewer, Scopes.IdeasRead)]
        public async Task TestGetIdeaCustomCollection(string role, string roleAssignmentRole, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection(collectionId: Guid.NewGuid())).ConfigureAwait(false);
            await Factory.RoleAssignmentStore.StoreRoleAssignmentAsync(new RoleAssignment
            {
                CollectionId = collection.CollectionId,
                PrincipalId = Tokens.PrincipalId,
                Role = roleAssignmentRole,
            }).ConfigureAwait(false);

            var idea = await Factory.IdeaStore.StoreIdeaAsync(CreateNewIdea(collection.CollectionId)).ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.GetAsync(GetIdeaUri(idea)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var view = await response.Content.ReadAsAsync<Idea.Version3>().ConfigureAwait(false);

            view.Should().NotBeNull().And.BeEquivalentTo(this.Representer.ToView(idea));
        }

        [Theory]
        [InlineData("Administrator", Scopes.IdeasRead)]
        [InlineData("User", Scopes.IdeasRead)]
        public async Task TestGetIdeaCustomCollectionNoRoleAssignment(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection(collectionId: Guid.NewGuid())).ConfigureAwait(false);
            var idea = await Factory.IdeaStore.StoreIdeaAsync(CreateNewIdea(collection.CollectionId)).ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.GetAsync(GetIdeaUri(idea)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData("Administrator", Scopes.IdeasWrite)]
        [InlineData("User", Scopes.IdeasWrite)]
        public async Task TestStoreIdeaCustomCollection(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection(collectionId: Guid.NewGuid())).ConfigureAwait(false);
            var idea = await Factory.IdeaStore.StoreIdeaAsync(CreateNewIdea(collection.CollectionId)).ConfigureAwait(false);
            await Factory.RoleAssignmentStore.StoreRoleAssignmentAsync(new RoleAssignment
            {
                CollectionId = collection.CollectionId,
                PrincipalId = Tokens.PrincipalId,
                Role = RoleAssignment.Owner,
            }).ConfigureAwait(false);

            idea.Description = "This is an updated description";
            idea.Completed = true;

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.PutAsJsonAsync(GetIdeaUri(idea), this.Representer.ToView(idea)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var view = await response.Content.ReadAsAsync<Idea.Version3>().ConfigureAwait(false);

            view.Should().NotBeNull().And.BeEquivalentTo(this.Representer.ToView(idea));
        }

        [Theory]
        [InlineData("Administrator", Scopes.IdeasWrite)]
        [InlineData("User", Scopes.IdeasWrite)]
        public async Task TestStoreIdeaCustomCollectionNotFound(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);
            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection(collectionId: Guid.NewGuid())).ConfigureAwait(false);
            var idea = CreateNewIdea(collection.CollectionId);
            idea.Id = Guid.NewGuid();

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.PutAsJsonAsync(GetIdeaUri(idea), this.Representer.ToView(idea)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData("Administrator", RoleAssignment.Viewer, Scopes.IdeasWrite)]
        [InlineData("User", RoleAssignment.Viewer, Scopes.IdeasWrite)]
        public async Task TestStoreIdeaCustomCollectionForbidden(string role, string roleAssignmentRole, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);
            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection(collectionId: Guid.NewGuid())).ConfigureAwait(false);
            await Factory.RoleAssignmentStore.StoreRoleAssignmentAsync(new RoleAssignment
            {
                CollectionId = collection.CollectionId,
                PrincipalId = Tokens.PrincipalId,
                Role = roleAssignmentRole,
            }).ConfigureAwait(false);

            var idea = CreateNewIdea(collection.CollectionId);
            idea.Id = Guid.NewGuid();

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.PutAsJsonAsync(GetIdeaUri(idea), this.Representer.ToView(idea)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData("Administrator", Scopes.IdeasWrite)]
        [InlineData("User", Scopes.IdeasWrite)]
        public async Task TestStoreIdeaNotFoundCustomCollection(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);
            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection(collectionId: Guid.NewGuid())).ConfigureAwait(false);
            await Factory.RoleAssignmentStore.StoreRoleAssignmentAsync(new RoleAssignment
            {
                CollectionId = collection.CollectionId,
                PrincipalId = Tokens.PrincipalId,
                Role = RoleAssignment.Owner,
            }).ConfigureAwait(false);
            var idea = CreateNewIdea(collection.CollectionId);
            idea.Id = Guid.NewGuid();

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.PutAsJsonAsync(GetIdeaUri(idea), this.Representer.ToView(idea)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var view = await response.Content.ReadAsAsync<Idea.Version3>().ConfigureAwait(false);

            view.Should().NotBeNull().And.BeEquivalentTo(this.Representer.ToView(idea));
        }

        [Theory]
        [InlineData("Administrator", RoleAssignment.Owner, Scopes.IdeasRead)]
        [InlineData("User", RoleAssignment.Owner, Scopes.IdeasRead)]
        [InlineData("Administrator", RoleAssignment.Contributor, Scopes.IdeasRead)]
        [InlineData("User", RoleAssignment.Contributor, Scopes.IdeasRead)]
        [InlineData("Administrator", RoleAssignment.Viewer, Scopes.IdeasRead)]
        [InlineData("User", RoleAssignment.Viewer, Scopes.IdeasRead)]
        public async Task TestGetIdeasNonDefaultCollection(string role, string roleAssignmentRole, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection(collectionId: Guid.NewGuid())).ConfigureAwait(false);
            var idea = await Factory.IdeaStore.StoreIdeaAsync(CreateNewIdea(collection.CollectionId)).ConfigureAwait(false);
            await Factory.RoleAssignmentStore.StoreRoleAssignmentAsync(new RoleAssignment
            {
                CollectionId = collection.CollectionId,
                PrincipalId = Tokens.PrincipalId,
                Role = roleAssignmentRole,
            }).ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.GetAsync(GetIdeasUri(collection)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var view = await response.Content.ReadAsAsync<Idea.Version3[]>().ConfigureAwait(false);

            view.Should().NotBeNull().And.ContainEquivalentOf(this.Representer.ToView(idea));
        }

        [Theory]
        [InlineData("Administrator", Scopes.IdeasRead)]
        [InlineData("User", Scopes.IdeasRead)]
        public async Task TestGetIdeasCollectionNotFound(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);
            var collection = CreateCollection(collectionId: Guid.NewGuid());

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.GetAsync(GetIdeasUri(collection)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData("Administrator", RoleAssignment.Owner, Scopes.IdeasWrite)]
        [InlineData("User", RoleAssignment.Owner, Scopes.IdeasWrite)]
        [InlineData("Administrator", RoleAssignment.Contributor, Scopes.IdeasWrite)]
        [InlineData("User", RoleAssignment.Contributor, Scopes.IdeasWrite)]
        public async Task TestAddIdeaNonDefaultCollection(string role, string collectionRole, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);
            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection(collectionId: Guid.NewGuid())).ConfigureAwait(false);
            await Factory.RoleAssignmentStore.StoreRoleAssignmentAsync(new RoleAssignment
            {
                CollectionId = collection.CollectionId,
                PrincipalId = Tokens.PrincipalId,
                Role = collectionRole,
            }).ConfigureAwait(false);

            var idea = CreateNewIdea();

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.PostAsJsonAsync(GetIdeasUri(collection), this.Representer.ToView(idea)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var view = await response.Content.ReadAsAsync<Idea.Version3>().ConfigureAwait(false);

            // The collection ID will have been updated, so make sure we do the same.
            idea.CollectionId = collection.CollectionId;
            view.Should().NotBeNull().And.BeEquivalentTo(this.Representer.ToView(idea));
        }

        [Theory]
        [InlineData("Administrator", Scopes.IdeasWrite)]
        [InlineData("User", Scopes.IdeasWrite)]
        public async Task TestAddIdeaCollectionNotFound(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);
            var collection = CreateCollection(collectionId: Guid.NewGuid());
            var idea = CreateNewIdea();

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.PostAsJsonAsync(GetIdeasUri(collection), this.Representer.ToView(idea)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData("Administrator", Scopes.IdeasWrite)]
        [InlineData("User", Scopes.IdeasWrite)]
        public async Task TestRemoveIdeaNonDefaultCollection(string role, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection(collectionId: Guid.NewGuid())).ConfigureAwait(false);
            var idea = await Factory.IdeaStore.StoreIdeaAsync(CreateNewIdea(collection.CollectionId)).ConfigureAwait(false);
            await Factory.RoleAssignmentStore.StoreRoleAssignmentAsync(new RoleAssignment
            {
                CollectionId = collection.CollectionId,
                PrincipalId = Tokens.PrincipalId,
                Role = RoleAssignment.Owner,
            }).ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.DeleteAsync(GetIdeaUri(idea)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var storedIdea = await Factory.IdeaStore.GetIdeaAsync(collection.CollectionId, idea.Id).ConfigureAwait(false);
            storedIdea.Should().BeNull();
        }

        [Theory]
        [InlineData("Administrator", RoleAssignment.Contributor, Scopes.IdeasWrite)]
        [InlineData("User", RoleAssignment.Contributor, Scopes.IdeasWrite)]
        [InlineData("Administrator", RoleAssignment.Viewer, Scopes.IdeasWrite)]
        [InlineData("User", RoleAssignment.Viewer, Scopes.IdeasWrite)]
        public async Task TestRemoveIdeaNonDefaultCollectionForbiddenRole(string role, string roleAssignmentRole, params string[] scopes)
        {
            await Factory.ClearAsync().ConfigureAwait(false);

            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection(collectionId: Guid.NewGuid())).ConfigureAwait(false);
            var idea = await Factory.IdeaStore.StoreIdeaAsync(CreateNewIdea(collection.CollectionId)).ConfigureAwait(false);
            await Factory.RoleAssignmentStore.StoreRoleAssignmentAsync(new RoleAssignment
            {
                CollectionId = collection.CollectionId,
                PrincipalId = Tokens.PrincipalId,
                Role = roleAssignmentRole,
            }).ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.DeleteAsync(GetIdeaUri(idea)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            var storedIdea = await Factory.IdeaStore.GetIdeaAsync(collection.CollectionId, idea.Id).ConfigureAwait(false);
            storedIdea.Should().NotBeNull();
        }

        protected override Uri GetIdeasUri(Collection collection)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (collection.CollectionId == Tokens.PrincipalId)
                return base.GetIdeasUri(collection);

            return new Uri($"/api/{Version}/collection/{collection.CollectionId.ToString("N", CultureInfo.InvariantCulture)}/ideas", UriKind.Relative);
        }

        protected override Uri GetIdeaUri(Idea idea)
        {
            if (idea is null)
            {
                throw new ArgumentNullException(nameof(idea));
            }

            if (idea.CollectionId == Tokens.PrincipalId)
                return base.GetIdeaUri(idea);

            return new Uri($"/api/{Version}/collection/{idea.CollectionId.ToString("N", CultureInfo.InvariantCulture)}/idea/{idea.Id.ToString("N", CultureInfo.InvariantCulture)}", UriKind.Relative);
        }

        protected override Uri GetRandomIdeaUri(Collection collection)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (collection.CollectionId == Tokens.PrincipalId)
                return base.GetRandomIdeaUri(collection);

            return new Uri($"/api/{Version}/collection/{collection.CollectionId.ToString("N", CultureInfo.InvariantCulture)}/idea/random", UriKind.Relative);
        }
    }
}
