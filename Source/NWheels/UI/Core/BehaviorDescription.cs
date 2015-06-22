using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Uidl;

namespace NWheels.UI.Core
{
    public abstract class BehaviorDescription : UINodeDescription
    {
        protected BehaviorDescription(string idName, BehaviorType type, NotificationDescription parent)
            : base(idName, parent)
        {
            base.NodeType = UINodeType.Behavior;
            this.BehaviorType = type;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BehaviorType BehaviorType { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BehaviorDescription OnSuccess { get; set; }
        public BehaviorDescription OnFailure { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum BehaviorType
    {
        CallApi,
        Navigate,
        AlertUser,
        Broadcast,
        InvokeCommand,
        AlterModel,
        BranchByRule
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum ApiCallType
    {
        OneWay,
        RequestReply
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum BroadcastDirection
    {
        BubbleUp,
        TunnelDown,
        BubbleUpAndTunnelDown
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NavigateBehaviorDescription : BehaviorDescription
    {
        public NavigateBehaviorDescription(string idName, NotificationDescription parent)
            : base(idName, BehaviorType.Navigate, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UINodeType TargetType { get; set; }
        public string TargetIdName { get; set; }
        public string ContainerQualifiedName { get; set; }
        public string InputExpression { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class CallApiBehaviorDescription : BehaviorDescription
    {
        public CallApiBehaviorDescription(string idName, NotificationDescription parent)
            : base(idName, BehaviorType.CallApi, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ApiCallType CallType { get; set; }
        public string ContractName { get; set; }
        public string OperationName { get; set; }
        public string[] ParameterExpressions { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class InvokeCommandBehaviorDescription : BehaviorDescription
    {
        public InvokeCommandBehaviorDescription(string idName, NotificationDescription parent)
            : base(idName, BehaviorType.InvokeCommand, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DuplicateReference]
        public CommandDescription Command { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string CommandQualifiedName
        {
            get { return Command.QualifiedName; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class AlertBehaviorDescription : BehaviorDescription
    {
        public AlertBehaviorDescription(string idName, NotificationDescription parent)
            : base(idName, BehaviorType.AlertUser, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAlertDescription Alert { get; set; }
        public string[] ParameterExpressions { get; set; }
        public UserAlertDisplayMode DisplayMode { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class BroadcastBehaviorDescription : BehaviorDescription
    {
        public BroadcastBehaviorDescription(string idName, NotificationDescription parent)
            : base(idName, BehaviorType.Broadcast, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DuplicateReference]
        public NotificationDescription Notification { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string NotificationQualifiedName
        {
            get { return Notification.QualifiedName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BroadcastDirection Direction { get; set; }
        public string PayloadExpression { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class BranchByRuleBehaviorDescription : BehaviorDescription
    {
        public BranchByRuleBehaviorDescription(string idName, NotificationDescription parent)
            : base(idName, BehaviorType.BranchByRule, parent)
        {
            this.BranchRules = new List<BranchRuleDescription>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<BranchRuleDescription> BranchRules { get; private set; }
        public BehaviorDescription Otherwise { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class BranchRuleDescription
    {
        public string ConditionExpression { get; set; }
        public BehaviorDescription OnTrue { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class AlterModelBehaviorDescription : BehaviorDescription
    {
        public AlterModelBehaviorDescription(string idName, NotificationDescription parent)
            : base(idName, BehaviorType.AlterModel, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ReadExpression { get; set; }
        public string WriteExpression { get; set; }
    }

}
