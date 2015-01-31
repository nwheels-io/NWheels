using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Samples.RestService
{
    public partial class Order
    {
        private DateTimeUtc _utc;// = new DateTime();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [NotMapped]
        public DateTimeOffset DateTimeUtc
        {
            get { return _utc; }
            set { _utc = value; }
        }
    }
}
