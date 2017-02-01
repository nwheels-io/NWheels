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

        public MethodSignature(IEnumerable<MethodParameter> parameters, MethodParameter returnValue, bool isAsync)
            : this()
        {
            this.Parameters.AddRange(parameters);
            this.ReturnValue = returnValue;
            this.IsAsync = isAsync;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodSignature(MethodInfo binding)
            : this()
        {
            if (binding.ReturnType != null && binding.ReturnType != typeof(void))
            {
                this.ReturnValue = new MethodParameter() {
                    Type = binding.ReturnType
                };
            }

            var bindingParameters = binding.GetParameters();

            for (int i = 0 ; i < bindingParameters.Length ; i++)
            {
                var parameter = new MethodParameter(
                    name: bindingParameters[i].Name, 
                    position: i + 1, 
                    type: bindingParameters[i].ParameterType, 
                    modifier: GetParameterModifier(bindingParameters[i]));

                this.Parameters.Add(parameter);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodSignature(ConstructorInfo binding)
            : this()
        {
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static MethodParameterModifier GetParameterModifier(ParameterInfo binding)
        {
            if (binding.ParameterType.IsByRef)
            {
                return (binding.IsOut ? MethodParameterModifier.Out : MethodParameterModifier.Ref);
            }

            return MethodParameterModifier.None;
        }
    }
}
