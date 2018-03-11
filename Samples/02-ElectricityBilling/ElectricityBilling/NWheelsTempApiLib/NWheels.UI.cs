using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Components;

namespace NWheels
{
    namespace UI
    {
        using NWheels.Microservices;

        public static class TypeContract
        {
            public class TemplateUrlAttribute : Attribute
            {
                public TemplateUrlAttribute(string url)
                {
                }
            }
            public class DefaultComponentAttribute : Attribute
            {
            }
        }

        public static class MemberContract
        {
            public class TemplatePlaceholderAttribute : Attribute
            {
                public TemplatePlaceholderAttribute(string placeholderName)
                {
                }
            }

            public class InitialViewAttribute : Attribute
            {
                public InitialViewAttribute(Type componenType)
                {
                }
            }

            public class BindToParentModelAttribute : Attribute
            {
                public static string ParentModelMember { get; set; }
            }

            public class OutputAttribute : Attribute
            {
            }

            public static class Storage
            {
                public class ClientMachineAttribute : Attribute
                {
                }
            }
        }

        public static class Empty
        {
            public class Args { }
            public class Model { }
            public class Session { }
        }

        //public class ClientCodeWriter<TContext> : CodeWriter<TContext>
        //{
        //    public PromiseExprWriter FIRE(Event @event, Expr data = null)
        //    {
        //        return new PromiseExprWriter();
        //    }

        //    public PromiseExprWriter FIRE<TData>(Event<TData> @event, Expression<Func<TData>> data)
        //    {
        //        return new PromiseExprWriter();
        //    }

        //    public Expr MUTATE<T>(T target, Expression<Func<T>> newValue)
        //    {
        //        return new PromiseExprWriter();
        //    }
        //}


        public class ClientError
        {
        }

        namespace Components
        {
            //public class Event
            //{
            //    public void Subscribe(params Statement[] handlerBlock)
            //    {
            //    }

            //    public class ConfigureAttribute : Attribute
            //    {
            //        public string SourceComponent { get; set; }
            //        public string SourceEvent { get; set; }
            //    }
            //}

            //public class Event<TData> : Event
            //{
            //    public void Subscribe(Func<Expr<TData>, Statement> handler)
            //    {
            //    }
            //}

            //public class Promise
            //{
            //    public Promise Then(Action action)
            //    {
            //        return new Promise();
            //    }

            //    public Promise Then(Func<Promise> action)
            //    {
            //        return new Promise();
            //    }

            //    public Promise<T> Then<T>(Func<T> action)
            //    {
            //        return new Promise<T>();
            //    }

            //    public Promise<T> Then<T>(Func<Promise<T>> action)
            //    {
            //        return new Promise<T>();
            //    }

            //    public Promise Catch<TException>(Action<TException> action)
            //        where TException : Exception
            //    {
            //        return new Promise();
            //    }

            //    public Promise Catch<TException>(Func<TException, Promise> action)
            //        where TException : Exception
            //    {
            //        return new Promise();
            //    }

            //    public Promise Finally(Action action)
            //    {
            //        return new Promise();
            //    }

            //    public static Promise Completed()
            //    {
            //        return new Promise();
            //    }
            //}

            //public class Promise<TResult> : Promise
            //{
            //    public Promise Then(Action<TResult> action)
            //    {
            //        return new Promise();
            //    }

            //    public Promise<TOther> Then<TOther>(Func<TResult, TOther> action)
            //    {
            //        return new Promise<TOther>();
            //    }

            //    public static Promise<TResult> Completed(TResult result)
            //    {
            //        return new Promise<TResult>();
            //    }
            //}

            public enum AlertSeverity
            {
                Default,
                Success,
                Info,
                Warning,
                Failure,
                Danger
            }

            [Flags]
            public enum AlertResponses
            {
                None = 0,
                OK = 0x1,
                Cancel = 0x2,
                OKCancel = OK | Cancel,
                Yes = 0x4,
                No = 0x8,
                Dismiss = 0x10,
                YesNo = Yes | No,
                YesNoCancel = Yes | No | Cancel,
            }

            public enum AlertBehavior
            {
                Sticky,
                Toast,
                Modal
            }

            public enum CommandSeverity
            {
                Default,
                Read,
                Write,
                Destroy
            }

            public enum CommandImportance
            {
                Default,
                Pirmary,
                Secondary,
                Utility
            }

            [Flags]
            public enum RowReOrderOptions
            {
                None = 0,
                Edit = 0x01,
                Arrows = 0x02,
                DragDrop = 0x04
            }

            public enum DataSourceOption
            {
                Model,
                Repository,
                Query
            }

            public interface INavigationTargetComponent<TNavigationArgs>
            {
            }

            public interface INavigationSourceComponent
            {
                Task NavigateTo(BaseComponent destination);
                Task NavigateTo<TArgs>(INavigationTargetComponent<TArgs> destination, TArgs arguments);
            }

            public interface IStructureBuilder
            {
                object ToggleGroup<T>(object structure, Action<T> onChange);
            }

            public static class Structure
            {
                public static IStructureBuilder Builder => null;
            }

            public class StyleConfiguration
            {
                public void Class(string className) { }
                public FontConfiguration Font { get; }

                public class FontConfiguration
                {
                    public void StrikeThrough()
                    {
                    }
                }
            }

            public abstract class BaseUIApp<TModel, TArgs> : 
                NavigationTargetComponent<TModel, TArgs>, 
                INavigationSourceComponent
            {
                public Task NavigateTo(BaseComponent destination)
                {
                    return Task.CompletedTask;
                }

                public Task NavigateTo<TArgs>(INavigationTargetComponent<TArgs> destination, TArgs arguments)
                {
                    return Task.CompletedTask;
                }
            }

            public class Command
            {
                public event Action OnExecute;
                public event Action<CommandStateQuery> OnUpdate;

                public class ConfigureAttribute : Attribute
                {
                    public CommandSeverity Severity { get; set; }
                    public CommandImportance Importance { get; set; }
                    public string Text { get; set; }
                    public string Icon { get; set; }
                    public bool HideIfDisabled { get; set; }
                    public bool ShowIfNotAuthorized { get; set; }
                }
            }

            public class Command<TData>
            {
                public event Action<TData> OnExecute;
                public event Action<CommandStateQuery> OnUpdate;
            }

            public class CommandStateQuery
            {
                public bool Enabled { get; set; }
            }

            public class ServerFaultException<TFault> : Exception
            {
                public TFault Code { get; }
            }

            public abstract class BaseComponent
            {
                protected T ServerComponent<T>() where T : class
                {
                    return default(T);
                }

                protected Task Alert(AlertSeverity severity, AlertBehavior behavior, string text)
                {
                    return Task.CompletedTask;
                }

                protected Task Alert(AlertBehavior behavior, Exception error)
                {
                    return Task.CompletedTask;
                }

                protected Task<AlertResponses> Alert(AlertBehavior behavior, AlertResponses responses, Exception error)
                {
                    return Task.FromResult(default(AlertResponses));
                }

                protected Task Alert<TData>(AlertSeverity severity, AlertBehavior behavior, TData data, string format)
                {
                    return Task.CompletedTask;
                }

                protected Task<AlertResponses> Alert(AlertSeverity severity, AlertBehavior behavior, AlertResponses responses, string text)
                {
                    return Task.FromResult(default(AlertResponses));
                }

                protected Task<AlertResponses> Alert<TData>(AlertSeverity severity, AlertBehavior behavior, AlertResponses responses, TData data, string format)
                {
                    return Task.FromResult(default(AlertResponses));
                }

                public class ConfigureAttribute : Attribute
                {
                    public string TemplatePlaceholder { get; set; }
                    public bool BindToParentModel { get; set; }
                    public string ParentModelMember { get; set; }
                }

                public class ServerComponentInvoication<TComponent>
                {
                    public Task Call(Expression<Func<TComponent, Task>> invocation)
                    {
                        return Task.CompletedTask;
                    }

                    public Task<TResult> Call<TResult>(Expression<Func<TComponent, Task<TResult>>> invocation)
                    {
                        return Task.FromResult(default(TResult));
                    }
                }
            }

            public abstract class BaseComponent<TModel> : BaseComponent
            {
                protected virtual void Configuration()
                {
                }

                protected virtual void Controller()
                {
                }

                protected Binding<T> Bind<T>(T targetMember)
                {
                    return new Binding<T>();
                }

                protected TModel Model { get; }
                protected event Func<Task> OnInit;
                protected event Action OnShow;

                protected class Binding<T>
                {
                    public void To(Func<TModel, T> modelMember) { }
                    public void To<S>(Func<TModel, S> modelMember, Func<S, T> transform) { }
                }
            }

            public abstract class NavigationTargetComponent<TModel, TNavigationArgs> : 
                BaseComponent<TModel>, 
                INavigationTargetComponent<TNavigationArgs>
            {
                protected event Action<TNavigationArgs> OnNavigatedHere;
            }

            //public static class FrameComponent
            //{
            //    public class ConfigureAttribute : BaseComponent.ConfigureAttribute
            //    {
            //        public string InitialViewMember { get; set; }
            //    }
            //}

            public class FrameComponent : BaseComponent<Empty.Model>, INavigationSourceComponent
            {
                public Task NavigateTo(BaseComponent destination)
                {
                    return Task.CompletedTask;
                }

                public Task NavigateTo<TArgs>(INavigationTargetComponent<TArgs> destination, TArgs arguments)
                {
                    return Task.CompletedTask;
                }

                public class ConfigureAttribute : BaseComponent.ConfigureAttribute
                {
                    public string InitialViewMember { get; set; }
                }
            }

            public class FormComponent<TModel> : BaseComponent<TModel>
            {
                public FormField<T> GetField<T>(Func<TModel, T> field)
                {
                    return new FormField<T>();
                }
            }

            public class FormField<T>
            {
                public event Func<T, Task> OnValidateUniqueValue;
            }

            public class ContentComponent<TModel> : BaseComponent<TModel>
            {
                //TBD
            }

            public static class TransactionComponent
            {
                public class ConfigureAttribute : BaseComponent.ConfigureAttribute
                {
                }
            }

            public class TransactionComponent<TModel> : BaseComponent<TModel>
            {
                public FormComponent<TModel> InputForm { get; }

                public event Func<Task> OnSubmit;
                public event Action OnCompleted;
            }

            public class ToolbarComponent : BaseComponent<Empty.Model>
            {
                public object Structure { get; set; }
            }

            public class DropDownComponent : BaseComponent<Empty.Model>
            {
                public ToolbarComponent Contents { get; }
            }

            public class ToggleGroupComponent : BaseComponent<Empty.Model>
            {
                public object Structure { get; set; }
            }

            public class LinkComponent : BaseComponent<Empty.Model>
            {
                public string Text { get; set; }
                public string Icon { get; set; }

                public event Func<Task> OnClick;
            }

            public class MenuItemComponent : BaseComponent<Empty.Model>
            {
                public event Action OnSelected;
            }

            public static class DataDrivenMenuComponent
            {
                public class ConfigureAttribute : Attribute
                {
                    public string TextFormat { get; set; }
                    public string[] GroupBy { get; set; }
                }
            }

            public class DataDrivenMenuComponent<TItem> : BaseComponent<DataDrivenMenuComponent<TItem>.ItemsModel>
            {
                public event Func<IEnumerable<TItem>> OnDataQuery;
                public event Action<TItem> OnItemSelected;

                public class ItemsModel
                {
                    IEnumerable<TItem> Items { get; }
                }
            }

            public static class DataSourceComponent
            {
                public class ConfigureAttribute : Attribute
                {
                    public DataSourceOption DataOption { get; set; }
                }
            }

            public class DataSourceComponent<TRecord>
            {
                public IEnumerable<TRecord> Source { get; set; }
                public IEnumerable<TRecord> Query { get; set; }
                public DataSourceOption DataOption { get; set; }
            }

            public static class DataGridComponent
            {
                public class ConfigureAttribute : Attribute
                {
                    public RowReOrderOptions RowReOrder { get; set; }
                }
            }

            public class DataGridComponent<TItem> : BaseComponent<IEnumerable<TItem>>
            {
                public RowReOrderOptions RowReOrder { get; set; }
                public GridColumnsConfiguration Columns { get; }
                public GridRowsConfiguration Rows { get; }

                public class GridColumnsConfiguration
                {
                    public GridColumnsConfiguration ConfigureAll(params Func<TItem, object>[] fields)
                    {
                        return this;
                    }

                    public GridColumnsConfiguration Configure(Func<TItem, object> field, ushort? relativeWidth)
                    {
                        return this;
                    }
                }

                public class GridRowsConfiguration
                {
                    public GridRowsConfiguration StyleIf(Func<TItem, bool> condition, Action<StyleConfiguration> style)
                    {
                        return this;           
                    }
                }
            }

            public static class CrudComponent
            {
                public class ConfigureAttribute : Attribute
                {
                }
            }

            public class CrudComponent<TItem> : BaseComponent<CrudComponent<TItem>.CrudModel>
            {
                public DataSourceComponent<TItem> Data { get; }
                public DataGridComponent<TItem> Grid { get; }
                public FormComponent<TItem> Form { get; }

                public event Func<IEnumerable<TItem>> OnQueryData;

                public class CrudModel
                {
                    public List<TItem> LocalData { get; set; }
                }
            }

            public class AdminComponent
            {
                public ToolbarComponent Toolbar { get; }
                public ToolbarComponent Navigation { get; }
                public ToolbarComponent Utilities { get; }
                public ToolbarComponent Status { get; }
                public FrameComponent ViewFrame { get; }
            }
        }

        namespace Web
        {
            public static class MicroserviceHostBuilderExtensions
            {
                public static MicroserviceHostBuilder ExposeWebApp<TWebApp>(
                    this MicroserviceHostBuilder hostBuilder,
                    string baseUrlPath = "/")
                {
                    return hostBuilder;
                }
            }

            public abstract class WebPage<TModel, TArgs> : NavigationTargetComponent<TModel, TArgs>
            {
            }

            public abstract class WebApp<TModel, TArgs> : BaseUIApp<TModel, TArgs>
            {
            }

            public static class TypeContract
            {
                public class IndexPageAttribute : NWheels.UI.TypeContract.DefaultComponentAttribute
                {
                }
            }
        }
    }
}
