using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;
using NWheels.Exceptions;
using TT = Hapil.TypeTemplate;

namespace NWheels.DataObjects.Core.Factories
{
    public class PropertyImplementationStrategyMap
    {
        private readonly ObjectFactoryContext _factoryContext;
        private readonly ITypeMetadata _metaType;
        private readonly PropertyImplementationStrategy[] _strategyPrototypes;
        private readonly HashSet<PropertyInfo> _baseContractProperties;
        private readonly Dictionary<IPropertyMetadata, PropertyImplementationStrategy> _map;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyImplementationStrategyMap(
            ObjectFactoryContext factoryContext,
            ITypeMetadata metaType, 
            params PropertyImplementationStrategy[] strategyPrototypes)
        {
            _factoryContext = factoryContext;
            _metaType = metaType;
            _strategyPrototypes = strategyPrototypes;

            _baseContractProperties = new HashSet<PropertyInfo>();
            PopulateBaseContractProperties();

            _map = new Dictionary<IPropertyMetadata, PropertyImplementationStrategy>();
            BuildMap();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void InvokeStrategies(Action<PropertyImplementationStrategy> action)
        {
            InvokeStrategies(strategy => true, action);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void InvokeStrategies(Func<PropertyImplementationStrategy, bool> predicate, Action<PropertyImplementationStrategy> action)
        {
            foreach ( var strategy in this.Strategies.Where(predicate) )
            {
                using ( TT.CreateScope<TT.TProperty>(strategy.MetaProperty.ClrType) )
                {
                    action(strategy);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<IPropertyMetadata> Properties
        {
            get
            {
                return _map.Keys;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<PropertyImplementationStrategy> Strategies
        {
            get
            {
                return _map.Values;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyImplementationStrategy this[IPropertyMetadata metaProperty]
        {
            get
            {
                return _map[metaProperty];
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void PopulateBaseContractProperties()
        {
            if (_metaType.BaseType != null)
            {
                _baseContractProperties.UnionWith(_metaType.BaseType.Properties.Select(p => p.ContractPropertyInfo));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void BuildMap()
        {
            foreach ( var metaProperty in _metaType.Properties.Where(NotImplementedByBaseEntity) )
            {
                var strategyPrototype = _strategyPrototypes.FirstOrDefault(s => s.ShouldApply(metaProperty));

                if ( strategyPrototype == null )
                {
                    throw new ContractConventionException(
                        typeof(PropertyImplementationConvention), 
                        _metaType.ContractType, 
                        metaProperty.ContractPropertyInfo, 
                        "No implementation strategy found for this property.");
                }

                var strategyClone = strategyPrototype.Clone(metaProperty);
                _map.Add(metaProperty, strategyClone);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool NotImplementedByBaseEntity(IPropertyMetadata property)
        {
            return !_baseContractProperties.Contains(property.ContractPropertyInfo);
        }
    }
}
