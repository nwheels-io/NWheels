using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing
{
    [DataContract(Namespace = RuleSystemDescription.DataContractNamespace)]
    public class RuleSystemData
    {
        public void MergeFirst(RuleSystemData rules)
        {
            throw new NotImplementedException();    
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void MergeLast(RuleSystemData rules)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ToJsonString()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public IList<RuleSystemDescription.RuleSet> RuleSets { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static RuleSystemData FromJsonString(string json)
        {
            throw new NotImplementedException();
        }
    }
}
