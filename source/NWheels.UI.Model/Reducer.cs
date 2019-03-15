using System.Net.NetworkInformation;

namespace NWheels.UI.Model
{
    public abstract class Reducer<T>
    {
        public T State => default(T);

        protected Reducer(T initialState)
        {
        }
    }
}
