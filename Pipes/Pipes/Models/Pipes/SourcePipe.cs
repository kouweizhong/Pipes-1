﻿using System;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public interface ISourcePipe<TMessage> : IPipe
    {
        ISimpleOutlet<TMessage> Outlet { get; }
        Func<TMessage> MessageProducer { get; }
    }

    public class SourcePipe<TMessage> : SimplePipe<TMessage>, ISourcePipe<TMessage>
    {
        public ISimpleOutlet<TMessage> Outlet { get; }
        public Func<TMessage> MessageProducer { get; }

        public SourcePipe(ISimpleOutlet<TMessage> outlet, Func<TMessage> messageProducer) : base(new IInlet[] {}, new[] {outlet})
        {
            Outlet = outlet;
            MessageProducer = messageProducer;
        }

        protected override Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage)
        {
            return null;
        }

        protected override Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage)
        {
            return MessageProducer;
        }
    }
}
