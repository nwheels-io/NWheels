using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Messages
{
    public class MessageTypeRegistration
    {
        public MessageTypeRegistration(Type bodyType)
        {
            BodyType = bodyType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IList<Type> GetBodyTypeHierarchyTopDown()
        {
            var hierarchy = new List<Type>();

            for (
                var type = this.BodyType; 
                type != null ;
                type = type.BaseType )
            {
                hierarchy.Add(type);
            }

            hierarchy.Reverse(); // from bottom-up to top-down
            return hierarchy;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type BodyType { get; private set; }
    }
}
