﻿using System;
using Pipes.Constants;
using Pipes.Helpers;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.Builders
{
    public interface IValvedPipeBuilder<TReceive, TSend>
    {
        /// <summary>
        /// A function that, given the pipe, will produce the inlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe>, ISimpleInlet<TReceive>> Inlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe>, ISimpleOutlet<TSend>> Outlet { get; set; }

        IValvedPipe<TReceive, TSend, IPrioritisingTieBreaker> Build();
        ITieBreakingValvedPipeBuilder<TReceive, TSend, TTieBreaker> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker;
        ITieBreakingValvedPipeBuilder<TReceive, TSend, IAlternatingTieBreaker> WithAlternatingTieBreaker(Alternated alternated = Alternated.LeftHasPriorityInitially);
        ITieBreakingValvedPipeBuilder<TReceive, TSend, IPrioritisingTieBreaker> WithPrioritisingTieBreaker(Priority priority = Priority.Left);
        ITieBreakingValvedPipeBuilder<TReceive, TSend, IRandomisingTieBreaker> WithRandomisingTieBreaker(double leftProbability = 0.5);
    }

    public class ValvedPipeBuilder<TReceive, TSend> : IValvedPipeBuilder<TReceive, TSend>
    {
        public Func<Lazy<IPipe>, ISimpleInlet<TReceive>> Inlet { get; set; }
        public Func<Lazy<IPipe>, ISimpleOutlet<TSend>> Outlet { get; set; }

        public ValvedPipeBuilder()
        {
            Inlet = p => new SimpleInlet<TReceive>(p, SharedResourceHelpers.CreateSharedResource());
            Outlet = p => new SimpleOutlet<TSend>(p, SharedResourceHelpers.CreateSharedResource());
        }

        public IValvedPipe<TReceive, TSend, IPrioritisingTieBreaker> Build()
        {
            return CopyInletsAndOutletsTo(new TieBreakingValvedPipeBuilder<TReceive, TSend, IPrioritisingTieBreaker>(new PrioritisingTieBreaker(Priority.Left))).Build();
        }

        public ITieBreakingValvedPipeBuilder<TReceive, TSend, TTieBreaker> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker
        {
            return CopyInletsAndOutletsTo(new TieBreakingValvedPipeBuilder<TReceive, TSend, TTieBreaker>(tieBreaker));
        }

        public ITieBreakingValvedPipeBuilder<TReceive, TSend, IAlternatingTieBreaker> WithAlternatingTieBreaker(Alternated alternated = Alternated.LeftHasPriorityInitially)
        {
            return CopyInletsAndOutletsTo(new TieBreakingValvedPipeBuilder<TReceive, TSend, IAlternatingTieBreaker>(new AlternatingTieBreaker(alternated)));
        }

        public ITieBreakingValvedPipeBuilder<TReceive, TSend, IPrioritisingTieBreaker> WithPrioritisingTieBreaker(Priority priority = Priority.Left)
        {
            return CopyInletsAndOutletsTo(new TieBreakingValvedPipeBuilder<TReceive, TSend, IPrioritisingTieBreaker>(new PrioritisingTieBreaker(priority)));
        }

        public ITieBreakingValvedPipeBuilder<TReceive, TSend, IRandomisingTieBreaker> WithRandomisingTieBreaker(double leftProbability = 0.5)
        {
            if (leftProbability < 0 || leftProbability > 1) throw new ArgumentOutOfRangeException(nameof(leftProbability), "The left probability must be between 0 and 1 (inclusive)");
            return CopyInletsAndOutletsTo(new TieBreakingValvedPipeBuilder<TReceive, TSend, IRandomisingTieBreaker>(new RandomisingTieBreaker(leftProbability)));
        }

        private ITieBreakingValvedPipeBuilder<TReceive, TSend, TTieBreaker> CopyInletsAndOutletsTo<TTieBreaker>(ITieBreakingValvedPipeBuilder<TReceive, TSend, TTieBreaker> tieBreakingValvedPipeBuilder)
            where TTieBreaker : ITieBreaker
        {
            tieBreakingValvedPipeBuilder.Inlet = Inlet;
            tieBreakingValvedPipeBuilder.Outlet = Outlet;
            return tieBreakingValvedPipeBuilder;
        }
    }
}