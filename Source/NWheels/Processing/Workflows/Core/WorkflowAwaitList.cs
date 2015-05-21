using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.Extensions;

namespace NWheels.Processing.Workflows.Core
{
    /// <summary>
    /// A data structure which tracks subscribers awaiting for events.
    /// </summary>
    /// <typeparam name="TAwaitId">
    /// The id type of the awaiting subscribers tracked by this data structure.
    /// This type parameter allows reuse of WorkflowAwaitList by both WorkflowEngine and WorkflowProcessor.
    /// WorkflowEngine tracks workflow instances identified by a Guid, while WorkflowProcessor tracks actors identified by a string name.
    /// </typeparam>
    /// <remarks>
    /// Thread safety: this data structure is not safe for concurrent access.
    /// </remarks>
    public class WorkflowAwaitList<TAwaitId>
    {
        /// <summary>
        /// EntryKey is a (type,key) pair, where 'type' is the CLR type of the event class, and 'key' is a key object encapsulated by event object.
        /// Entry encapsulates a linked list of subscribers to events identified by the EntryKey.
        /// </summary>
        private readonly Dictionary<EntryKey, Entry> _entries = new Dictionary<EntryKey, Entry>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Merges an exsting entry into this await list.
        /// </summary>
        /// <remarks>
        /// This is used to reconstruct "the big" await list of the WorkfowEngine based on "small" await lists of individual workflow instances.
        /// </remarks>
        public void Merge(Entry externalEntry)
        {
            var internalEntry = _entries.GetOrAdd(externalEntry.EntryKey, key => new Entry(key));
            internalEntry.Merge(externalEntry);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Push(Type eventType, object eventKey, TAwaitId awaitId, DateTime timeoutAtUtc)
        {
             Push(new EntryKey(eventType, eventKey), awaitId, timeoutAtUtc);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Push(EntryKey entryKey, TAwaitId awaitId, DateTime timeoutAtUtc)
        {
            Entry entry;

            if ( !_entries.TryGetValue(entryKey, out entry) )
            {
                entry = new Entry(entryKey);
                _entries.Add(entryKey, entry);
            }

            entry.Push(awaitId, timeoutAtUtc);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAwaitId[] Take(IWorkflowEvent @event)
        {
            return Take(new EntryKey(@event.GetEventType(), @event.GetEventKey()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAwaitId[] Take(EntryKey entryKey)
        {
            Entry entry;

            if ( _entries.TryGetValue(entryKey, out entry) )
            {
                _entries.Remove(entryKey);
                return entry.Select(e => e.Id).ToArray();
            }
            else
            {
                return new TAwaitId[0];
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TimedOutAwaiter[] TakeTimedOut(DateTime utcNow)
        {
            var timedOut = new List<TimedOutAwaiter>();
            var entryArray = _entries.Values.ToArray();

            foreach ( var entry in entryArray )
            {
                entry.TakeTimedOut(utcNow, timedOut);

                if ( entry.Count == 0 )
                {
                    _entries.Remove(entry.EntryKey);
                }
            }

            return timedOut.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Entry[] GetAllEntries()
        {
            return _entries.Values.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsEmpty
        {
            get
            {
                return (_entries.Count == 0);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// EntryKey is a (type,key) pair, where 'type' is the CLR type of the event class, and 'key' is a key object encapsulated by event object.
        /// </summary>
        /// <remarks>
        /// The EntryKey supports comparison for equality through implementation of Object.Equals and IEquatable[EntryKey].
        /// </remarks>
        public struct EntryKey : IEquatable<EntryKey>
        {
            public readonly Type EventType;
            public readonly object EventKey;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntryKey(Type eventType, object eventKey)
            {
                this.EventType = eventType;
                this.EventKey = eventKey;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool Equals(object obj)
            {
                if ( obj is EntryKey )
                {
                    return Equals((EntryKey)obj);
                }
                else
                {
                    return false;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool Equals(EntryKey other)
            {
                if ( this.EventType != other.EventType )
                {
                    return false;
                }

                if ( this.EventKey == null && other.EventKey == null )
                {
                    return true;
                }

                if ( this.EventKey != null && other.EventKey != null )
                {
                    return this.EventKey.Equals(other.EventKey);
                }

                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override int GetHashCode()
            {
                return EventType.GetHashCode() ^ (EventKey != null ? EventKey.GetHashCode() : 0);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override string ToString()
            {
                return EventType.FullName + (EventKey != null ? "[" + EventKey.ToString() + "]" : "");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Encapsulates a linked list of subscribers to events identified by an EntryKey.
        /// </summary>
        public class Entry : IEnumerable<Awaiter>
        {
            private readonly EntryKey _entryKey;
            private readonly LinkedList<Awaiter> _awaitIds;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Entry(EntryKey entryKey)
            {
                _entryKey = entryKey;
                _awaitIds = new LinkedList<Awaiter>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerator<Awaiter> GetEnumerator()
            {
                return _awaitIds.GetEnumerator();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _awaitIds.GetEnumerator();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Push(TAwaitId awaitId, DateTime timeoutAtUtc)
            {
                _awaitIds.AddLast(new Awaiter(awaitId, timeoutAtUtc));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Merge(Entry externalEntry)
            {
                foreach ( var externalAwaiter in externalEntry._awaitIds )
                {
                    _awaitIds.AddLast(externalAwaiter);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void TakeTimedOut(DateTime utcNow, List<TimedOutAwaiter> timedOut)
            {
                var node = _awaitIds.First;

                while ( node != null )
                {
                    if ( utcNow >= node.Value.TimeoutAtUtc )
                    {
                        timedOut.Add(new TimedOutAwaiter(node.Value.Id, this.EntryKey));
                        _awaitIds.Remove(node);
                    }
                    else
                    {
                        node = node.Next;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public EntryKey EntryKey
            {
                get { return _entryKey; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int Count
            {
                get { return _awaitIds.Count; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TimedOutAwaiter
        {
            public TimedOutAwaiter(TAwaitId id, EntryKey key)
            {
                this.Id = id;
                this.EntryKey = key;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TimedOutWorkflowEvent CreateTimedOutEvent()
            {
                return new TimedOutWorkflowEvent(this.EntryKey.EventType, this.EntryKey.EventKey);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TAwaitId Id { get; private set; }
            public EntryKey EntryKey { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public struct Awaiter
        {
            public Awaiter(TAwaitId id, DateTime timeoutAtUtc)
            {
                this.Id = id;
                this.TimeoutAtUtc = timeoutAtUtc;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public readonly TAwaitId Id;
            public readonly DateTime TimeoutAtUtc;
        }
    }
}
