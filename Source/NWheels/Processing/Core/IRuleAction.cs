namespace NWheels.Processing.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRuleAction : IRuleDomainObject
    {
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Represents a parameterless action in rule engine. 
    /// </summary>
    public interface IRuleAction<TDataContext> : IRuleAction
    {
        void Apply(IRuleActionContext<TDataContext> context);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Represents a 1-parameter action in rule engine. 
    /// </summary>
    public interface IRuleAction<TDataContext, T1> : IRuleAction, IParameterizedRuleDomainObject
    {
        void Apply(IRuleActionContext<TDataContext> context, T1 param1);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Represents a 2-parameter action in rule engine. 
    /// </summary>
    public interface IRuleAction<TDataContext, T1, T2> : IRuleAction, IParameterizedRuleDomainObject
    {
        void Apply(IRuleActionContext<TDataContext> context, T1 param1, T2 param2);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Represents a 3-parameter action in rule engine. 
    /// </summary>
    public interface IRuleAction<TDataContext, T1, T2, T3> : IRuleAction, IParameterizedRuleDomainObject
    {
        void Apply(IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Represents a 4-parameter action in rule engine. 
    /// </summary>
    public interface IRuleAction<TDataContext, T1, T2, T3, T4> : IRuleAction, IParameterizedRuleDomainObject
    {
        void Apply(IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3, T4 param4);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Represents a 5-parameter action in rule engine. 
    /// </summary>
    public interface IRuleAction<TDataContext, T1, T2, T3, T4, T5> : IRuleAction, IParameterizedRuleDomainObject
    {
        void Apply(IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Represents a 6-parameter action in rule engine. 
    /// </summary>
    public interface IRuleAction<TDataContext, T1, T2, T3, T4, T5, T6> : IRuleAction, IParameterizedRuleDomainObject
    {
        void Apply(IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Represents a 7-parameter action in rule engine. 
    /// </summary>
    public interface IRuleAction<TDataContext, T1, T2, T3, T4, T5, T6, T7> : IRuleAction, IParameterizedRuleDomainObject
    {
        void Apply(IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Represents a 8-parameter action in rule engine. 
    /// </summary>
    public interface IRuleAction<TDataContext, T1, T2, T3, T4, T5, T6, T7, T8> : IRuleAction, IParameterizedRuleDomainObject
    {
        void Apply(IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8);
    }
}
