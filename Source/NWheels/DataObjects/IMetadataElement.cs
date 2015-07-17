using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NWheels.DataObjects.Core;

namespace NWheels.DataObjects
{
    public interface IMetadataElement
    {
        void AcceptVisitor(ITypeMetadataVisitor visitor);
        Type ElementType { get; }
        string ReferenceName { get; }
    }
}
