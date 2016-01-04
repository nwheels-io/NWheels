using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.UI.Core;
using TT = Hapil.TypeTemplate;

namespace NWheels.UI.Factories
{
    public interface IQueryResultAggregatorObjectFactory
    {
        IQueryResultAggregator GetQueryResultAggregator(ApplicationEntityService.QueryContext queryContext);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class QueryResultAggregatorObjectFactory : ConventionObjectFactory, IQueryResultAggregatorObjectFactory
    {
        private readonly ITypeMetadataCache _metadataCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public QueryResultAggregatorObjectFactory(DynamicModule module, ITypeMetadataCache metadataCache)
            : base(module)
        {
            _metadataCache = metadataCache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IQueryResultAggregator GetQueryResultAggregator(ApplicationEntityService.QueryContext queryContext)
        {
            var typeKey = new QueryTypeKey(queryContext);
            var typeEntry = GetOrBuildType(typeKey);
            return typeEntry.CreateInstance<IQueryResultAggregator>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override TypeKey CreateTypeKey(Type contractType, params Type[] secondaryInterfaceTypes)
        {
            var queryContext = ApplicationEntityService.QueryContext.Current;

            if ( queryContext == null )
            {
                throw new InvalidOperationException("Not runnng as ApplicationEntityService query.");
            }

            return new QueryTypeKey(queryContext);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            return new IObjectFactoryConvention[] {
                new AggregatorObjectConvention(_metadataCache, (QueryTypeKey)context.TypeKey)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class QueryTypeKey : TypeKey
        {
            private readonly string _cacheKey;
            private readonly string _entityName;
            private readonly ApplicationEntityService.QuerySelectItem[] _aggregations;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public QueryTypeKey(ApplicationEntityService.QueryContext queryContext)
                : base(baseType: typeof(object), primaryInterface: typeof(IQueryResultAggregator))
            {
                _cacheKey = queryContext.Options.BuildCacheKey();
                _entityName = queryContext.Options.EntityName;
                _aggregations = queryContext
                    .Options.SelectPropertyNames.Where(s => s.IsAggregation)
                    .Concat(queryContext.Options.IncludePropertyNames.Where(s => s.IsAggregation))
                    .ToArray();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of TypeKey

            public override int GetHashCode()
            {
                return base.GetHashCode() ^ _cacheKey.GetHashCode();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool Equals(TypeKey other)
            {
                if ( !base.Equals(other) )
                {
                    return false;
                }

                var otherQueryTypeKey = (other as QueryTypeKey);

                if ( otherQueryTypeKey != null )
                {
                    return (this._cacheKey == otherQueryTypeKey._cacheKey);
                }

                return false;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public string EntityName
            {
                get { return _entityName; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IReadOnlyList<ApplicationEntityService.QuerySelectItem> Aggregations
            {
                get { return _aggregations; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AggregatorObjectConvention : ImplementationConvention
        {
            private readonly ITypeMetadataCache _metadataCache;
            private readonly QueryTypeKey _typeKey;
            private readonly ITypeMetadata _metaType;
            private readonly IReadOnlyDictionary<string, IReadOnlyList<IPropertyMetadata>> _metaPropertyPathByAliasName;
            private readonly Dictionary<string, Field<TT.TProperty>> _aggregatedFieldByAliasName;
            private Field<bool> _isFirstRecordField;
            private Field<bool> _isAggregationFinishedField;
            private Field<int> _recordCountField;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AggregatorObjectConvention(ITypeMetadataCache metadataCache, QueryTypeKey typeKey)
                : base(Will.ImplementBaseClass)
            {
                _metadataCache = metadataCache;
                _typeKey = typeKey;
                _metaType = _metadataCache.GetTypeMetadata(typeKey.EntityName);
                _metaPropertyPathByAliasName = typeKey.Aggregations.ToDictionary<ApplicationEntityService.QuerySelectItem, string, IReadOnlyList<IPropertyMetadata>>(
                    aggr => aggr.AliasName, 
                    aggr => aggr.BuildMetaPropertyPath(_metaType));
                _aggregatedFieldByAliasName = new Dictionary<string, Field<TypeTemplate.TProperty>>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                _isFirstRecordField = writer.Field<bool>("m_IsFirstRecord");
                _isAggregationFinishedField = writer.Field<bool>("m_IsAggregationFinished");
                _recordCountField = writer.Field<int>("m_RecordCount");

                writer.DefaultConstructor();

                var implementation = writer.ImplementInterface<IQueryResultAggregator>();

                ImplementAggregationFields(writer);
                ImplementAggregateMethod(implementation);
                ImplementGetAggregatedValueMethod(implementation);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementAggregationFields(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                foreach ( var aggregation in _typeKey.Aggregations )
                {
                    var aliasName = aggregation.AliasName;
                    var metaProperty = _metaPropertyPathByAliasName[aliasName].Last();

                    using ( TT.CreateScope<TT.TProperty>(metaProperty.ClrType) )
                    {
                        var propertyName = string.Join(".", aggregation.PropertyPath);
                        var backingField = writer.Field<TT.TProperty>("m_" + propertyName);

                        _aggregatedFieldByAliasName[aliasName] = backingField;
                        writer.NewVirtualWritableProperty<TT.TProperty>(propertyName).ImplementAutomatic(backingField);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementGetAggregatedValueMethod(ImplementationClassWriter<IQueryResultAggregator> implementation)
            {
                implementation.Method<string, object>(x => x.GetAggregatedValue).Implement(
                    (w, name) => {
                        w.If(!_isAggregationFinishedField).Then(() => {
                            foreach ( var aggregation in _typeKey.Aggregations.Where(aggr => aggr.AggregationType == ApplicationEntityService.AggregationType.Avg) )
                            {
                                var aliasName = aggregation.AliasName;
                                var metaProperty = _metaPropertyPathByAliasName[aliasName].Last();

                                using ( TT.CreateScope<TT.TProperty>(metaProperty.ClrType) )
                                {
                                    _aggregatedFieldByAliasName[aliasName].Assign(_aggregatedFieldByAliasName[aliasName] / _recordCountField.CastTo<TT.TProperty>());
                                }
                            }

                            _isAggregationFinishedField.Assign(true);
                        });

                        var switchStatement = w.Switch(name);

                        foreach ( var aggregation in _typeKey.Aggregations )
                        {
                            var aliasName = aggregation.AliasName;
                            var metaProperty = _metaPropertyPathByAliasName[aliasName].Last();

                            using ( TT.CreateScope<TT.TProperty>(metaProperty.ClrType) )
                            {
                                switchStatement.Case(aliasName).Do(() => w.Return(_aggregatedFieldByAliasName[aliasName]));
                            }
                        }

                        switchStatement.Default(() => w.Throw<ArgumentException>("Invalid property name."));
                    });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementAggregateMethod(ImplementationClassWriter<IQueryResultAggregator> implementation)
            {
                implementation.Method<object>(x => x.Aggregate).Implement((w, record) => {
                    w.If(_isAggregationFinishedField).Then(() => {
                        w.Throw<InvalidOperationException>("Cannot aggregate more reocrds once aggregation results were retrieved.");
                    });

                    w.If(_isFirstRecordField).Then(() =>
                    {
                        foreach ( var aggregation in _typeKey.Aggregations )
                        {
                            var aliasName = aggregation.AliasName;
                            var metaProperty = _metaPropertyPathByAliasName[aliasName].Last();
                            
                            using ( TT.CreateScope<TT.TProperty>(metaProperty.ClrType) )
                            {
                                var valueLocal = w.Local<TT.TProperty>();
                                valueLocal.Assign(BuildPropertyReadExpression(record, aggregation));
                                _aggregatedFieldByAliasName[aliasName].Assign(valueLocal);
                            }
                        }
    
                        _isFirstRecordField.Assign(false);
                    })
                    .Else(() =>
                    {
                        foreach ( var aggregation in _typeKey.Aggregations )
                        {
                            var aggregationCopy = aggregation;
                            var aliasName = aggregation.AliasName;
                            var metaProperty = _metaPropertyPathByAliasName[aliasName].Last();

                            using ( TT.CreateScope<TT.TProperty>(metaProperty.ClrType) )
                            {
                                var valueLocal = w.Local<TT.TProperty>();
                                valueLocal.Assign(BuildPropertyReadExpression(record, aggregation));

                                ImplementAggregation(aggregationCopy, w, _aggregatedFieldByAliasName[aliasName], valueLocal);
                            }
                        }
                    });
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private MutableOperand<TT.TProperty> BuildPropertyReadExpression(Operand<object> record, ApplicationEntityService.QuerySelectItem aggregation)
            {
                var propertyPath = _metaPropertyPathByAliasName[aggregation.AliasName];
                MutableOperand<TT.TProperty> expression;

                using ( TT.CreateScope<TT.TProperty>(propertyPath[0].ClrType) )
                {
                    expression = record.Prop<TT.TProperty>(propertyPath[0].ContractPropertyInfo);
                }

                IOperand stepTarget = expression;

                for ( int i = 1 ; i < propertyPath.Count ; i++ )
                {
                    using ( TT.CreateScope<TT.TContract, TT.TProperty>(propertyPath[i].DeclaringContract.ContractType, propertyPath[i].ClrType) )
                    {
                        expression = stepTarget.CastTo<TT.TContract>().Prop<TT.TProperty>(propertyPath[i].ContractPropertyInfo);
                        stepTarget = expression;
                    }
                }

                return expression;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void ImplementAggregation(
                ApplicationEntityService.QuerySelectItem aggregation, 
                MethodWriterBase writer,
                Field<TT.TProperty> aggregatedField, 
                Local<TT.TProperty> valueLocal)
            {
                switch ( aggregation.AggregationType )
                {
                    case ApplicationEntityService.AggregationType.Sum:
                    case ApplicationEntityService.AggregationType.Avg:
                        aggregatedField.Assign(aggregatedField + valueLocal);
                        break;
                    case ApplicationEntityService.AggregationType.Min:
                        writer.If(valueLocal < aggregatedField).Then(() => {
                            aggregatedField.Assign(valueLocal);
                        });
                        break;
                    case ApplicationEntityService.AggregationType.Max:
                        writer.If(valueLocal > aggregatedField).Then(() => {
                            aggregatedField.Assign(valueLocal);
                        });
                        break;
                    default:
                        throw new ArgumentException("Aggregation type not supported: " + aggregation.AggregationType);
                }
            }
        }
    }
}
