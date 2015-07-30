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
        private readonly List<StrategyRule> _strategyRules;
        private readonly Dictionary<IPropertyMetadata, PropertyImplementationStrategy> _map;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private PropertyImplementationStrategyMap()
        {
            _strategyRules = new List<StrategyRule>();
            _map = new Dictionary<IPropertyMetadata, PropertyImplementationStrategy>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void InvokeStrategies(Action<PropertyImplementationStrategy> action)
        {
            InvokeStrategies(strategy => true, action);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void InvokeStrategies(Func<PropertyImplementationStrategy, bool> predicate, Action<PropertyImplementationStrategy> action)
        {
            InvokeStrategies(this.Strategies.Where(predicate), action);
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

        private void AddStrategyRule(
            Func<ITypeMetadataCache, ITypeMetadata, IPropertyMetadata, bool> condition,
            Func<IPropertyMetadata, PropertyImplementationStrategy> strategyFactory)
        {
            _strategyRules.Add(new StrategyRule(condition, strategyFactory));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private HashSet<PropertyInfo> GetBaseContractProperties(ITypeMetadata metaType)
        {
            var baseProperties = new HashSet<PropertyInfo>();

            if  ( metaType.BaseType != null )
            {
                baseProperties.UnionWith(metaType.BaseType.Properties.Select(p => p.ContractPropertyInfo));
            }

            return baseProperties;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void BuildMap(ITypeMetadataCache metadataCache, ITypeMetadata metaType)
        {
            var baseProperties = GetBaseContractProperties(metaType);
            Func<IPropertyMetadata, bool> notImplementedByBaseEntity = p => !baseProperties.Contains(p.ContractPropertyInfo);

            foreach ( var metaProperty in metaType.Properties.Where(notImplementedByBaseEntity) )
            {
                var effectiveRule = _strategyRules.FirstOrDefault(rule => rule.Condition(metadataCache, metaType, metaProperty));

                if ( effectiveRule == null )
                {
                    throw new ContractConventionException(
                        typeof(PropertyImplementationConvention), 
                        metaType.ContractType, 
                        metaProperty.ContractPropertyInfo, 
                        "No implementation strategy found for this property.");
                }

                var strategyInstance = effectiveRule.StrategyFactory(metaProperty);
                _map.Add(metaProperty, strategyInstance);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void InvokeStrategies(IEnumerable<PropertyImplementationStrategy> strategies, Action<PropertyImplementationStrategy> action)
        {
            foreach ( var strategy in strategies )
            {
                using ( TT.CreateScope<TT.TProperty>(strategy.MetaProperty.ClrType) )
                {
                    action(strategy);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Builder
        {
            private PropertyImplementationStrategyMap _mapBeingBuilt;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Builder()
            {
                _mapBeingBuilt = new PropertyImplementationStrategyMap();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AddRule(
                Func<IPropertyMetadata, bool> condition, 
                Func<IPropertyMetadata, PropertyImplementationStrategy> strategyFactory)
            {
                AddRule((cache, type, property) => condition(property), strategyFactory);
            }
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AddRule(
                Func<ITypeMetadata, IPropertyMetadata, bool> condition, 
                Func<IPropertyMetadata, PropertyImplementationStrategy> strategyFactory)
            {
                AddRule((cache, type, property) => condition(type, property), strategyFactory);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AddRule(
                Func<ITypeMetadataCache, ITypeMetadata, IPropertyMetadata, bool> condition, 
                Func<IPropertyMetadata, PropertyImplementationStrategy> strategyFactory)
            {
                if ( _mapBeingBuilt == null )
                {
                    throw new InvalidOperationException("Cannot add rules after the map has been built.");
                }

                _mapBeingBuilt.AddStrategyRule(condition, strategyFactory);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PropertyImplementationStrategyMap Build(ITypeMetadataCache metadataCache, ITypeMetadata metaType)
            {
                _mapBeingBuilt.BuildMap(metadataCache, metaType);
                
                var result = _mapBeingBuilt;
                _mapBeingBuilt = null;
                return result;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class StrategyRule
        {
            public StrategyRule(Func<ITypeMetadataCache, ITypeMetadata, IPropertyMetadata, bool> condition, Func<IPropertyMetadata, PropertyImplementationStrategy> strategyFactory)
            {
                this.Condition = condition;
                this.StrategyFactory = strategyFactory;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Func<ITypeMetadataCache, ITypeMetadata, IPropertyMetadata, bool> Condition { get; private set; }
            public Func<IPropertyMetadata, PropertyImplementationStrategy> StrategyFactory { get; private set; }
        }
    }
}
