﻿using System;
using System.Collections.Generic;
using Pipes.Models.Lets;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface ITwoInletPipe<TMessage> : IPipe<TMessage>
    {
        Inlet<TMessage> LeftInlet { get; }
        Inlet<TMessage> RightInlet { get; }
        Outlet<TMessage> Outlet { get; }
    }

    public abstract class TwoInletPipe<TMessage> : ITwoInletPipe<TMessage>
    {
        public Inlet<TMessage> LeftInlet { get; private set; }
        public Inlet<TMessage> RightInlet { get; private set; }
        public Outlet<TMessage> Outlet { get; private set; }

        protected TwoInletPipe()
        {
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var leftInletResource = resourceGroup.CreateAndAcquireSharedResource();
            var rightInletResource = resourceGroup.CreateAndAcquireSharedResource();
            var outletResource = resourceGroup.CreateAndAcquireSharedResource();
            var pipeResource = resourceGroup.CreateAndAcquireSharedResource();

            pipeResource.AssociatedObject = this;

            resourceGroup.ConnectSharedResources(leftInletResource, pipeResource);
            resourceGroup.ConnectSharedResources(rightInletResource, pipeResource);
            resourceGroup.ConnectSharedResources(pipeResource, outletResource);

            LeftInlet = new Inlet<TMessage>(this, leftInletResource);
            RightInlet = new Inlet<TMessage>(this, rightInletResource);
            Outlet = new Outlet<TMessage>(this, outletResource);
            
            resourceGroup.FreeSharedResources();
        }

        public IReadOnlyCollection<Inlet<TMessage>> Inlets
        {
            get { return new[] {LeftInlet, RightInlet}; }
        }

        public IReadOnlyCollection<Outlet<TMessage>> Outlets
        {
            get { return new[] {Outlet}; }
        }

        public abstract Action<TMessage> FindReceiver();
        public abstract Func<TMessage> FindSender();
    }
}