using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging
{
    [DataContract(Namespace = "NWheels.Logging", Name = "Activity")]
    public class FormattedActivityLogNode : ActivityLogNode
    {
        private readonly string _singleLineText;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FormattedActivityLogNode(string singleLineText)
        {
            _singleLineText = singleLineText;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            return _singleLineText;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            if ( base.Exception != null )
            {
                return base.Exception.ToString();
            }
            else
            {
                return null;
            }
        }
    }
}
