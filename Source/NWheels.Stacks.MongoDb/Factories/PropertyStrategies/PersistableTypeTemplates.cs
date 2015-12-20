using System;
using Hapil;
using NWheels.Entities;
using NWheels.Entities.Core;

namespace NWheels.Stacks.MongoDb.Factories.PropertyStrategies
{
    public static class PersistableTypeTemplates
    {
        public class TPersistable : TypeTemplate.ITemplateType<TPersistable>, TypeTemplate.TContract, IPersistableObject
        {
            #region Implementation of IObject

            public Type ContractType
            {
                get { throw new NotImplementedException(); }
            }

            public Type FactoryType
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsModified
            {
                get { throw new NotImplementedException(); }
            }

            #endregion

            #region Implementation of IPersistableObject

            public object[] ExportValues(IEntityRepository entityRepo)
            {
                throw new NotImplementedException();
            }

            public void ImportValues(IEntityRepository entityRepo, object[] values)
            {
                throw new NotImplementedException();
            }

            public object EntityId
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }
    }
}
