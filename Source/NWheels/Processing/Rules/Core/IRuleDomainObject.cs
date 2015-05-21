namespace NWheels.Processing.Rules.Core
{
    /// <summary>
    /// Represents a pluggable user-defined objects, identified by a string name.
    /// </summary>
    /// <remarks>
    /// This interface is indented to represent user-defined variables (IRuleVariable), functions (IRuleFunction), and actions (IRuleAction) for rule engine.
    /// Rule engine runtime binds to IRuleDomainObject objects when loading rule system description and preparing its runtime version.
    /// </remarks>
    public interface IRuleDomainObject
    {
        /// <summary>
        /// Gets a string that identifies this oeprand. 
        /// </summary>
        string IdName { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets user-friendly description of the operand.
        /// </summary>
        string Description { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// A rule domain object which has one or more parameters
    /// </summary>
    public interface IParameterizedRuleDomainObject : IRuleDomainObject
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        void DescribeParameters(out RuleSystemDescription.ParameterDescription[] parameters);
    }
}
