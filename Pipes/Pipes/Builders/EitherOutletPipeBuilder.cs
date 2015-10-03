﻿using System;
using Pipes.Constants;
using Pipes.Helpers;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.Builders
{
    public interface IEitherOutletPipeBuilder<TMessage>
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

        IEitherOutletPipe<IPrioritisingTieBreaker, TMessage> Build();
        ITieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker;
        ITieBreakingEitherOutletPipeBuilder<IAlternatingTieBreaker, TMessage> WithAlternatingTieBreaker(Alternated alternated = Alternated.LeftHasPriorityInitially);
        ITieBreakingEitherOutletPipeBuilder<IPrioritisingTieBreaker, TMessage> WithPrioritisingTieBreaker(Priority priority = Priority.Left);
        ITieBreakingEitherOutletPipeBuilder<IRandomisingTieBreaker, TMessage> WithRandomisingTieBreaker(double leftProbability = 0.5);
    }

    public class EitherOutletPipeBuilder<TMessage> : IEitherOutletPipeBuilder<TMessage>
    {
        public Func<Lazy<IPipe<TMessage>>, IInlet<TMessage>> Inlet { get; set; }
        public Func<Lazy<IPipe<TMessage>>, IOutlet<TMessage>> LeftOutlet { get; set; }
        public Func<Lazy<IPipe<TMessage>>, IOutlet<TMessage>> RightOutlet { get; set; }

        public EitherOutletPipeBuilder()
        {
            Inlet = p => new Inlet<TMessage>(p, SharedResourceHelpers.CreateSharedResource());
            LeftOutlet = p => new Outlet<TMessage>(p, SharedResourceHelpers.CreateSharedResource());
            RightOutlet = p => new Outlet<TMessage>(p, SharedResourceHelpers.CreateSharedResource());
        }

        public IEitherOutletPipe<IPrioritisingTieBreaker, TMessage> Build()
        {
            return CopyInletsAndOutletsTo(new TieBreakingEitherOutletPipeBuilder<IPrioritisingTieBreaker, TMessage>(new PrioritisingTieBreaker(Priority.Left))).Build();
        }

        public ITieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker
        {
            return CopyInletsAndOutletsTo(new TieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage>(tieBreaker));
        }

        public ITieBreakingEitherOutletPipeBuilder<IAlternatingTieBreaker, TMessage> WithAlternatingTieBreaker(Alternated alternated = Alternated.LeftHasPriorityInitially)
        {
            return CopyInletsAndOutletsTo(new TieBreakingEitherOutletPipeBuilder<IAlternatingTieBreaker, TMessage>(new AlternatingTieBreaker(alternated)));
        }

        public ITieBreakingEitherOutletPipeBuilder<IPrioritisingTieBreaker, TMessage> WithPrioritisingTieBreaker(Priority priority = Priority.Left)
        {
            return CopyInletsAndOutletsTo(new TieBreakingEitherOutletPipeBuilder<IPrioritisingTieBreaker, TMessage>(new PrioritisingTieBreaker(priority)));
        }

        public ITieBreakingEitherOutletPipeBuilder<IRandomisingTieBreaker, TMessage> WithRandomisingTieBreaker(double leftProbability = 0.5)
        {
            if (leftProbability < 0 || leftProbability > 1) throw new ArgumentOutOfRangeException("leftProbability", "The left probability must be between 0 and 1 (inclusive)");
            return CopyInletsAndOutletsTo(new TieBreakingEitherOutletPipeBuilder<IRandomisingTieBreaker, TMessage>(new RandomisingTieBreaker(leftProbability)));
        }

        private ITieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> CopyInletsAndOutletsTo<TTieBreaker>(ITieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> tieBreakingEitherOutletPipeBuilder)
            where TTieBreaker : ITieBreaker
        {
            tieBreakingEitherOutletPipeBuilder.Inlet = Inlet;
            tieBreakingEitherOutletPipeBuilder.LeftOutlet = LeftOutlet;
            tieBreakingEitherOutletPipeBuilder.RightOutlet = RightOutlet;
            return tieBreakingEitherOutletPipeBuilder;
        }
    }
}