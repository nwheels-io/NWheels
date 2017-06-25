using NWheels.Frameworks.Uidl.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Frameworks.Uidl.Generic
{
    public class FormFieldElement<TValue> : AbstractUIElement<TValue>, IFormFieldElement
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IFormFieldElement
    {
    }
}
