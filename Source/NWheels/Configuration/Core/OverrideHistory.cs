using System;
using System.Collections.Generic;
using System.Xml;

namespace NWheels.Configuration.Core
{
    public class OverrideHistory : IOverrideHistory
    {
        private readonly EntryList _elementEntries = new EntryList();
        private readonly Dictionary<string, EntryList> _propertyEntriesByName = new Dictionary<string, EntryList>(StringComparer.InvariantCultureIgnoreCase);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerator<IOverrideHistoryEntry> GetEnumerator()
        {
            return _elementEntries.GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _elementEntries.GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void PushElementOverride(IXmlLineInfo lineInfo)
        {
            _elementEntries.Add(new Entry(null, ConfigurationSourceInfo.CurrentSourceInUse, lineInfo));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void PushPropertyOverride(string propertyName, string value, IXmlLineInfo lineInfo)
        {
            EntryList propertyEntries;

            if ( !_propertyEntriesByName.TryGetValue(propertyName, out propertyEntries) )
            {
                propertyEntries = new EntryList();
                _propertyEntriesByName.Add(propertyName, propertyEntries);
            }

            propertyEntries.Add(new Entry(value, ConfigurationSourceInfo.CurrentSourceInUse, lineInfo));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<IOverrideHistoryEntry> this[string propertyName]
        {
            get
            {
                EntryList propertyEntries;

                if ( _propertyEntriesByName.TryGetValue(propertyName, out propertyEntries) )
                {
                    return propertyEntries;
                }
                else
                {
                    return new IOverrideHistoryEntry[0];
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Entry : IOverrideHistoryEntry
        {
            public Entry(string value, ConfigurationSourceInfo source, IXmlLineInfo lineInfo)
            {
                this.Value = value;
                this.Source = source;

                if ( lineInfo != null && lineInfo.HasLineInfo() )
                {
                    this.LineNumber = lineInfo.LineNumber;
                    this.LinePosition = lineInfo.LinePosition;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override string ToString()
            {
                var lineInfoText = (LineNumber > 0 || LinePosition > 0 ? string.Format(" ({0},{1})", LineNumber, LinePosition) : string.Empty);

                if ( Value != null )
                {
                    return string.Format("'{0}' from {1}#{2}{3}", Value, Source.Level, Source.Name, lineInfoText);
                }
                else
                {
                    return string.Format("from {1}#{2}{3}", Value, Source.Level, Source.Name, lineInfoText);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Value { get; private set; }
            public ConfigurationSourceInfo Source { get; private set; }
            public int LineNumber { get; private set; }
            public int LinePosition { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class EntryList : List<Entry>
        {
        }
    }
}
