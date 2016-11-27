using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Utilities;

namespace NWheels.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ConcatIf<T>(this IEnumerable<T> first, IEnumerable<T> secondOrNull) where T : class
        {
            if ( secondOrNull != null )
            {
                return first.Concat(secondOrNull);
            }
            else
            {
                return first;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, params T[] second)
        {
            return first.Concat((IEnumerable<T>)second);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<T> ConcatOne<T>(this IEnumerable<T> first, T second)
        {
            return first.Concat(new T[] { second });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<T> ConcatOneIf<T>(this IEnumerable<T> first, T second) where T : class
        {
            if ( second != null )
            {
                return first.Concat(new T[] { second });
            }
            else
            {
                return first;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<T> InjectDependenciesFrom<T>(this IEnumerable<T> source, IComponentContext components)
        {
            if ( components != null && source != null )
            {
                return new ObjectUtility.DependencyInjectingEnumerable<T>(source, components);
            }
            else
            {
                return source;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            var index = 0;

            foreach ( var item in source )
            {
                action(item, index);
                index++;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<IReadOnlyList<T>> TakeChunks<T>(this IEnumerable<T> source, int length)
        {
            return new ChunkingEnumerable<T>(source, length);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<string> ReadLines(this StreamReader input)
        {
            return new LineReadingEnumerable(input);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class LineReadingEnumerable : IEnumerable<string>
        {
            private readonly StreamReader _input;

            public LineReadingEnumerable(StreamReader input)
            {
                _input = input;
            }

            #region Implementation of IEnumerable

            public IEnumerator<string> GetEnumerator()
            {
                return new LineReadingEnumerator(_input);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class LineReadingEnumerator : IEnumerator<string>
        {
            private readonly StreamReader _input;
            private string _current;

            public LineReadingEnumerator(StreamReader input)
            {
                _input = input;
            }

            #region Implementation of IDisposable

            public void Dispose()
            {
                _input.Dispose();
            }

            #endregion

            #region Implementation of IEnumerator

            public bool MoveNext()
            {
                _current = _input.ReadLine();
                return (_current != null);
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public string Current
            {
                get { return _current; }
            }

            object IEnumerator.Current
            {
                get { return _current; }
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ChunkingEnumerable<T> : IEnumerable<IReadOnlyList<T>>
        {
            private readonly IEnumerable<T> _inner;
            private readonly int _length;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ChunkingEnumerable(IEnumerable<T> inner, int length)
            {
                _inner = inner;
                _length = length;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IEnumerable

            public IEnumerator<IReadOnlyList<T>> GetEnumerator()
            {
                return new ChunkingEnumerator<T>(_inner.GetEnumerator(), _length);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ChunkingEnumerator<T> : IEnumerator<IReadOnlyList<T>>
        {
            private readonly IEnumerator<T> _inner;
            private readonly int _length;
            private IReadOnlyList<T> _current;
            private bool _endOfInner;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ChunkingEnumerator(IEnumerator<T> inner, int length)
            {
                _inner = inner;
                _length = length;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IDisposable

            public void Dispose()
            {
                _inner.Dispose();
                _current = null;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IEnumerator

            public bool MoveNext()
            {
                var currentBuffer = new List<T>();

                while (currentBuffer.Count < _length && !_endOfInner)
                {
                    if (!_inner.MoveNext())
                    {
                        _endOfInner = true;
                        break;
                    }

                    currentBuffer.Add(_inner.Current);
                }

                if (currentBuffer.Count > 0)
                {
                    _current = currentBuffer;
                    return true;
                }

                _current = null;
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Reset()
            {
                _inner.Reset();
                _current = null;
                _endOfInner = false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IReadOnlyList<T> Current
            {
                get
                {
                    if (_current != null)
                    {
                        return _current;
                    }

                    throw new InvalidOperationException();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            #endregion
        }
    }
}
