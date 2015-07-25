using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.DataObjects.Core;

namespace NWheels.Utilities
{
    public static class ObjectUtility
    {
        public static void InjectDependenciesToObject<T>(T obj, IComponentContext components)
        {
            if ( components != null )
            {
                var dependant = obj as IHaveDependencies;

                if ( dependant != null )
                {
                    dependant.InjectDependencies(components);
                }

                var composite = obj as IHaveNestedObjects;

                if ( composite != null )
                {
                    var nestedObjects = new HashSet<object>();
                    composite.DeepListNestedObjects(nestedObjects);

                    foreach ( var nestedDependant in nestedObjects.OfType<IHaveDependencies>() )
                    {
                        nestedDependant.InjectDependencies(components);
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void InjectDependenciesToManyObjects<T>(IEnumerable<T> source, IComponentContext components)
        {
            if ( components != null && source != null )
            {
                foreach ( var obj in source )
                {
                    InjectDependenciesToObject(obj, components);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DependencyInjectingEnumerable<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _source;
            private readonly IComponentContext _components;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DependencyInjectingEnumerable(IEnumerable<T> source, IComponentContext components)
            {
                _source = source;
                _components = components;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerator<T> GetEnumerator()
            {
                return new DependencyInjectingEnumerator<T>(_source.GetEnumerator(), _components);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DependencyInjectingEnumerator<T> : IEnumerator<T>
        {
            private readonly IEnumerator<T> _source;
            private readonly IComponentContext _components;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DependencyInjectingEnumerator(IEnumerator<T> source, IComponentContext components)
            {
                _source = source;
                _components = components;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                _source.Dispose();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool MoveNext()
            {
                var result = _source.MoveNext();

                if ( result )
                {
                    ObjectUtility.InjectDependenciesToObject(_source.Current, _components);
                }

                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Reset()
            {
                _source.Dispose();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public T Current
            {
                get
                {
                    return _source.Current;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            object System.Collections.IEnumerator.Current
            {
                get { return this.Current; }
            }
        }
    }
}

