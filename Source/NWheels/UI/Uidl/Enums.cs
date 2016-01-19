using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Uidl
{
    public enum UidlNodeType
    {
        Application = 10,
        Screen = 20,
        ScreenPart = 30,
        Widget = 40,
        UserAlert = 50,
        Command = 60,
        CommandGroup = 70,
        DataBinding = 80,
        Behavior = 90,
        Notification = 100
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum UidlMetaTypeKind
    {
        Key = 10,
        Value = 20,
        Object = 30
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum BindingSourceType
    {
        Model = 10,
        AppState = 20,
        Api = 30,
        Entity = 40
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum BehaviorType
    {
        CallApi = 10,
        Navigate = 20,
        AlertUser = 30,
        Broadcast = 40,
        InvokeCommand = 50,
        QueryModel = 60,
        AlterModel = 70,
        BranchByRule = 80,
        ActivateSessionTimeout = 90,
        DeactivateSessionTimeout = 100,
        RestartApp = 110,
        DownloadContent = 120
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum NavigationType
    {
        LoadInline = 10,
        PopupModal = 20
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum ApiCallType
    {
        OneWay = 10,
        RequestReply = 20,
        RequestReplyAsync = 30,
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum ApiCallResultType
    {
        Command = 10,
        EntityQuery = 20,
        EntityQueryExport = 30
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum ApiCallTargetType
    {
        TransactionScript = 10,
        ServiceMethod = 20,
        EntityMethod = 30,
        EntityChangeSet = 40,
        DomainApi = 50
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum BroadcastDirection
    {
        BubbleUp = 10,
        TunnelDown = 20,
        BubbleUpAndTunnelDown = 30
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum ModalResult
    {
        None = 10,
        OK = 20,
        Cancel = 30
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum UserAlertResult
    {
        None = 10,
        OK = 20,
        Cancel = 30,
        Abort = 40,
        Retry = 50,
        Ignore = 60,
        Yes = 70,
        No = 80,
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum UserAlertType
    {
        Info = 10,
        Success = 20,
        Warning = 30,
        Error = 40
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum UserAlertDisplayMode
    {
        Inline = 10,
        Popup = 20,
        Modal = 30
    }
}
