using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.Exceptions
{
    public class EntityValidationException : Exception
    {
        public EntityValidationException(string message)
            : base(message)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityValidationException(Type contractType, string propertyName, string errorMessage)
            : base(FormatValidationErrorMessage(contractType, propertyName, errorMessage))
        {
            this.ContractType = contractType;
            this.PropertyName = propertyName;
            this.ErrorMessage = errorMessage;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ContractType { get; set; }
        public string PropertyName { get; set; }
        public string ErrorMessage { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static EntityValidationException For<TEntity>(Expression<Func<TEntity, object>> property, string message)
        {
            return new EntityValidationException(
                contractType: typeof(TEntity),
                propertyName: property.GetPropertyInfo().Name,
                errorMessage: message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string FormatValidationErrorMessage(Type contractType, string propertyName, string errorMessage)
        {
            return string.Format(
                "Validation for entity '{0}' failed. Property '{1}': {2}",
                contractType.Name, propertyName, errorMessage);
        }
    }
}
