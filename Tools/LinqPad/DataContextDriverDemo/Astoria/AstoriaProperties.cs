using System.Net;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;

namespace DataContextDriverDemo.Astoria
{
	/// <summary>
	/// Wrapper to expose typed properties over ConnectionInfo.DriverData.
	/// </summary>
	class AstoriaProperties
	{
		readonly IConnectionInfo _cxInfo;
		readonly XElement _driverData;

		public AstoriaProperties (IConnectionInfo cxInfo)
		{
			_cxInfo = cxInfo;
			_driverData = cxInfo.DriverData;
		}

		public bool Persist
		{
			get { return _cxInfo.Persist; }
			set { _cxInfo.Persist = value; }
		}

		public string Uri
		{
			get { return (string)_driverData.Element ("Uri") ?? ""; }
			set { _driverData.SetElementValue ("Uri", value); }
		}

		public string Domain
		{
			get { return (string)_driverData.Element ("Domain") ?? ""; }
			set { _driverData.SetElementValue ("Domain", value); }
		}

		public string UserName
		{
			get { return (string)_driverData.Element ("UserName") ?? ""; }
			set { _driverData.SetElementValue ("UserName", value); }
		}

		public string Password
		{
			get { return _cxInfo.Decrypt ((string)_driverData.Element ("Password") ?? ""); }
			set { _driverData.SetElementValue ("Password", _cxInfo.Encrypt (value)); }
		}

		public ICredentials GetCredentials ()
		{
			if (!string.IsNullOrEmpty (Domain))
				return new NetworkCredential (UserName, Password, Domain);

			if (!string.IsNullOrEmpty (UserName))
				return new NetworkCredential (UserName, Password);

			return CredentialCache.DefaultNetworkCredentials;
		}
	}

}
