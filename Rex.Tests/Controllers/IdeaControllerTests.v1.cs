﻿using Rex.Models;
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
    }
}
