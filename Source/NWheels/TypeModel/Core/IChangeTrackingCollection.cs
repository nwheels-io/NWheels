using System.Collections.Generic;

namespace NWheels.TypeModel.Core
{
    public interface IChangeTrackingCollection<T> : ICollection<T>
    {
        void GetChanges(out T[] added, out T[] changed, out T[] removed);
        bool IsChanged { get; }
    }
}
