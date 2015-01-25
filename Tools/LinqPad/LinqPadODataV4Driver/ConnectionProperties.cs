using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;

namespace LinqPadODataV4Driver
{
    public class ConnectionProperties
    {
		readonly IConnectionInfo _connectionInfo;
		readonly XElement _driverData;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ConnectionProperties(IConnectionInfo connectionInfo)
		{
			_connectionInfo = connectionInfo;
			_driverData = connectionInfo.DriverData;
		}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

		public bool Persist
		{
			get { return _connectionInfo.Persist; }
			set { _connectionInfo.Persist = value; }
		}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

		public string Uri
		{
			get { return (string)_driverData.Element ("Uri") ?? ""; }
			set { _driverData.SetElementValue ("Uri", value); }
		}
    }
}
