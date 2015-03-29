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
        #region ISemanticDataType Members

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

        public int? MinStringLength
        {
            get { throw new NotImplementedException(); }
        }

        public int? MaxStringLength
        {
            get { throw new NotImplementedException(); }
        }

        public decimal? MinNumericValue
        {
            get { throw new NotImplementedException(); }
        }

        public decimal? MaxNumericValue
        {
            get { throw new NotImplementedException(); }
        }

        public string RegularExpression
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

        public SemanticType SemanticType
        {
            get { throw new NotImplementedException(); }
        }

        public System.ComponentModel.DataAnnotations.DataType DataTypeAnnotation
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
