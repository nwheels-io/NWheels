using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Core
{
    public class WorkflowAwaitList<TAwaitId>
    {
        private readonly Dictionary<EntryKey, Entry> _entries = new Dictionary<EntryKey, Entry>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

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
