﻿using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Tests.Helpers;

namespace Pipes.Tests.UnitTests.Models.Pipes
{
    [TestFixture]
    public class CapacityPipeTests
    {
        private ICapacityPipe<int> capacityZeroPipe;
        private ICapacityPipe<int> capacityTwoPipe;
        private ICapacityPipe<int> capacityThreePipe;
        
        [SetUp]
        public void SetUp()
        {
            capacityZeroPipe = PipeBuilder.New.CapacityPipe<int>().WithCapacity(0).Build();
            capacityTwoPipe = PipeBuilder.New.CapacityPipe<int>().WithCapacity(2).Build();
            capacityThreePipe = PipeBuilder.New.CapacityPipe<int>().WithCapacity(3).Build();
        }

        [Test]
        public void CapacityPipe_HasOneInlet()
        {
            // Assert
            capacityZeroPipe.Inlet.Should().NotBeNull();
            capacityZeroPipe.ConnectableInlets.Count.Should().Be(1);
            capacityZeroPipe.ConnectableInlets.Single().Should().Be(capacityZeroPipe.Inlet);
        }

        [Test]
        public void CapacityPipe_HasOneOutlet()
        {
            // Assert
            capacityZeroPipe.Outlet.Should().NotBeNull();
            capacityZeroPipe.ConnectableOutlets.Count.Should().Be(1);
            capacityZeroPipe.ConnectableOutlets.Single().Should().Be(capacityZeroPipe.Outlet);
        }

        [Test]
        public void FindReceiver_GivenThereIsNoReceiver_ReturnsNull()
        {
            // Act
            var receiver = capacityZeroPipe.FindReceiver(capacityZeroPipe.Inlet);

            // Assert
            receiver.Should().BeNull();
        }

        [Test]
        public void FindReceiver_GivenThereIsAReceiver_ReturnsThatReceiver()
        {
            // Arrange
            const int message = 3;
            var receivedMessage = default(int);
            var thread = new Thread(() =>
            {
                receivedMessage = capacityZeroPipe.Outlet.Receive();
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var receiver = capacityZeroPipe.FindReceiver(capacityZeroPipe.Inlet);

            // Assert
            receiver.Should().NotBeNull();

            // Act
            receiver(message);
            Thread.Sleep(500);

            // Assert
            receivedMessage.Should().Be(message);
        }

        [Test]
        public void FindSender_GivenThereIsNoSender_ReturnsNull()
        {
            // Act
            var sender = capacityZeroPipe.FindSender(capacityZeroPipe.Outlet);

            // Assert
            sender.Should().BeNull();
        }

        [Test]
        public void FindSender_GivenThereIsASender_ReturnsThatSender()
        {
            // Arrange
            const int message = 3;
            var thread = new Thread(() =>
            {
                capacityZeroPipe.Inlet.Send(message);
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var sender = capacityZeroPipe.FindSender(capacityZeroPipe.Outlet);

            // Assert
            sender.Should().NotBeNull();

            // Act
            var receivedMessage = sender();

            // Assert
            receivedMessage.Should().Be(message);
        }

        [Test]
        public void FindReceiver_GivenThereIsAPipeConnectedToItsOutlet_AsksThatPipeForAReceiver()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            var mockInlet = (IInlet<int>)mockPipe.Object.ConnectableInlets.Single();

            capacityZeroPipe.Outlet.ConnectTo(mockInlet);

            // Act
            capacityZeroPipe.FindReceiver(capacityZeroPipe.Inlet);

            // Assert
            mockPipe.Verify(p => p.FindReceiver(mockInlet));
        }

        [Test]
        public void FindSender_GivenThereIsAPipeConnectedToItsInlet_AsksThatPipeForASender()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            var mockOutlet = (IOutlet<int>)mockPipe.Object.ConnectableOutlets.Single();

            capacityZeroPipe.Inlet.ConnectTo(mockOutlet);

            // Act
            capacityZeroPipe.FindSender(capacityZeroPipe.Outlet);

            // Assert
            mockPipe.Verify(p => p.FindSender(mockOutlet));
        }

        [Test]
        public void FindReceiver_GivenThePipeHasSpareCapacity_ReturnsAReceiver()
        {
            // Act
            var receiver = capacityThreePipe.FindReceiver(capacityThreePipe.Inlet);

            // Assert
            receiver.Should().NotBeNull();
        }

        [Test]
        public void FindReceiver_GivenThePipeHasAReceiverOnItsOutlet_PrefersThatReceiver()
        {
            // Arrange
            const int message = 3;
            var receivedMessage = default(int);
            var thread = new Thread(() =>
            {
                receivedMessage = capacityThreePipe.Outlet.Receive();
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var receiver = capacityThreePipe.FindReceiver(capacityThreePipe.Inlet);

            // Assert
            receiver.Should().NotBeNull();

            // Act
            receiver(message);
            Thread.Sleep(500);

            // Assert
            receivedMessage.Should().Be(message);
        }

        [Test]
        public void FindReceiver_GivenThePipeHasAReceiverFromAConnectedPipe_PrefersThatReceiver()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            var mockInlet = (IInlet<int>)mockPipe.Object.ConnectableInlets.Single();

            capacityThreePipe.Outlet.ConnectTo(mockInlet);
            var receivedMessage = 0;
            var expectedReceiver = new Action<int>(m => { receivedMessage = m; });
            mockPipe.Setup(p => p.FindReceiver(mockInlet)).Returns(expectedReceiver);

            // Act
            var actualReceiver = capacityThreePipe.FindReceiver(capacityThreePipe.Inlet);
            const int message = 15;
            actualReceiver(message);

            // Assert
            receivedMessage.Should().Be(message);
        }

        [Test]
        public void FindSender_GivenThePipeIsHoldingAMessage_ReturnsThatMessage()
        {
            // Arrange
            const int message = 14;
            capacityThreePipe.Inlet.Send(message);

            // Act
            var sender = capacityThreePipe.FindSender(capacityThreePipe.Outlet);

            // Assert
            sender.Should().NotBeNull();
            sender().Should().Be(message);
        }

        [Test]
        public void FindSender_GivenThePipeIsHoldingMessages_ReturnsMessagesInTheFirstInFirstOutOrder()
        {
            // Arrange
            const int expectedFirstMessage = 1;
            const int expectedSecondMessage = 2;
            const int expectedThirdMessage = 3;
            capacityThreePipe.FindReceiver(capacityThreePipe.Inlet)(expectedFirstMessage);
            capacityThreePipe.FindReceiver(capacityThreePipe.Inlet)(expectedSecondMessage);
            capacityThreePipe.FindReceiver(capacityThreePipe.Inlet)(expectedThirdMessage);

            // Act
            var actualFirstMessage = capacityThreePipe.FindSender(capacityThreePipe.Outlet)();
            var actualSecondMessage = capacityThreePipe.FindSender(capacityThreePipe.Outlet)();
            var actualThirdMessage = capacityThreePipe.FindSender(capacityThreePipe.Outlet)();

            // Assert
            actualFirstMessage.Should().Be(expectedFirstMessage);
            actualSecondMessage.Should().Be(expectedSecondMessage);
            actualThirdMessage.Should().Be(expectedThirdMessage);
        }

        [Test]
        public void FindReceiver_GivenThePipeIsFull_ReturnsNull()
        {
            // Arrange
            capacityThreePipe.FindReceiver(capacityThreePipe.Inlet)(1);
            capacityThreePipe.FindReceiver(capacityThreePipe.Inlet)(2);
            capacityThreePipe.FindReceiver(capacityThreePipe.Inlet)(3);

            // Act
            var receiver = capacityThreePipe.FindReceiver(capacityThreePipe.Inlet);

            // Assert
            receiver.Should().BeNull();
        }

        [Test]
        public void FindSender_AppliedToConnectedCapacityPipes_ReturnsTheMessagesFromAllConnectedPipesInTheRightOrder()
        {
            // Arrange
            capacityThreePipe.Outlet.ConnectTo(capacityTwoPipe.Inlet);

            const int expectedFirstMessage = 1;
            const int expectedSecondMessage = 2;
            const int expectedThirdMessage = 3;
            const int expectedFourthMessage = 4;
            const int expectedFifthMessage = 5;
            const int expectedSixthMessage = 6;

            capacityThreePipe.FindReceiver(capacityThreePipe.Inlet)(expectedFirstMessage);
            capacityThreePipe.FindReceiver(capacityThreePipe.Inlet)(expectedSecondMessage);
            capacityThreePipe.FindReceiver(capacityThreePipe.Inlet)(expectedThirdMessage);
            capacityThreePipe.FindReceiver(capacityThreePipe.Inlet)(expectedFourthMessage);
            capacityThreePipe.FindReceiver(capacityThreePipe.Inlet)(expectedFifthMessage);

            var thread = new Thread(() => capacityThreePipe.Inlet.Send(expectedSixthMessage));
            thread.Start();
            Thread.Sleep(500);

            // Act
            var actualFirstMessage = capacityTwoPipe.FindSender(capacityTwoPipe.Outlet)();
            var actualSecondMessage = capacityTwoPipe.FindSender(capacityTwoPipe.Outlet)();
            var actualThirdMessage = capacityTwoPipe.FindSender(capacityTwoPipe.Outlet)();
            var actualFourthMessage = capacityTwoPipe.FindSender(capacityTwoPipe.Outlet)();
            var actualFifthMessage = capacityTwoPipe.FindSender(capacityTwoPipe.Outlet)();
            var actualSixthMessage = capacityTwoPipe.FindSender(capacityTwoPipe.Outlet)();

            // Assert
            actualFirstMessage.Should().Be(expectedFirstMessage);
            actualSecondMessage.Should().Be(expectedSecondMessage);
            actualThirdMessage.Should().Be(expectedThirdMessage);
            actualFourthMessage.Should().Be(expectedFourthMessage);
            actualFifthMessage.Should().Be(expectedFifthMessage);
            actualSixthMessage.Should().Be(expectedSixthMessage);
        }

        [Test]
        public void StoredMessages_GivenTheCapacityPipeHasNoMessages_ReturnsAnEmptyList()
        {
            // Act
            var storedMessages = capacityThreePipe.StoredMessages;

            // Assert
            storedMessages.Should().BeEmpty();
        }

        [Test]
        public void StoredMessages_GivenTheCapacityPipeHasMessages_ReturnsThoseMessagesInOrder()
        {
            // Arrange
            capacityThreePipe.Inlet.Send(1);
            capacityThreePipe.Inlet.Send(2);
            capacityThreePipe.Inlet.Send(3);

            // Act
            var storedMessages = capacityThreePipe.StoredMessages;

            // Assert
            storedMessages.Count.Should().Be(3);
            storedMessages[0].Should().Be(1);
            storedMessages[1].Should().Be(2);
            storedMessages[2].Should().Be(3);
        }
    }
}
