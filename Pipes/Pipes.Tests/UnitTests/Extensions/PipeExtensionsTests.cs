﻿using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Extensions;
using Pipes.Models.Graphs;

namespace Pipes.Tests.UnitTests.Extensions
{
    [TestFixture]
    public class PipeExtensionsTests
    {
        [Test]
        public void CreateGraphOfPipeSystem_GivenASinglePipe_ReturnsAGraphRepresentingThatPipe()
        {
            // Arrange
            var basicPipe = PipeBuilder.New.BasicPipe<int>().Build();

            // Act
            var graph = basicPipe.CreateGraphOfPipeSystem();

            // Assert
            graph.VertexCount.Should().Be(3);
            graph.Vertices.Should().Contain(new[]
            {
                PipeGraphVertex<int>.Create(basicPipe),
                PipeGraphVertex<int>.Create(basicPipe.Inlet),
                PipeGraphVertex<int>.Create(basicPipe.Outlet)
            });

            graph.EdgeCount.Should().Be(2);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe.Inlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe.Outlet))).Should().Be(1);
        }

        [Test]
        public void CreateGraphOfPipeSystem_GivenPipesConnectedInALine_ReturnsAGraphWithVerticesConnectedInALine()
        {
            // Arrange
            var basicPipe1 = PipeBuilder.New.BasicPipe<int>().Build();
            var basicPipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            var basicPipe3 = PipeBuilder.New.BasicPipe<int>().Build();

            basicPipe1.Outlet.ConnectTo(basicPipe2.Inlet);
            basicPipe2.Outlet.ConnectTo(basicPipe3.Inlet);

            // Act
            var graph = basicPipe1.CreateGraphOfPipeSystem();

            // Assert
            graph.Vertices.Count().Should().Be(9);
            graph.Vertices.Should().Contain(new[]
            {
                PipeGraphVertex<int>.Create(basicPipe1),
                PipeGraphVertex<int>.Create(basicPipe1.Inlet),
                PipeGraphVertex<int>.Create(basicPipe1.Outlet),
                PipeGraphVertex<int>.Create(basicPipe2),
                PipeGraphVertex<int>.Create(basicPipe2.Inlet),
                PipeGraphVertex<int>.Create(basicPipe2.Outlet),
                PipeGraphVertex<int>.Create(basicPipe3),
                PipeGraphVertex<int>.Create(basicPipe3.Inlet),
                PipeGraphVertex<int>.Create(basicPipe3.Outlet)
            });

            graph.EdgeCount.Should().Be(8);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe1.Inlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe1))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe1)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe1.Outlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe1.Outlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe2.Inlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe2.Inlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe2))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe2)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe2.Outlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe2.Outlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe3.Inlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe3.Inlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe3))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe3)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe3.Outlet))).Should().Be(1);
        }

        [Test]
        public void CreateGraphOfPipeSystem_OfPipesFormingABinaryTree_ReturnsAGraphOfABinaryTree()
        {
            // Arrange
            var twoOutletPipe1 = PipeBuilder.New.EitherOutletPipe<int>().Build();
            var twoOutletPipe2 = PipeBuilder.New.EitherOutletPipe<int>().Build();
            var twoOutletPipe3 = PipeBuilder.New.EitherOutletPipe<int>().Build();

            twoOutletPipe1.LeftOutlet.ConnectTo(twoOutletPipe2.Inlet);
            twoOutletPipe1.RightOutlet.ConnectTo(twoOutletPipe3.Inlet);

            // Act
            var graph = twoOutletPipe1.CreateGraphOfPipeSystem();

            // Assert
            graph.Vertices.Count().Should().Be(12);
            graph.Vertices.Should().Contain(new[]
            {
                PipeGraphVertex<int>.Create(twoOutletPipe1),
                PipeGraphVertex<int>.Create(twoOutletPipe1.Inlet),
                PipeGraphVertex<int>.Create(twoOutletPipe1.LeftOutlet),
                PipeGraphVertex<int>.Create(twoOutletPipe1.RightOutlet),
                PipeGraphVertex<int>.Create(twoOutletPipe2),
                PipeGraphVertex<int>.Create(twoOutletPipe2.Inlet),
                PipeGraphVertex<int>.Create(twoOutletPipe2.LeftOutlet),
                PipeGraphVertex<int>.Create(twoOutletPipe2.RightOutlet),
                PipeGraphVertex<int>.Create(twoOutletPipe3),
                PipeGraphVertex<int>.Create(twoOutletPipe3.Inlet),
                PipeGraphVertex<int>.Create(twoOutletPipe3.LeftOutlet),
                PipeGraphVertex<int>.Create(twoOutletPipe3.RightOutlet)
            });

            graph.EdgeCount.Should().Be(11);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe1.Inlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe1))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe1)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe1.LeftOutlet))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe1)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe1.RightOutlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe1.LeftOutlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe2.Inlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe2.Inlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe2))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe2)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe2.LeftOutlet))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe2)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe2.RightOutlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe1.RightOutlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe3.Inlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe3.Inlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe3))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe3)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe3.LeftOutlet))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe3)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe3.RightOutlet))).Should().Be(1);
        }

        [Test]
        public void CreateGraphOfPipeSystem_GivenAPipeConnectedToItself_ReturnsAGraphWithASelfLoop()
        {
            // Arrange
            var basicPipe = PipeBuilder.New.BasicPipe<int>().Build();

            basicPipe.Inlet.ConnectedOutlet = basicPipe.Outlet;
            basicPipe.Outlet.ConnectedInlet = basicPipe.Inlet;

            // Act
            var graph = basicPipe.CreateGraphOfPipeSystem();

            // Assert
            graph.Vertices.Count().Should().Be(3);
            graph.Vertices.Should().Contain(new[]
            {
                PipeGraphVertex<int>.Create(basicPipe),
                PipeGraphVertex<int>.Create(basicPipe.Inlet),
                PipeGraphVertex<int>.Create(basicPipe.Outlet)
            });

            graph.EdgeCount.Should().Be(3);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe.Inlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe.Outlet))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe.Outlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe.Inlet))).Should().Be(1);
        }

        [Test]
        public void CreateGraphOfPipeSystem_GivenPipesFormingExactlyACycle_ReturnsAGraphWithACycle()
        {
            // Arrange
            var basicPipe1 = PipeBuilder.New.BasicPipe<int>().Build();
            var basicPipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            var basicPipe3 = PipeBuilder.New.BasicPipe<int>().Build();

            basicPipe1.Outlet.ConnectTo(basicPipe2.Inlet);
            basicPipe2.Outlet.ConnectTo(basicPipe3.Inlet);
            basicPipe3.Outlet.ConnectedInlet = basicPipe1.Inlet;
            basicPipe1.Inlet.ConnectedOutlet = basicPipe3.Outlet;

            // Act
            var graph = basicPipe1.CreateGraphOfPipeSystem();

            // Assert
            graph.Vertices.Count().Should().Be(9);
            graph.Vertices.Should().Contain(new[]
            {
                PipeGraphVertex<int>.Create(basicPipe1),
                PipeGraphVertex<int>.Create(basicPipe1.Inlet),
                PipeGraphVertex<int>.Create(basicPipe1.Outlet),
                PipeGraphVertex<int>.Create(basicPipe2),
                PipeGraphVertex<int>.Create(basicPipe2.Inlet),
                PipeGraphVertex<int>.Create(basicPipe2.Outlet),
                PipeGraphVertex<int>.Create(basicPipe3),
                PipeGraphVertex<int>.Create(basicPipe3.Inlet),
                PipeGraphVertex<int>.Create(basicPipe3.Outlet)
            });

            graph.EdgeCount.Should().Be(9);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe1.Inlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe1))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe1)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe1.Outlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe1.Outlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe2.Inlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe2.Inlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe2))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe2)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe2.Outlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe2.Outlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe3.Inlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe3.Inlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe3))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe3)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe3.Outlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe3.Outlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe1.Inlet))).Should().Be(1);
        }

        [Test]
        public void CreateGraphOfPipeSystem_GivenPipesIncludingACycle_ReturnsAGraphWithACycle()
        {
            // Arrange
            var basicPipe1 = PipeBuilder.New.BasicPipe<int>().Build();
            var basicPipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            var basicPipe3 = PipeBuilder.New.BasicPipe<int>().Build();
            var twoOutletPipe = PipeBuilder.New.EitherOutletPipe<int>().Build();

            basicPipe1.Outlet.ConnectTo(basicPipe2.Inlet);
            basicPipe2.Outlet.ConnectTo(twoOutletPipe.Inlet);
            twoOutletPipe.RightOutlet.ConnectTo(basicPipe3.Inlet);
            twoOutletPipe.LeftOutlet.ConnectedInlet = basicPipe1.Inlet;
            basicPipe1.Inlet.ConnectedOutlet = twoOutletPipe.LeftOutlet;

            // Act
            var graph = basicPipe1.CreateGraphOfPipeSystem();

            // Assert
            graph.Vertices.Count().Should().Be(13);
            graph.Vertices.Should().Contain(new[]
            {
                PipeGraphVertex<int>.Create(basicPipe1),
                PipeGraphVertex<int>.Create(basicPipe1.Inlet),
                PipeGraphVertex<int>.Create(basicPipe1.Outlet),
                PipeGraphVertex<int>.Create(basicPipe2),
                PipeGraphVertex<int>.Create(basicPipe2.Inlet),
                PipeGraphVertex<int>.Create(basicPipe2.Outlet),
                PipeGraphVertex<int>.Create(basicPipe3),
                PipeGraphVertex<int>.Create(basicPipe3.Inlet),
                PipeGraphVertex<int>.Create(basicPipe3.Outlet),
                PipeGraphVertex<int>.Create(twoOutletPipe),
                PipeGraphVertex<int>.Create(twoOutletPipe.Inlet),
                PipeGraphVertex<int>.Create(twoOutletPipe.LeftOutlet),
                PipeGraphVertex<int>.Create(twoOutletPipe.RightOutlet)
            });

            graph.EdgeCount.Should().Be(13);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe1.Inlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe1))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe1)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe1.Outlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe2.Inlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe2))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe2)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe2.Outlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe3.Inlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe3))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe3)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe3.Outlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe.Inlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe.LeftOutlet))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe.RightOutlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe1.Outlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe2.Inlet))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(basicPipe2.Outlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe.Inlet))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe.RightOutlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe3.Inlet))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe.LeftOutlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(basicPipe1.Inlet))).Should().Be(1);
        }

        [Test]
        public void CreateGraphOfPipeSystem_GivenATwoOutletPipeDoublyConnectedToATwoInletPipe_ReturnsTheCorrectGraph()
        {
            // Arrange
            var twoOutletPipe = PipeBuilder.New.EitherOutletPipe<int>().Build();
            var twoInletPipe = PipeBuilder.New.EitherInletPipe<int>().Build();

            twoOutletPipe.LeftOutlet.ConnectTo(twoInletPipe.LeftInlet);
            twoOutletPipe.RightOutlet.ConnectedInlet = twoInletPipe.RightInlet;
            twoInletPipe.RightInlet.ConnectedOutlet = twoOutletPipe.RightOutlet;

            // Act
            var graph = twoOutletPipe.CreateGraphOfPipeSystem();

            // Assert
            graph.Vertices.Count().Should().Be(8);
            graph.Vertices.Should().Contain(new[]
            {
                PipeGraphVertex<int>.Create(twoOutletPipe),
                PipeGraphVertex<int>.Create(twoOutletPipe.Inlet),
                PipeGraphVertex<int>.Create(twoOutletPipe.LeftOutlet),
                PipeGraphVertex<int>.Create(twoOutletPipe.RightOutlet),
                PipeGraphVertex<int>.Create(twoInletPipe),
                PipeGraphVertex<int>.Create(twoInletPipe.LeftInlet),
                PipeGraphVertex<int>.Create(twoInletPipe.RightInlet),
                PipeGraphVertex<int>.Create(twoInletPipe.Outlet)
            });

            graph.EdgeCount.Should().Be(8);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe.Inlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe.LeftOutlet))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoOutletPipe.RightOutlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoInletPipe.LeftInlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoInletPipe))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoInletPipe.RightInlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoInletPipe))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoInletPipe)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoInletPipe.Outlet))).Should().Be(1);

            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe.LeftOutlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoInletPipe.LeftInlet))).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(PipeGraphVertex<int>.Create(twoOutletPipe.RightOutlet)) && e.Target.Equals(PipeGraphVertex<int>.Create(twoInletPipe.RightInlet))).Should().Be(1);
        }
    }
}
