using System;
using System.Collections.Generic;

namespace NWheels.Microservices.Orchestration
{
    public interface IRevertableSequenceBuilder
    {
        Fluent.RevertableSequence.IOnceFluent Once();
        Fluent.RevertableSequence.IItemFluent<T> ForEach<T>(Func<ICollection<T>> collectionFactory);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public delegate void RevertableSequenceItemAction<in T>(T item, int itemIndex, bool isLastItem);

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    namespace Fluent.RevertableSequence
    {
        public interface IOnceFluent : IOnceRevertFluent
        {
            IOnceRevertFluent OnPerform(Action performAction);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IOnceRevertFluent
        {
            void OnRevert(Action revertAction);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IItemFluent<T> : IItemRevertFluent<T>
        {
            IItemRevertFluent<T> OnPerform(RevertableSequenceItemAction<T> performAction);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IItemRevertFluent<T>
        {
            void OnRevert(RevertableSequenceItemAction<T> revertAction);
        }
    }
}
