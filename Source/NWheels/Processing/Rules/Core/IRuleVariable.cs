namespace NWheels.Processing.Rules.Core
{
    /// <summary>
    /// Represents a variable in the rule engine. 
    /// A variable is basically a function that gets TContext and returns TValue.
    /// </summary>
    public interface IRuleVariable<in TDataContext, out TValue> : IRuleDomainObject
    {
        /// <summary>
        /// Returns value of this variable from specified data context object.
        /// </summary>
        /// <param name="context">
        /// The data context object. 
        /// It is declared as contravariant, which allows casting this variable to be used with a derived data context type.
        /// </param>
        /// <returns>
        /// The value of the variable based on the specified context.
        /// It is declared as covariant, which allows casting this variable to be used with an ancestor result value type.
        /// </returns>
        TValue GetValue(TDataContext context);
    }
}
