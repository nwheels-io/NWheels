using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NWheels.Domains.DevOps.SystemLogs.UI.Screens;
using NWheels.Domains.Security.UI;
using NWheels.Samples.MyHRApp.Domain;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;
using NWheels.Domains.Security;

namespace NWheels.Samples.MyHRApp.UI
{
    public class HRMainScreen : ScreenBase<HRMainScreen, Empty.Input, Empty.Data, Empty.State>
    {
        public HRMainScreen(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<HRMainScreen, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = Console;
            Console
                .Dashboard(this.Departments)
                .StatusBarWidgets(CurrentUser)
                .Navigation(new {
                    Management = new { 
                        Departments = Goto(this.Departments).Appearance(icon: "table"),
                        Employees = Goto(this.Employees).Appearance(icon: "table"),
                    },
                    System = new {
                        @this = new {
                            Icon = "gear"
                        },
                        UserAccounts = Goto(this.Users).Appearance(icon: "table"),
                        SystemLog = Popup(App.SystemLog),
                    },
                    Logout = Menu.Action.InvokeCommand(CurrentUser.LogOut).Appearance(icon: "sign-out")
                });

            Console.ProfilePhoto.SetEntity<IProfilePhotoEntity>(x => x.ImageType, x => x.ImageContents);
            Console.ProfilePhoto.Bind(vm => vm.State.ProfilePhotoId).ToGlobalAppState<HRApp.IState>(x => x.LoggedInUser.ProfilePhotoId);
            Console.ProfilePhoto.Bind(vm => vm.State.PersonFullName).ToGlobalAppState<HRApp.IState>(x => x.LoggedInUser.FullName);
            Console.ProfilePhoto.Bind(vm => vm.State.UserType).ToGlobalAppState<HRApp.IState>(x => x.LoggedInUser.UserType);
            Console.ProfilePhoto.Bind(vm => vm.State.UserId).ToGlobalAppState<HRApp.IState>(x => x.LoggedInUser.UserId);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        
        public HRApp App { get; set; }
        public ManagementConsole Console { get; set; }
        public LoggedInUserWidget CurrentUser { get; set; }
        public CrudScreenPart<IDepartmentEntity> Departments { get; set; }
        public CrudScreenPart<IEmployeeEntity> Employees { get; set; }
        public UserAccountCrudScreenPart Users { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Menu.ItemAction Goto(IScreenPartWithInput<object> screenPart, params string[] userRoles)
        {
            return Menu.Action.Goto(screenPart, this.Console.MainContent).UserRoles(userRoles);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Menu.ItemAction Goto(IScreenPartWithInput<Empty.Input> screenPart, params string[] userRoles)
        {
            return Menu.Action.Goto(screenPart, this.Console.MainContent).UserRoles(userRoles);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private Menu.ItemAction Popup<TInput>(IScreenWithInput<TInput> screen, params string[] userRoles)
        {
            return Menu.Action.PopupScreen(screen).UserRoles(userRoles);
        }
    }
}