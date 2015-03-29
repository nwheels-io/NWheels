using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.Modules.Security.Domain
{
    public class PasswordDataType : ISemanticDataType
    {
        public System.ComponentModel.DataAnnotations.DataType GetDataTypeAnnotation()
        {
            throw new NotImplementedException();
        }

        public IPropertyValidationMetadata GetDefaultValidation()
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public Type ClrType
        {
            get { throw new NotImplementedException(); }
        }

        public object DefaultValue
        {
            get { throw new NotImplementedException(); }
        }

        public string DefaultDisplayName
        {
            get { throw new NotImplementedException(); }
        }

        public string DefaultDisplayFormat
        {
            get { throw new NotImplementedException(); }
        }

        public bool? DefaultSortAscending
        {
            get { throw new NotImplementedException(); }
        }
    }
}
