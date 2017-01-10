using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public class MethodSignature
    {
        public MethodSignature()
        {
            this.Parameters = new List<MethodParameter>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsVoid
        {
            get
            {
                return (ReturnValue == null);
            } 
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsAsync { get; set; }
        public List<MethodParameter> Parameters { get; private set; }
        public MethodParameter ReturnValue { get; set; }
    }
}
