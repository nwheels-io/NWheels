using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Extensions;
using NWheels.TypeModel;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace, Name = "Crud")]
    public class Crud<TEntity> : WidgetBase<Crud<TEntity>, Empty.Data, ICrudViewState<TEntity>>
        where TEntity : class
    {
        public Crud(string idName, ControlledUidlNode parent)
            : this(idName, parent, DataGridMode.Standalone)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Crud(string idName, ControlledUidlNode parent, DataGridMode mode)
            : base(idName, parent)
        {
            this.WidgetType = "Crud";
            this.TemplateName = "Crud";
            this.MetaType = this.MetadataCache.GetTypeMetadata(typeof(TEntity));
            this.EntityName = MetaType.QualifiedName;
            this.Mode = mode;
            this.EntityCommands = new List<UidlCommand>();
            this.StaticCommands = new List<UidlCommand>();

            CreateForm();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Crud(string idName, ControlledUidlNode parent, IPropertyMetadata parentEntityNavigation)
            : this(idName, parent, DataGridMode.Inline)
        {
            if ( parentEntityNavigation.RelationalMapping != null &&
                 parentEntityNavigation.RelationalMapping.StorageStyle == PropertyStorageStyle.InverseForeignKey &&
                 parentEntityNavigation.Relation != null &&
                 parentEntityNavigation.Relation.InverseProperty != null )
            {
                this.InlineStorageStyle = parentEntityNavigation.RelationalMapping.StorageStyle;
                this.InverseNavigationProperty = parentEntityNavigation.Relation.InverseProperty.Name;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return new WidgetUidlNode[] { Form, FormTypeSelector, Grid }.Where(w => w != null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FilterByType<TDerived>() where TDerived : TEntity
        {
            this.EntityTypeFilter = MetadataCache.GetTypeMetadata(typeof(TDerived)).QualifiedName;

            if ( FormTypeSelector != null )
            {
                FormTypeSelector.FilterByType(typeof(TDerived));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddStaticCommands(params UidlCommand[] staticCommands)
        {
            StaticCommands.AddRange(staticCommands);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddEntityCommands(params UidlCommand[] entityCommands)
        {
            EntityCommands.AddRange(entityCommands);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string EntityName { get; set; }
        [DataMember]
        public string EntityTypeFilter { get; set; }
        [DataMember]
        public DataGridMode Mode { get; set; }
        [DataMember]
        public PropertyStorageStyle? InlineStorageStyle { get; set; }
        [DataMember]
        public string InverseNavigationProperty { get; set; }
        [DataMember]
        public DataGrid<TEntity> Grid { get; set; }
        [DataMember, ManuallyAssigned]
        public Form<TEntity> Form { get; set; }
        [DataMember, ManuallyAssigned]
        public TypeSelector FormTypeSelector { get; set; }
        [DataMember, ManuallyAssigned]
        public List<UidlCommand> StaticCommands { get; set; }
        [DataMember, ManuallyAssigned]
        public List<UidlCommand> EntityCommands { get; set; }
        [DataMember]
        public bool UpdateCommandsOnSelection { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand Save { get; set; }
        public UidlCommand Cancel { get; set; }
        public UidlNotification RefreshRequested { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<IPromiseFailureInfo> QueryEntityFailed { get; set; }
        public UidlNotification<IPromiseFailureInfo> NewDomainObjectFailed { get; set; }
        public UidlNotification DeleteEntityCompleted { get; set; }
        public UidlNotification<IPromiseFailureInfo> DeleteEntityFailed { get; set; }
        public UidlNotification StoreEntityCompleted { get; set; }
        public UidlNotification<IPromiseFailureInfo> StoreEntityFailed { get; set; }
        public UidlNotification<TEntity> SelectedEntityChanged { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Crud<TEntity>, Empty.Data, ICrudViewState<TEntity>> presenter)
        {
            this.Grid.UsePascalCase = true;
            this.Grid.Mode = this.Mode;
            this.Save.Icon = "check";
            this.Save.Severity = CommandSeverity.Change;
            this.Save.Kind = CommandKind.Submit;
            this.Cancel.Icon = "times";
            this.Cancel.Severity = CommandSeverity.Loose;
            this.Cancel.Kind = CommandKind.Reject;

            presenter.On(QueryEntityFailed).UserAlertFrom<ICrudUserAlerts>().ShowPopup((x, vm) => x.FailedToQueryDataFromServer(), faultInfo: vm => vm.Input);
            presenter.On(Grid.QueryFailed).UserAlertFrom<ICrudUserAlerts>().ShowPopup((x, vm) => x.FailedToQueryDataFromServer(), faultInfo: vm => vm.Input);
            presenter.On(NewDomainObjectFailed).UserAlertFrom<ICrudUserAlerts>().ShowPopup((x, vm) => x.FailedToRetrieveDefaultValues(), faultInfo: vm => vm.Input);
            presenter.On(DeleteEntityCompleted).UserAlertFrom<ICrudUserAlerts>().ShowPopup((x, vm) => x.DeleteOperationCompleted());
            presenter.On(DeleteEntityFailed).UserAlertFrom<ICrudUserAlerts>().ShowPopup((x, vm) => x.DeleteOperationFailed(), faultInfo: vm => vm.Input);
            presenter.On(StoreEntityCompleted).UserAlertFrom<ICrudUserAlerts>().ShowPopup((x, vm) => x.DataSuccessfullySaved());
            presenter.On(StoreEntityFailed).UserAlertFrom<ICrudUserAlerts>().ShowPopup((x, vm) => x.FailedToSaveData(), faultInfo: vm => vm.Input);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void PostDescribePresenter(PresenterBuilder<Crud<TEntity>, Empty.Data, ICrudViewState<TEntity>> presenter)
        {
            ConfigureForms();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuild(UidlBuilder builder)
        {
            builder.RegisterMetaType(typeof(TEntity));

            var metaType = builder.MetadataCache.GetTypeMetadata(typeof(TEntity));

            builder.BuildManuallyInstantiatedNodes(Form, FormTypeSelector);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of RootContentUidlNode

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables()
                .Concat(EntityCommands.Select(cmd => cmd.Text))
                .Concat(StaticCommands.Select(cmd => cmd.Text));
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal ITypeMetadata MetaType { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void CreateForm()
        {
            var formOrTypeSelector = UidlUtility.CreateFormOrTypeSelector(MetaType, "Form", parent: this, isInline: false);

            this.Form = formOrTypeSelector as Form<TEntity>;
            this.FormTypeSelector = formOrTypeSelector as TypeSelector;

            if (FormTypeSelector != null)
            {
                FormTypeSelector.ParentModelProperty = "entity";
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureForms()
        {
            if ( Form != null )
            {
                ConfigureForm(Form);
            }

            if ( FormTypeSelector != null )
            {
                foreach ( var typeForm in FormTypeSelector.Selections.Select(s => s.Widget).OfType<IUidlForm>() )
                {
                    ConfigureForm(typeForm);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureForm(IUidlForm form)
        {
            form.UsePascalCase = true;
            form.NeedsAuthorize = true;

            foreach ( var field in form.Fields )
            {
                //if (field.FieldType == FormFieldType.Lookup && 
                //    (field.Modifiers & FormFieldModifiers.Ellipsis) != 0 && 
                //    !string.IsNullOrEmpty(field.LookupDisplayProperty))
                //{
                //    Grid.EnsureDataForLookupField(field);
                //}

                var nestedForm = field.NestedWidget as IUidlForm;

                if ( nestedForm != null )
                {
                    ConfigureForm(nestedForm);
                }
            }

            //if ( form.Commands.Count == 0 )
            //{
            //    form.Commands.Add(Save);
            //    form.Commands.Add(Cancel);
            //}
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    [ViewModelContract]
    public interface ICrudViewState<TEntity>
    {
        TEntity CurrentEntity { get; set; }
        ICollection<ICrudNavigatedEntity> NavigatedEntities { get; set; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    [ViewModelContract]
    public interface ICrudNavigatedEntity
    {
        Type Contract { get; set; }
        string Name { get; set; }
        object Id { get; set; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    [ViewModelContract]
    public interface ICrudFormState<TEntity>
    {
        TEntity CurrentEntity { get; set; }
        ICollection<ICrudNavigatedEntity> NavigatedEntities { get; set; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ICrudUserAlerts : IUserAlertRepository
    {
        [ErrorAlert]
        UidlUserAlert FailedToQueryDataFromServer();

        [ErrorAlert]
        UidlUserAlert FailedToRetrieveDefaultValues();

        [SuccessAlert]
        UidlUserAlert DeleteOperationCompleted();

        [ErrorAlert]
        UidlUserAlert DeleteOperationFailed();

        [SuccessAlert]
        UidlUserAlert DataSuccessfullySaved();

        [ErrorAlert]
        UidlUserAlert FailedToSaveData();
    }
}
