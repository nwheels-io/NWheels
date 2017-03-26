using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Mechanism.Syntax.Members;
using NWheels.Compilation.Policy.Relaxed;
using NWheels.Platform.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NWheels.Frameworks.Ddd.RestApi
{
    public class TxResourceHandlerListConvention : ConventionBase<Empty.ContextExtension>
    {
        protected override void Validate(ITypeFactoryContext<Empty.ContextExtension> context)
        {
            var txType = context.Key.PrimaryContract;

            if (!txType.HasAttribute<TransactionScriptComponentAttribute>())
            {
                throw NewValidationException(txType, $"Must be marked with a {typeof(TransactionScriptComponentAttribute).Name}.");
            }

            if (!txType.SelectPublicInstance<MethodMember>(where: m => m.HasAttribute<TransactionScriptMethodAttribute>()).Any())
            {
                throw NewValidationException(txType, $"Must have at least one method marked with {typeof(TransactionScriptMethodAttribute).Name}.");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void Implement(ITypeFactoryContext<Empty.ContextExtension> context, TypeWriter writer)
        {
            var txHandlerTypes = GenerateTxHandlerTypes(context, writer);

            writer.IMPLEMENTS<ITxResourceHandlerList>();
            writer.PUBLIC().METHOD_OF<ITxResourceHandlerList>(x => x.GetHandlerTypes).BODY(G =>             {
                G.RETURN(G.ARRAY<Type>(txHandlerTypes));
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private List<TypeMember> GenerateTxHandlerTypes(ITypeFactoryContext<Empty.ContextExtension> context, TypeWriter writer)
        {
            var txType = context.Key.PrimaryContract;
            var txMethods = txType.SelectPublicInstance<MethodMember>(where: m => m.HasAttribute<TransactionScriptMethodAttribute>());
            var txHandlerTypes = new List<TypeMember>();

            foreach (var method in txMethods)
            {
                txHandlerTypes.Add(GenerateTxHandlerType(context, writer, txType, method));
            }

            return txHandlerTypes;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeMember GenerateTxHandlerType(
            ITypeFactoryContext<Empty.ContextExtension> context, 
            TypeWriter writer, 
            TypeMember txType, 
            MethodMember txMethod)
        {
            var handlerType = writer.PRIVATE().CLASS($"HandlerOf_{txMethod.Name}").EXTENDS<RestResourceHandlerBase>();

            handlerType.PROTECTED().METHOD(nameof(RestResourceHandlerBase.OnPatch))
        }
    }
}
