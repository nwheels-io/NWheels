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

        public MethodSignature(MethodInfo clrBinding)
            : this()
        {
            this.ClrBinding = clrBinding;

            BindClrReturnType(clrBinding);
            BindClrParameters();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodSignature(ConstructorInfo clrBinding)
            : this()
        {
            this.ClrBinding = clrBinding;
            BindClrParameters();
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

        public MethodBase ClrBinding { get; }
        public bool IsAsync { get; set; }
        public MethodParameter ReturnValue { get; set; }
        public List<MethodParameter> Parameters { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void BindClrReturnType(MethodInfo clrMethod)
        {
            if (clrMethod.ReturnType != null && clrMethod.ReturnType != typeof(void))
            {
                this.ReturnValue = new MethodParameter() {
                    Type = clrMethod.ReturnType
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void BindClrParameters()
        {
            var clrParameters = ClrBinding.GetParameters();

            for (int i = 0 ; i < clrParameters.Length ; i++)
            {
                var parameter = new MethodParameter(
                    name: clrParameters[i].Name,
                    position: i + 1,
                    type: clrParameters[i].ParameterType,
                    modifier: GetParameterModifier(clrParameters[i]));

                this.Parameters.Add(parameter);
            }
        }

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
