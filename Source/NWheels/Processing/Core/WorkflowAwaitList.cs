using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Core
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
        /// Subscribes an awaiter to one or more events.
        /// </summary>
        /// <param name="entryKeys">
        /// EntryKey objects that identify events to subscribe the awaiter to.
        /// </param>
        /// <param name="awaitId">
        /// The id of the awaiter being subscribed.
        /// </param>
        public void Include(IEnumerable<EntryKey> entryKeys, TAwaitId awaitId)
        {
            foreach ( var entryKey in entryKeys )
            {
                Push(entryKey, awaitId);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Push(Type eventType, object eventKey, TAwaitId awaitId)
        {
            Push(new EntryKey(eventType, eventKey), awaitId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Push(EntryKey entryKey, TAwaitId awaitId)
        {
            Entry entry;

            if ( !_entries.TryGetValue(entryKey, out entry) )
            {
                entry = new Entry(entryKey);
                _entries.Add(entryKey, entry);
            }

            entry.Push(awaitId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAwaitId[] Take(Type eventType, object eventKey)
        {
            return Take(new EntryKey(eventType, eventKey));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAwaitId[] Take(EntryKey entryKey)
        {
            Entry entry;

            if ( _entries.TryGetValue(entryKey, out entry) )
            {
                _entries.Remove(entryKey);
                return entry.ToArray();
            }
            else
            {
                return new TAwaitId[0];
            }
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
        public class Entry : IEnumerable<TAwaitId>
        {
            private readonly EntryKey _entryKey;
            private readonly LinkedList<TAwaitId> _awaitIds;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Entry(EntryKey entryKey)
            {
                _entryKey = entryKey;
                _awaitIds = new LinkedList<TAwaitId>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerator<TAwaitId> GetEnumerator()
            {
                return _awaitIds.GetEnumerator();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _awaitIds.GetEnumerator();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Push(TAwaitId awaitId)
            {
                _awaitIds.AddLast(awaitId);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public EntryKey EntryKey
            {
                get { return _entryKey; }
            }
        }
    }
}
