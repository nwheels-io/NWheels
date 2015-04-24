using NWheels.Processing.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;
using ProtoBuf;

namespace NWheels.Processing.Impl
{
    [DataContract]
    public class WorkflowProcessorSnapshot
    {
        [DataMember(Order = 1)]
        public Actor[] Actors { get; set; }
        
        [DataMember(Order = 2)]
        public ActorAwaitList AwaitList { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract]
        public class Actor
        {
            [DataMember(Order = 1)]
            public string Name { get; set; }

            [DataMember(Order = 2)]
            public string WorkItemClrType { get; set; }
            
            [DataMember(Order = 3)]
            [ProtoMember(123, Options = MemberSerializationOptions.DynamicType)]
            public object[] WorkItems { get; set; }
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract]
        public class ActorAwaitList
        {
            [DataMember(Order = 1)]
            public ActorAwaitListEntry[] Entries { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static ActorAwaitList TakeSnapshotOf(WorkflowAwaitList<string> awaitList)
            {
                return new ActorAwaitList {
                    Entries = awaitList.GetAllEntries().Select(ActorAwaitListEntry.TakeSnapshotOf).ToArray()
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract]
        public class ActorAwaitListEntry
        {
            [DataMember(Order = 1)]
            public string EventClrType { get; set; }

            [DataMember(Order = 2)]
            [ProtoMember(456, Options = MemberSerializationOptions.DynamicType)]
            public object EventKey { get; set; }

            [DataMember(Order = 3)]
            public string[] ActorNames { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static ActorAwaitListEntry TakeSnapshotOf(WorkflowAwaitList<string>.Entry entry)
            {
                return new ActorAwaitListEntry {
                    EventClrType = entry.EntryKey.EventType.AssemblyQualifiedNameNonVersioned(),
                    EventKey = entry.EntryKey.EventKey,
                    ActorNames = entry.ToArray()
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract]
        public abstract class Primitive
        {
            public abstract object GetUnderlyingValue();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract]
        public class Primitive<T> : Primitive where T : struct
        {
            public Primitive()
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Primitive(T value)
            {
                this.Value = value;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override object GetUnderlyingValue()
            {
                return Value;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember(Order = 1)]
            public T Value { get; set; }
        }
    }
}
