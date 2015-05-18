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
        [DataMember(Order = 2)]
        public AwaitListSnapshot AwaitList { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract]
        public class AwaitListSnapshot
        {
            [DataMember(Order = 1)]
            public EntrySnapshot[] Entries { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static AwaitListSnapshot TakeSnapshotOf(WorkflowAwaitList<string> awaitList)
            {
                return new ActorAwaitList {
                    Entries = awaitList.GetAllEntries().Select(ActorAwaitListEntry.TakeSnapshotOf).ToArray()
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract]
        public class EntrySnapshot
        {
            [DataMember(Order = 1)]
            public string EventClrType { get; set; }

            [DataMember(Order = 2)]
            [ProtoMember(456, Options = MemberSerializationOptions.DynamicType)]
            public object EventKey { get; set; }

            [DataMember(Order = 3)]
            public AwaiterSnapshot[] Awaiters { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static EntrySnapshot TakeSnapshotOf(WorkflowAwaitList<string>.Entry entry)
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
        public class AwaiterSnapshot
        {
            [DataMember(Order = 1)]
            public string ActorName { get; set; }

            [DataMember(Order = 2)]
            public DateTime TimeoutAtUtc { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static AwaitingActor TakeSnapshotOf(WorkflowAwaitList<string>.Awaiter awaiter)
            {
                return new ActorAwaitListEntry
                {
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
