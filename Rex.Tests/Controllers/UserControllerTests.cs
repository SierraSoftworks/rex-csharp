namespace Rex.Tests.Controllers;

public abstract class UserControllerTests<TView>
    : IClassFixture<WebApplicationFactory<Startup>>
    where TView : class, IView<User>
{
    protected UserControllerTests(ITestOutputHelper testOutputHelper)
    {
        this.Factory = new RexAppFactory(testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper)));
        this.Representer = this.Factory.Services.GetRequiredService<IRepresenter<User, TView>>();
        TestOutputHelper = testOutputHelper;
    }

    protected abstract string Version { get; }

    public ITestOutputHelper TestOutputHelper { get; }

    protected RexAppFactory Factory { get; }

    public IRepresenter<User, TView> Representer { get; }

    [Fact]
    public async Task TestUnauthorized()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync(new Uri("/api/v3/user/37b2dd1da1a74fda515b862567c422ef", UriKind.Relative)).ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Headers.WwwAuthenticate.Should().NotBeNull().And.ContainEquivalentOf(new AuthenticationHeaderValue("Bearer"));
    }

    [Theory]
    [InlineData("GET", "/api/{Version}/user/{EmailHash}")]
    [InlineData("GET", "/api/{Version}/user/{EmailHash}", "Authorization")]
    public async Task TestCors(string method, string endpoint, params string[] headers)
    {
        var client = Factory.CreateClient();

        using (var request = new HttpRequestMessage(HttpMethod.Options, endpoint?.Replace("{Version}", this.Version, StringComparison.Ordinal)?.Replace("{EmailHash}", TestTokens.EmailHash, StringComparison.Ordinal)))
        {
            request.Headers.Add("Access-Control-Request-Method", method);
            request.Headers.Add("Access-Control-Allow-Headers", string.Join(", ", headers));
            request.Headers.Add("Origin", "https://rex.sierrasoftworks.com");

            var response = await client.SendAsync(request).ConfigureAwait(true);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault().Should().Contain("https://rex.sierrasoftworks.com");
            response.Headers.GetValues("Access-Control-Allow-Methods").FirstOrDefault().Should().Contain(method);
            response.Headers.GetValues("Access-Control-Allow-Credentials").FirstOrDefault().Should().Contain("true");

            if (headers?.Length != 0)
                response.Headers.GetValues("Access-Control-Allow-Headers").FirstOrDefault().Should().ContainAll(headers);
        }
    }

    [Theory]
    [InlineData("Administrator")]
    [InlineData("User")]
    public async Task TestGetExistingUser(string role)
    {
        var user = await Factory.UserStore.StoreUserAsync(new User {
            PrincipalId = TestTokens.PrincipalId,
            EmailHash = TestTokens.EmailHash,
            FirstName = "Testy"
        }).ConfigureAwait(true);

        var client = Factory.CreateAuthenticatedClient(role, "Users.Read");

        using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/{Version}/user/{user.EmailHash}"))
        {
            var response = await client.SendAsync(request).ConfigureAwait(true);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsAsync<User.Version3>().ConfigureAwait(true);
            content.Should().BeEquivalentTo(this.Representer.ToView(user));
        }
    }
}
