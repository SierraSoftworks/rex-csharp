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
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Linq;

namespace Rex.Tests.Controllers
{
    public class AuthControllerTests
    {
        public AuthControllerTests(ITestOutputHelper testOutputHelper)
        {
            this.Factory = new RexAppFactory(testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper)));
            TestOutputHelper = testOutputHelper;
        }

        public ITestOutputHelper TestOutputHelper { get; }

        protected RexAppFactory Factory { get; }

        [Fact]
        public async Task TestUnauthorized()
        {
            var client = Factory.CreateClient();

            var response = await client.GetAsync(new Uri("/api/v1/auth", UriKind.Relative)).ConfigureAwait(false);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            response.Headers.WwwAuthenticate.Should().NotBeNull().And.ContainEquivalentOf(new AuthenticationHeaderValue("Bearer"));
        }

        [Theory]
        [InlineData("GET", "/api/v1/auth")]
        [InlineData("GET", "/api/v1/auth", "Authorization")]
        public async Task TestCors(string method, string endpoint, params string[] headers)
        {
            var client = Factory.CreateClient();

            using (var request = new HttpRequestMessage(HttpMethod.Options, endpoint))
            {
                request.Headers.Add("Access-Control-Request-Method", method);
                request.Headers.Add("Access-Control-Allow-Headers", string.Join(", ", headers));
                request.Headers.Add("Origin", "https://rex.sierrasoftworks.com");

                var response = await client.SendAsync(request).ConfigureAwait(false);
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault().Should().Contain("https://rex.sierrasoftworks.com");
                response.Headers.GetValues("Access-Control-Allow-Methods").FirstOrDefault().Should().Contain(method);
                response.Headers.GetValues("Access-Control-Allow-Credentials").FirstOrDefault().Should().Contain("true");

                if (headers.Any())
                    response.Headers.GetValues("Access-Control-Allow-Headers").FirstOrDefault().Should().ContainAll(headers);
            }
        }

        [Theory]
        [InlineData("Administrator")]
        [InlineData("User")]
        public async Task TestRoleClaims(string role)
        {
            var client = Factory.CreateAuthenticatedClient(role, "Ideas.Read", "Ideas.Write");

            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/auth"))
            {
                var response = await client.SendAsync(request).ConfigureAwait(false);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await response.Content.ReadAsAsync<Dictionary<string, object>>().ConfigureAwait(false);
                content.Should().ContainKey("http://schemas.microsoft.com/identity/claims/objectidentifier").WhoseValue.Should().Be(TestTokens.PrincipalId.ToString());
                content.Should().ContainKey("http://schemas.microsoft.com/ws/2008/06/identity/claims/role").WhoseValue.Should().Be(role);
                content.Should().ContainKey("http://schemas.microsoft.com/identity/claims/scope").WhoseValue.Should().Be("user_impersonation Ideas.Read Ideas.Write");
            }
        }

        [Theory]
        [InlineData("eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJhdWQiOiJ0ZXN0cyIsImlzcyI6InRlc3RzIiwiaWF0IjoxNTAwMDAwMDAwLCJuYmYiOjE1MDAwMDAwMDAsImV4cCI6MjUwMDAwMDAwMCwiYWNyIjoiMSIsImFtciI6WyJwd2QiLCJtZmEiXSwiYXBwaWQiOiI3YTZkNmVhZS02Y2VlLTQzZDYtYWY1Ny0zZGE5MDhiMGYxMjAiLCJhcHBpZGFjciI6IjAiLCJkZXZpY2VpZCI6IjYxMzRjOWM5LWI4OTMtNDY1MS05ZmQxLTM2MThkYjhkMWZjNCIsImZhbWlseV9uYW1lIjoiTWNUZXN0ZXJzb24iLCJnaXZlbl9uYW1lIjoiVGVzdHkiLCJuYW1lIjoiVGVzdHkgTWNUZXN0ZXJzb24iLCJvaWQiOiJkNmNmNWU3Zi1iMTJhLTQ0NGQtOGM3Zi02NzkwYjc3ZTQ5YTkiLCJyb2xlcyI6WyJBZG1pbmlzdHJhdG9yIl0sInNjcCI6InVzZXJfaW1wZXJzb25hdGlvbiBJZGVhcy5SZWFkIElkZWFzLldyaXRlIENvbGxlY3Rpb25zLlJlYWQgQ29sbGVjdGlvbnMuV3JpdGUgUm9sZUFzc2lnbm1lbnRzLldyaXRlIiwic3ViIjoiWHdxMnNRSkVZVWJ4a3dWXzBWOUdnX25JQVcybVdYOXRKbnRfR3Fya2RibSIsInRpZCI6IjZhZGEwNjFiLTE2NmMtNGE3My1iMzZkLWM3N2Q2Y2M4Y2FhNCIsInVuaXF1ZV9uYW1lIjoidGVzdHlAdGVzdGVyc29uLmNvbSIsInVwbiI6InRlc3R5QHRlc3RlcnNvbi5jb20iLCJ2ZXIiOiIxLjAifQ.raXl7qG0q-YeiLoNCPFnK2DVSpHtLR1STUoVyZFLH6U")]
        public async Task TestHardCodedTokenClaims(string token)
        {
            var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/auth"))
            {
                var response = await client.SendAsync(request).ConfigureAwait(false);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await response.Content.ReadAsAsync<Dictionary<string, object>>().ConfigureAwait(false);
                content.Should().ContainKey("http://schemas.microsoft.com/identity/claims/objectidentifier").WhoseValue.Should().Be(TestTokens.PrincipalId.ToString());
                content.Should().ContainKey("http://schemas.microsoft.com/ws/2008/06/identity/claims/role").WhoseValue.Should().Be("Administrator");
                content.Should().ContainKey("http://schemas.microsoft.com/identity/claims/scope").WhoseValue.Should().Be("user_impersonation Ideas.Read Ideas.Write Collections.Read Collections.Write RoleAssignments.Write");
            }
        }
    }
}
