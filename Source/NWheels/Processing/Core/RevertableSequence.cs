using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.Processing.Fluent.RevertableSequence;

namespace NWheels.Processing.Core
{
    internal class RevertableSequence : IRevertableSequence, IRevertableSequenceBuilder
    {
        private readonly IRevertableSequenceCodeBehind _codeBehind;
        private readonly List<StepBuilder> _stepBuilders;
        private readonly Stack<Step> _performedSteps;
        private RevertableSequenceState _state;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RevertableSequence(IRevertableSequenceCodeBehind codeBehind)
        {
            _codeBehind = codeBehind;
            _stepBuilders = new List<StepBuilder>();
            _performedSteps = new Stack<Step>();
            _state = RevertableSequenceState.NotPerformed;

            _codeBehind.BuildSequence(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IOnceFluent IRevertableSequenceBuilder.Once()
        {
            var builder = new SingleStepBuilder();
            _stepBuilders.Add(builder);
            return builder;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IItemFluent<T> IRevertableSequenceBuilder.ForEach<T>(Func<ICollection<T>> collectionFactory)
        {
            var builder = new CollectionStepBuilder<T>(collectionFactory);
            _stepBuilders.Add(builder);
            return builder;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Perform()
        {
            _performedSteps.Clear();

            try
            {
                foreach ( var builder in _stepBuilders )
                {
                    var steps = builder.GetSteps();

                    foreach ( var step in steps )
                    {
                        step.Perform();
                        _performedSteps.Push(step);
                    }
                }

                _state = RevertableSequenceState.Performed;
            }
            catch ( Exception performException )
            {
                try
                {
                    Revert();
                }
                catch ( Exception revertException )
                {
                    _state = RevertableSequenceState.RevertFailed;
                    throw new AggregateException(performException, revertException).Flatten();
                }

                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Revert()
        {
            var revertExceptions = new List<Exception>();
            var stepsFailedToRevert = new Stack<Step>();

            while ( _performedSteps.Count > 0 ) // LIFO loop
            {
                var step = _performedSteps.Pop();

                try
                {
                    step.Revert();
                }
                catch ( Exception e )
                {
                    revertExceptions.Add(e);
                    stepsFailedToRevert.Push(step);
                }
            }

            if ( revertExceptions.Count == 0 )
            {
                _state = RevertableSequenceState.Reverted;
            }
            else
            {
                _state = RevertableSequenceState.RevertFailed;

                foreach ( var step in stepsFailedToRevert )
                {
                    _performedSteps.Push(step);
                }

                throw new AggregateException(revertExceptions);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RevertableSequenceState State
        {
            get
            {
                return _state;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class Step
        {
            public Step(Action perform, Action revert)
            {
                this.Perform = perform;
                this.Revert = revert;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Action Perform { get; private set; }
            public Action Revert { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private abstract class StepBuilder
        {
            public abstract Step[] GetSteps();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class SingleStepBuilder : StepBuilder, IOnceFluent, IOnceRevertFluent
        {
            private Action _perform = null;
            private Action _revert = null;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IOnceRevertFluent IOnceFluent.OnPerform(Action performAction)
            {
                _perform = performAction;
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IOnceRevertFluent.OnRevert(Action revertAction)
            {
                _revert = revertAction;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override Step[] GetSteps()
            {
                return new[] {
                    new Step(
                        perform: () => {
                            if ( _perform != null )
                            {
                                _perform();
                            }
                        }, 
                        revert: () => {
                            if ( _revert != null )
                            {
                                _revert();
                            }
                        })
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class CollectionStepBuilder<T> : StepBuilder, IItemFluent<T>, IItemRevertFluent<T>
        {
            private readonly Func<ICollection<T>> _collectionFactory;
            private RevertableSequenceItemAction<T> _perform = null;
            private RevertableSequenceItemAction<T> _revert = null;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public CollectionStepBuilder(Func<ICollection<T>> collectionFactory)
            {
                _collectionFactory = collectionFactory;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IItemRevertFluent<T> IItemFluent<T>.OnPerform(RevertableSequenceItemAction<T> performAction)
            {
                _perform = performAction;
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IItemRevertFluent<T>.OnRevert(RevertableSequenceItemAction<T> revertAction)
            {
                _revert = revertAction;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override Step[] GetSteps()
            {
                var collection = _collectionFactory();

                if ( collection == null )
                {
                    return new Step[0];
                }

                return collection.Select((item, index) => new Step(
                    perform: () => {
                        if ( _perform != null )
                        {
                            _perform(item, index, isLastItem: index == collection.Count - 1);
                        }
                    },
                    revert: () => {
                        if ( _revert != null )
                        {
                            _revert(item, index, isLastItem: index == collection.Count - 1);
                        }
                    })).ToArray();
            }
        }
    }
}
