using System;
using System.Collections.Generic;
using NWheels.Processing.Rules.Core;

namespace NWheels.Processing.Rules
{
    public class CompiledRuleSystem<TDataContext>
    {
		public IList<IRuleDomainObject> Variables { get; private set; }
		public IList<IRuleDomainObject> Functions { get; private set; }
		public IList<IRuleDomainObject> Actions { get; private set; }

		//----------------------------------------------------------------------------------------------------------------------
	
		public CompiledRuleSystem()
	    {
			this.Variables=new List<IRuleDomainObject>();
			this.Functions = new List<IRuleDomainObject>();
			this.Actions=new List<IRuleDomainObject>();
	    }

		//----------------------------------------------------------------------------------------------------------------------
	    public void Run(TDataContext context)
        {
        }
    }
}
