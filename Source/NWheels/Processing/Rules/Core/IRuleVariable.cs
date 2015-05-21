namespace NWheels.Processing.Rules.Core
{
    /// <summary>
    /// Represents a variable in the rule engine. 
    /// A variable is basically a function that gets TContext and returns TValue.
    /// </summary>
    public interface IRuleVariable<TDataContext, TValue> : IRuleDomainObject
    {
        /// <summary>
        /// Returns value of this variable from specified data context object.
        /// </summary>
        /// <param name="context">
        /// The data context object.
        /// </param>
        /// <returns>
        /// The value of the variable based on the specified context.
        /// </returns>
        TValue GetValue(TDataContext context);
    }
}
