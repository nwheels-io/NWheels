using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Rules.Core
{
	public class RuleFunction<TReturn>:IRuleFunction<TReturn>
	{
		#region IRuleFunction<TReturn> Members

		public TReturn GetValue()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IRuleDomainObject Members

		public string IdName
		{
			get { throw new NotImplementedException(); }
		}

		public string Description
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}
