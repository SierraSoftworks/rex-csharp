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

        protected override Uri GetIdeaUri(Idea idea)
        {
            if (idea is null)
            {
                throw new ArgumentNullException(nameof(idea));
            }

            return new Uri($"/api/{Version}/collection/{idea.CollectionId.ToString("N", CultureInfo.InvariantCulture)}/idea/{idea.Id.ToString("N", CultureInfo.InvariantCulture)}", UriKind.Relative);
        }
    }
}
