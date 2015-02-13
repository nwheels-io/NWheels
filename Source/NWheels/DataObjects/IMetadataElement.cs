using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NWheels.DataObjects
{
    public interface IMetadataElement
    {
        void AcceptVisitor(IMetadataElementVisitor visitor);
        Type ElementType { get; }
    }
}
