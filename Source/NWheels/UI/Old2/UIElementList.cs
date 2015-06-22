using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace NWheels.UI
{
    public class UIElementList<T> : IReadOnlyList<T>
        where T : IUIElement
    {
        private readonly IComponentContext _components;
        private readonly List<T> _innerList;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal UIElementList(IComponentContext components)
        {
            _components = components;
            _innerList = new List<T>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T Add()
        {
            var newItem = _components.Resolve<T>();
            _innerList.Add(newItem);
            return newItem;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T InsertAt(int index)
        {
            var newItem = _components.Resolve<T>();
            _innerList.Insert(index, newItem);
            return newItem;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerator<T> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T this[int index]
        {
            get { return _innerList[index]; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int Count
        {
            get { return _innerList.Count; }
        }
    }
}
