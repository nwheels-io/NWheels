using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.Metadata;
using NWheels.Core;
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
        where TService : class
    {
        private IReadOnlyList<TService> _orderedComponents;
        private PipelineObjectFactory _pipelineFactory;
        private TService _pipelineAsService = null;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        internal Pipeline(IEnumerable<TService> orderedComponents)
        {
            _orderedComponents = orderedComponents.ToArray();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        internal Pipeline(IEnumerable<TService> orderedComponents, PipelineObjectFactory pipelineFactory)
        {
            _orderedComponents = orderedComponents.ToArray();
            _pipelineFactory = pipelineFactory;
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

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Rebuild(IComponentContext components)
        {
            _orderedComponents = components.Resolve<IEnumerable<TService>>().ToArray();
            _pipelineAsService = null;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public TService AsService()
        {
            if ( _pipelineAsService == null )
            {
                if ( _orderedComponents.Count == 1 )
                {
                    _pipelineAsService = _orderedComponents[0];
                }
                else
                {
                    if ( _pipelineFactory == null )
                    {
                        throw new InvalidOperationException("This pipeline object was not initialized with pipeline object factory.");
                    }

                    _pipelineAsService = _pipelineFactory.GetPipelineAsServiceObject(this);
                }
            }

            return _pipelineAsService;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static implicit operator Pipeline<TService>(TService[] orderedComponents)
        {
            return new Pipeline<TService>(orderedComponents);
        }
    }
}
