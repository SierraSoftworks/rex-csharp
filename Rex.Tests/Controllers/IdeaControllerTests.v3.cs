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

            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection(Tokens.PrincipalId)).ConfigureAwait(false);
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

            var collection = await Factory.CollectionStore.StoreCollectionAsync(CreateCollection(Tokens.PrincipalId)).ConfigureAwait(false);
            var idea = await Factory.IdeaStore.StoreIdeaAsync(CreateNewIdea(collection.CollectionId)).ConfigureAwait(false);

            var client = Factory.CreateAuthenticatedClient(role, scopes);

            var response = await client.GetAsync(GetIdeaUri(idea)).ConfigureAwait(false);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
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
    }
}
