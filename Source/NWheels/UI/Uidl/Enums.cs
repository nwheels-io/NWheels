using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Uidl
{
    public enum UidlNodeType
    {
        Application,
        Screen,
        ScreenPart,
        Widget,
        UserAlert,
        Command,
        DataBinding,
        Behavior,
        Notification
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum UidlMetaTypeKind
    {
        Key,
        Value,
        Object
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum BindingSourceType
    {
        Model,
        Api,
        Entity
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

    public enum NavigationType
    {
        LoadInline,
        PopupModal
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum ApiCallType
    {
        OneWay,
        RequestReply
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum ApiCallTargetType
    {
        TransactionScript,
        ServiceMethod,
        EntityMethod,
        EntityChangeSet,
        DomainApi
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum BroadcastDirection
    {
        BubbleUp,
        TunnelDown,
        BubbleUpAndTunnelDown
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum ModalResult
    {
        None,
        OK,
        Cancel
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum UserAlertResult
    {
        None,
        OK,
        Cancel,
        Abort,
        Retry,
        Ignore,
        Yes,
        No,
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum UserAlertType
    {
        Info,
        Success,
        Warning,
        Error
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum UserAlertDisplayMode
    {
        Inline,
        Popup
    }
}
