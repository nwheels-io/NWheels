namespace NWheels.Processing.Rules.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRuleFunction : IRuleDomainObject
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Represents a function in the rule engine.
    /// </summary>
    public interface IRuleFunction<TReturn> : IRuleFunction
    {
        TReturn GetValue();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Represents a 1-argument function in the rule engine.
    /// </summary>
    public interface IRuleFunction<TArg1, TReturn> : IRuleFunction
    {
        TReturn GetValue(TArg1 arg1);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Represents a 2-argument function in the rule engine.
    /// </summary>
    public interface IRuleFunction<TArg1, TArg2, TReturn> : IRuleFunction
    {
        TReturn GetValue(TArg1 arg1, TArg2 arg2);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Represents a 3-argument function in the rule engine.
    /// </summary>
    public interface IRuleFunction<TArg1, TArg2, TArg3, TReturn> : IRuleFunction
    {
        TReturn GetValue(TArg1 arg1, TArg2 arg2, TArg3 arg3);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Represents a 4-argument function in the rule engine.
    /// </summary>
    public interface IRuleFunction<TArg1, TArg2, TArg3, TArg4, TReturn> : IRuleFunction
    {
        TReturn GetValue(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);
    }
}
