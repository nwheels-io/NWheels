using System;
using System.Collections.Generic;
using NWheels.Entities;
using NWheels.Entities.Core;

namespace NWheels.Processing.Commands
{
    public class EntityChangeSetCommandMessage : AbstractCommandMessage
    {
        public EntityChangeSetCommandMessage(IReadOnlyList<ChangeItem> changes)
        {
            this.Changes = changes;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyList<ChangeItem> Changes { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ChangeItem
        {
            public Type EntityContract { get; private set; }
            public IEntityId EntityId { get; private set; }
            public IEntityId TemporaryId { get; private set; }
            public EntityState EntityState { get; private set; }
            public IReadOnlyList<PropertySetter> PropertySetters { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class PropertySetter
        {
            public abstract void ApplyTo(IDomainObject domainObject);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class PropertySetter<T> : PropertySetter
        {
            public PropertySetter(string[] path, T newValue)
            {
                Path = path;
                NewValue = newValue;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void ApplyTo(IDomainObject domainObject)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string[] Path { get; private set; }
            public T NewValue { get; private set; }
        }
    }
}
