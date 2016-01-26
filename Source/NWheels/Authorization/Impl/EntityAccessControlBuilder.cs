using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.Extensions;

namespace NWheels.Authorization.Impl
{
    internal class EntityAccessControlBuilder : IEntityAccessControlBuilder
    {
        private readonly ITypeMetadataCache _metadataCache;
        private readonly Dictionary<Type, HashSet<IEntityAccessControl>> _controlsByContractType;
        private readonly HashSet<IEntityAccessControl> _wildcardControls;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityAccessControlBuilder(ITypeMetadataCache metadataCache)
        {
            _metadataCache = metadataCache;
            _controlsByContractType = new Dictionary<Type, HashSet<IEntityAccessControl>>();
            _wildcardControls = new HashSet<IEntityAccessControl>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEntityAccessControlBuilder

        public INonTypedEntityAccessControlBuilder ToAllEntities()
        {
            var control = new NonTypedEntityAccessControl();
            _wildcardControls.Add(control);
            return control;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypedEntityAccessControlBuilder<T> ToEntity<T>()
        {
            var control = new TypedEntityAccessControl<T>();
            var controlSet = _controlsByContractType.GetOrAdd(typeof(T), key => new HashSet<IEntityAccessControl>());
            controlSet.Add(control);
            return control;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INonTypedEntityAccessControlBuilder ToEntities<T1, T2>()
        {
            return ToEntities(typeof(T1), typeof(T2));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3>()
        {
            return ToEntities(typeof(T1), typeof(T2), typeof(T3));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4>()
        {
            return ToEntities(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4, T5>()
        {
            return ToEntities(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4, T5, T6>()
        {
            return ToEntities(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4, T5, T6, T7>()
        {
            return ToEntities(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4, T5, T6, T7, T8>()
        {
            return ToEntities(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
        {
            return ToEntities(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
        {
            return ToEntities(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>()
        {
            return ToEntities(
                typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), 
                typeof(T9), typeof(T10), typeof(T11));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>()
        {
            return ToEntities(
                typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), 
                typeof(T9), typeof(T10), typeof(T11), typeof(T12));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>()
        {
            return ToEntities(
                typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8),
                typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>()
        {
            return ToEntities(
                typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8),
                typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>()
        {
            return ToEntities(
                typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8),
                typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>()
        {
            return ToEntities(
                typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8),
                typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INonTypedEntityAccessControlBuilder ToEntities(params Type[] entityContractTypes)
        {
            var control = new NonTypedEntityAccessControl();

            foreach ( var contractType in entityContractTypes )
            {
                var controlSet = GetOrAddControlSet(contractType);
                controlSet.Add(control);
            }

            return control;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddRule(IEntityAccessRule rule)
        {
            rule.BuildAccessControl(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FillEntityAccessControlDictionary(Dictionary<Type, IEntityAccessControl> destination, out IEntityAccessControl defaultControl)
        {
            ExpandRegisteredControlSets();

            foreach ( var contractControlsPair in _controlsByContractType )
            {
                destination[contractControlsPair.Key] = GetSingleControl(contractControlsPair.Value);
            }

            if ( _wildcardControls.Count > 0 )
            {
                defaultControl = GetSingleControl(_wildcardControls);
            }
            else
            {
                defaultControl = null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IEntityAccessControl GetSingleControl(HashSet<IEntityAccessControl> controlSet)
        {
            if ( controlSet.Count == 1 )
            {
                return controlSet.Single();
            }
            else 
            {
                var pipe = new EntityAccessControlPipe(controlSet);
                return pipe;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ExpandRegisteredControlSets()
        {
            var ancestorContracts = _controlsByContractType.Keys.ToArray();

            foreach ( var ancestor in ancestorContracts )
            {
                var ancestorMetaType = _metadataCache.GetTypeMetadata(ancestor);
                var ancestorControlSet = GetOrAddControlSet(ancestor);
                
                ancestorControlSet.UnionWith(_wildcardControls);

                foreach ( var inheritorMetaType in ancestorMetaType.DerivedTypes )
                {
                    var inheritorControlSet = GetOrAddControlSet(inheritorMetaType.ContractType);
                    inheritorControlSet.UnionWith(ancestorControlSet);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private HashSet<IEntityAccessControl> GetOrAddControlSet(Type contractType)
        {
            return _controlsByContractType.GetOrAdd(contractType, key => new HashSet<IEntityAccessControl>());
        }
    }
}
