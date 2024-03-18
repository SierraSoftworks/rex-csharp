namespace Rex.Tests.Controllers;

public abstract class HealthControllerTests<TView>
    : IClassFixture<WebApplicationFactory<Startup>>
    where TView : class, IView<Health>
{
    protected HealthControllerTests(ITestOutputHelper testOutputHelper)
    {
        this.Factory = new RexAppFactory(testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper)));
        this.Representer = this.Factory.Services.GetRequiredService<IRepresenter<Health, TView>>();
    }

    protected abstract string Version { get; }

    RexAppFactory Factory { get; }

    protected IRepresenter<Health, TView> Representer { get; }

    [Theory]
    [InlineData("GET", "/api/{Version}/health", "Authorization")]
    public async Task TestCors(string method, string endpoint, params string[] headers)
    {
        var client = Factory.CreateClient();

        using (var request = new HttpRequestMessage(HttpMethod.Options, endpoint?.Replace("{Version}", this.Version, StringComparison.Ordinal)))
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

    [Fact]
    public async Task TestGetHealth()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync(new Uri($"/api/{Version}/health", UriKind.Relative)).ConfigureAwait(true);
        response.EnsureSuccessStatusCode();

        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var view = await response.Content.ReadAsAsync<TView>().ConfigureAwait(true);
        var model = Representer.ToModel(view);

        model.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}
