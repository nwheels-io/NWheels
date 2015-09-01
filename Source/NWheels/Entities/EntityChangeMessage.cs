using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities.Core;
using NWheels.Processing.Messages;

namespace NWheels.Entities
{
    public class EntityChangeMessage : MessageObjectBase
    {
        public EntityChangeMessage(IFramework framework, Type entityContract, IEnumerable<IDomainObject> entities)
            : base(framework)
        {
            EntityContract = entityContract;
            Entities = entities;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type EntityContract { get; private set; }
        public IEnumerable<IDomainObject> Entities { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static EntityChangeMessage Create<TEntity>(IFramework framework, IEnumerable<IDomainObject> entities, EntityState state)
        {
            switch ( state )
            {
                case EntityState.NewPristine:
                case EntityState.NewModified:
                    return new EntityCreatedMessage<TEntity>(framework, entities);
                case EntityState.RetrievedModified:
                    return new EntityUpdatedMessage<TEntity>(framework, entities);
                case EntityState.RetrievedDeleted:
                    return new EntityDeletedMessage<TEntity>(framework, entities);
                default:
                    return null;
            }
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public class EntityCreatedMessage : EntityChangeMessage
    {
        public EntityCreatedMessage(IFramework framework, Type entityContract, IEnumerable<IDomainObject> entities)
            : base(framework, entityContract, entities)
        {
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public class EntityCreatedMessage<TEntity> : EntityCreatedMessage
    {
        public EntityCreatedMessage(IFramework framework, IEnumerable<IDomainObject> entities)
            : base(framework, typeof(TEntity), entities)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public new IEnumerable<TEntity> Entities
        {
            get { return base.Entities.Cast<TEntity>(); }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class EntityUpdatedMessage : EntityChangeMessage
    {
        public EntityUpdatedMessage(IFramework framework, Type entityContract, IEnumerable<IDomainObject> entities)
            : base(framework, entityContract, entities)
        {
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public class EntityUpdatedMessage<TEntity> : EntityCreatedMessage
    {
        public EntityUpdatedMessage(IFramework framework, IEnumerable<IDomainObject> entities)
            : base(framework, typeof(TEntity), entities)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public new IEnumerable<TEntity> Entities
        {
            get { return base.Entities.Cast<TEntity>(); }
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public class EntityDeletedMessage : EntityChangeMessage
    {
        public EntityDeletedMessage(IFramework framework, Type entityContract, IEnumerable<IDomainObject> entities)
            : base(framework, entityContract, entities)
        {
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public class EntityDeletedMessage<TEntity> : EntityCreatedMessage
    {
        public EntityDeletedMessage(IFramework framework, IEnumerable<IDomainObject> entities)
            : base(framework, typeof(TEntity), entities)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public new IEnumerable<TEntity> Entities
        {
            get { return base.Entities.Cast<TEntity>(); }
        }
    }
}
