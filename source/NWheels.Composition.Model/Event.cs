using System;

namespace NWheels.Composition.Model
{
    public class Event<TData>
    {
        public void Subscribe(Action<TData> listener)
        {
        }

        public void Publish(TData data)
        {
        }

        public static Event<TData> operator +(Event<TData> @event, Action<TData> listener)
        {
            return @event;
        }
    }
}