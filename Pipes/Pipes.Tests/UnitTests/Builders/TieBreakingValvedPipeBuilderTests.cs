﻿using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.TieBreakers;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class TieBreakingValvedPipeBuilderTests
    {
        [Test]
        public void Build_ReturnsAPipeWithTheTieBreakerPassedIntoTheBuildersConstructor()
        {
            // Arrange
            var tieBreaker = new Mock<ITwoWayTieBreaker>().Object;

            // Act
            var pipe = new TieBreakingValvedPipeBuilder<string, int, ITwoWayTieBreaker>(tieBreaker).Build();

            // Assert
            pipe.TieBreaker.Should().Be(tieBreaker);
            pipe.Valve.Should().NotBeNull();
            pipe.Inlet.Should().NotBeNull();
            pipe.Outlet.Should().NotBeNull();
        }
    }
}