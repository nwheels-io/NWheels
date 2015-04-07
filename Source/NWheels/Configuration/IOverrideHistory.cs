using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace NWheels.Configuration
{
    public interface IOverrideHistory : IEnumerable<IOverrideHistoryEntry>
    {
        IEnumerable<IOverrideHistoryEntry> this[string propertyName] { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IOverrideHistoryEntry
    {
        string Value { get; }
        ConfigurationSourceInfo Source { get; }
        int LineNumber { get; }
        int LinePosition { get; }
    }
}
