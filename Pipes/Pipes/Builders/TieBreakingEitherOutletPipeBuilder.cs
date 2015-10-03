﻿using System;
using Pipes.Helpers;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.Builders
{
    public interface ITieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> where TTieBreaker : ITieBreaker
    {
        /// <summary>
        /// A function that, given the pipe, will produce the inlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe<TMessage>>, IInlet<TMessage>> Inlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the left outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe<TMessage>>, IOutlet<TMessage>> LeftOutlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the right outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe<TMessage>>, IOutlet<TMessage>> RightOutlet { get; set; }

        TTieBreaker TieBreaker { get; set; }

        IEitherOutletPipe<TTieBreaker, TMessage> Build();
    }

    public class TieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> : ITieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> where TTieBreaker : ITieBreaker
    {
        public Func<Lazy<IPipe<TMessage>>, IInlet<TMessage>> Inlet { get; set; }
        public Func<Lazy<IPipe<TMessage>>, IOutlet<TMessage>> LeftOutlet { get; set; }
        public Func<Lazy<IPipe<TMessage>>, IOutlet<TMessage>> RightOutlet { get; set; }
        public TTieBreaker TieBreaker { get; set; }

        public TieBreakingEitherOutletPipeBuilder(TTieBreaker tieBreaker)
        {
            Inlet = p => new Inlet<TMessage>(p, SharedResourceHelpers.CreateSharedResource());
            LeftOutlet = p => new Outlet<TMessage>(p, SharedResourceHelpers.CreateSharedResource());
            RightOutlet = p => new Outlet<TMessage>(p, SharedResourceHelpers.CreateSharedResource());
            TieBreaker = tieBreaker;
        }

        public IEitherOutletPipe<TTieBreaker, TMessage> Build()
        {
            EitherOutletPipe<TTieBreaker, TMessage>[] pipe = { null };
            var lazyPipe = new Lazy<IPipe<TMessage>>(() => pipe[0]);

            var inlet = Inlet(lazyPipe);
            var leftOutlet = LeftOutlet(lazyPipe);
            var rightOutlet = RightOutlet(lazyPipe);

            pipe[0] = new EitherOutletPipe<TTieBreaker, TMessage>(inlet, leftOutlet, rightOutlet, TieBreaker);

            return pipe[0];
        }
    }
}