using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Messages
{
    public interface IContentTemplateProvider
    {
        string GetTemplate(object contentType);
        string Format(string template, object data);
    }
}
