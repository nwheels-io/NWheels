using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Features.Metadata;
using NWheels.Extensions;

namespace NWheels
{
    /// <summary>
    /// Represents a pipeline of components implementing TService contract
    /// </summary>
    /// <remarks>
    /// The pipeline is instantiated by resolving type Pipeline[TService] from DI container
    /// </remarks>
    public class Pipeline<TService> : IReadOnlyList<TService>
    {
        private readonly IReadOnlyList<TService> _orderedComponents;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        internal Pipeline(IEnumerable<TService> orderedComponents)
        {
            _orderedComponents = orderedComponents.ToArray();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEnumerable

        public IEnumerator<TService> GetEnumerator()
        {
            return _orderedComponents.GetEnumerator();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _orderedComponents.GetEnumerator();
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        
        #region Implementation of IReadOnlyCollection<out TService>

        public int Count
        {
            get
            {
                return _orderedComponents.Count;
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IReadOnlyList<out TService>

        public TService this[int index]
        {
            get
            {
                return _orderedComponents[index];
            }
        }

        #endregion
    }
}
