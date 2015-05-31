using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NWheels.Extensions;
using NWheels.Processing.Workflows.Core;
using ProtoBuf;

namespace NWheels.Processing.Workflows.Impl
{
    [DataContract]
    public class WorkflowProcessorSnapshot
    {
        [DataMember(Order = 1)]
        public AwaitListSnapshot AwaitList { get; set; }

        [DataMember(Order = 2)]
        public IList<ActorCookie> Cookies { get; set; }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract]
        public class AwaitListSnapshot
        {
            [DataMember(Order = 1)]
            public EntrySnapshot[] Entries { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static AwaitListSnapshot TakeSnapshotOf(WorkflowAwaitList<string> awaitList)
            {
                return new AwaitListSnapshot {
                    Entries = awaitList.GetAllEntries().Select(EntrySnapshot.TakeSnapshotOf).ToArray()
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
                return new EntrySnapshot() {
                    EventClrType = entry.EntryKey.EventType.AssemblyQualifiedNameNonVersioned(),
                    EventKey = entry.EntryKey.EventKey,
                    Awaiters = entry.Select(AwaiterSnapshot.TakeSnapshotOf).ToArray()
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

            public static AwaiterSnapshot TakeSnapshotOf(WorkflowAwaitList<string>.Awaiter awaiter)
            {
                return new AwaiterSnapshot {
                    ActorName = awaiter.Id,
                    TimeoutAtUtc = awaiter.TimeoutAtUtc
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract]
        public class ActorCookie
        {
            [DataMember]
            public string ActorName { get; set; }
            [DataMember]
            public object Cookie { get; set; }
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
