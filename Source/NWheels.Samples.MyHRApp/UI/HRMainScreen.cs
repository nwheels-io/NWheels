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
using NWheels.Samples.MyHRApp.Authorization;

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
                        @this = new {
                            Icon = "th-large"
                        },
                        Departments = Goto(this.Departments).Appearance(icon: "sitemap"),
                        Positions = Goto(this.Positions).Appearance(icon: "shield"),
                        Employees = Goto(this.Employees).Appearance(icon: "user"),
                    },
                    System = new {
                        @this = new {
                            Icon = "gear",
                            Authorization = new UidlAuthorization(HRClaims.UserRoleAdministrator)
                        },
                        UserAccounts = Goto(this.Users).Appearance(icon: "lock"),
                        SystemLog = Popup(App.SystemLog).Appearance(icon: "list"),
                    },
                    Logout = Menu.Action.InvokeCommand(CurrentUser.LogOut).Appearance(icon: "sign-out")
                });

            Console.ProfilePhoto.SetEntity<IProfilePhotoEntity>(x => x.ImageType, x => x.ImageContents);
            Console.ProfilePhoto.Bind(vm => vm.State.ProfilePhotoId).ToGlobalAppState<HRApp.IState>(x => x.LoggedInUser.ProfilePhotoId);
            Console.ProfilePhoto.Bind(vm => vm.State.PersonFullName).ToGlobalAppState<HRApp.IState>(x => x.LoggedInUser.FullName);
            Console.ProfilePhoto.Bind(vm => vm.State.UserType).ToGlobalAppState<HRApp.IState>(x => x.LoggedInUser.UserType);
            Console.ProfilePhoto.Bind(vm => vm.State.UserId).ToGlobalAppState<HRApp.IState>(x => x.LoggedInUser.UserId);

            Departments.Crud.Grid
                .Column(x => x.Id, size: FieldSize.Small)
                .Column(x => x.Name)
                .Column(x => x.ManagerName, title: "Manager")
                .Column(x => x.ListOfPositions, title: "Positions", size: FieldSize.ExtraLarge);
            Departments.Crud.Form
                .Field(x => x.Manager, setup: f => {
                    f.LookupDisplayProperty = "FullName";
                    var lookupGrid = (DataGrid<IEmployeeEntity>)f.NestedWidget;
                    lookupGrid
                        .Column(x => x.Ssn, title: "SSN")
                        .Column(x => x.FullName, title: "Name")
                        .Column(x => x.Phone)
                        .Column(x => x.Email, size: FieldSize.Large)
                        .Column(x => x.DepartmentName, title: "Department")
                        .Column(x => x.PositionName, title: "Position");
                })
                .Field(x => x.Positions, setup: f => {
                    var positionGrid = (DataGrid<IPositionEntity>)f.NestedWidget;
                    positionGrid
                        .Column(x => x.Id, size: FieldSize.Small)
                        .Column(x => x.Name);
                })
                .ShowFields(x => x.Id, x => x.Name, x => x.Manager, x => x.Positions);

            Positions.Crud.Grid
                .Column(x => x.Id, size: FieldSize.Small)
                .Column(x => x.Name);

            Employees.Crud.Grid
                .Column(x => x.Ssn, size: FieldSize.Medium, title: "SSN")
                .Column(x => x.FullName, title: "Name")
                .Column(x => x.Email, size: FieldSize.Large)
                .Column(x => x.Phone)
                .Column(x => x.DepartmentName, title: "Department")
                .Column(x => x.PositionName, title: "Position");
            Employees.Crud.Form
                .ShowFields(
                    x => x.Ssn, 
                    x => x.FullName, 
                    x => x.Email, 
                    x => x.Phone, 
                    x => x.Name, 
                    x => x.Address,
                    x => x.Employment)
                .Field(x => x.Ssn, label: "SSN")
                .Field(x => x.Name, setup: f => {
                    var nameForm = (Form<IPersonName>)f.NestedWidget;
                    nameForm.ShowFields(x => x.Title, x => x.FirstName, x => x.MiddleName, x => x.LastName);
                });
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        
        public HRApp App { get; set; }
        public ManagementConsole Console { get; set; }
        public LoggedInUserWidget CurrentUser { get; set; }
        public CrudScreenPart<IDepartmentEntity> Departments { get; set; }
        public CrudScreenPart<IPositionEntity> Positions { get; set; }
        public CrudScreenPart<IEmployeeEntity> Employees { get; set; }
        public UserAccountCrudScreenPart<IHRUserAccountEntity> Users { get; set; }

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