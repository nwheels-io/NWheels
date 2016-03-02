using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using Caliburn.Micro;
using NWheels.Extensions;
using IContainer = Autofac.IContainer;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon.Caliburn
{
    public class AutofacBootstrapperBase<TRootViewModel> : BootstrapperBase
    {
        private IContainer _container;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AutofacBootstrapperBase()
        {
            Initialize();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<TRootViewModel>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void Configure()
        {
            base.Configure();
            BuildContainer();
            CustomMessageBinders.Configure();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override object GetInstance(Type serviceType, string key)
        {
            object instance;

            if (string.IsNullOrWhiteSpace(key))
            {
                if (_container.TryResolve(serviceType, out instance))
                {
                    return instance;
                }
            }
            else
            {
                if (_container.TryResolveNamed(key, serviceType, out instance))
                {
                    return instance;
                }
            }

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", key ?? serviceType.Name));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.Resolve(typeof(IEnumerable<>).MakeGenericType(serviceType)) as IEnumerable<object>;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void BuildUp(object instance)
        {
            _container.InjectProperties(instance);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Execute.BeginOnUIThread(() => MessageBox.Show(e.Exception.GetUserFriendlyException().Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error));
            e.Handled = true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void RegisterComponents(ContainerBuilder builder)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void BuildContainer()
        {
            var builder = new ContainerBuilder();
            var assembliesToScan = AssemblySource.Instance.ToArray();

            // register view models
            builder.RegisterAssemblyTypes(assembliesToScan)
                .Where(type => type.Name.EndsWith("ViewModel"))
                .Where(type => typeof(INotifyPropertyChanged).IsAssignableFrom(type))
                .AsSelf()
                .InstancePerDependency();

            // register views
            builder.RegisterAssemblyTypes(assembliesToScan)
                .Where(type => type.Name.EndsWith("View"))
                .AsSelf()
                .InstancePerDependency();

            // register application plugins
            builder.RegisterAssemblyModules(assembliesToScan);

            builder.Register<IWindowManager>(c => new WindowManager()).InstancePerLifetimeScope();
            builder.Register<IEventAggregator>(c => new EventAggregator()).InstancePerLifetimeScope();

            RegisterComponents(builder);

            _container = builder.Build();
        }
    }
}
