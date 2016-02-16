using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NWheels.Extensions;
using NWheels.UI.Uidl;

namespace NWheels.UI
{
    public abstract class WidgetBase<TWidget, TData, TState> : WidgetUidlNode, UidlBuilder.IBuildableUidlNode
        where TWidget : WidgetBase<TWidget, TData, TState>
        where TData : class
        where TState : class
    {
        protected WidgetBase(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        void UidlBuilder.IBuildableUidlNode.Build(UidlBuilder builder)
        {
            base.ModelDataType = builder.RegisterMetaType(typeof(TData));
            base.ModelStateType = builder.RegisterMetaType(typeof(TState));

            var childNodesToBuild = new HashSet<AbstractUidlNode>();
            childNodesToBuild.UnionWith(builder.GetDeclaredMemberNodes(this));
            childNodesToBuild.UnionWith(this.GetNestedWidgets());
            
            builder.BuildNodes(childNodesToBuild.ToArray());

            OnBuild(builder);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        void UidlBuilder.IBuildableUidlNode.DescribePresenter(UidlBuilder builder)
        {
            var presenter = new PresenterBuilder<TWidget, TData, TState>(builder, this);
            
            DescribePresenter(presenter);
            builder.DescribeNodePresenters(this.GetNestedWidgets().Cast<AbstractUidlNode>().ToArray());
            PostDescribePresenter(presenter);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BindingSourceBuilder<TValue> Bind<TValue>(Expression<Func<ViewModel<TData, TState, Empty.Input>, TValue>> property)
        {
            return new BindingSourceBuilder<TValue>(this, property);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void DescribePresenter(PresenterBuilder<TWidget, TData, TState> presenter);

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void PostDescribePresenter(PresenterBuilder<TWidget, TData, TState> presenter)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnBuild(UidlBuilder builder)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class BindingSourceBuilder<TValue>
        {
            private readonly ControlledUidlNode _targetNode;
            private readonly Expression<Func<ViewModel<TData, TState, Empty.Input>, TValue>> _destinationProperty;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public BindingSourceBuilder(ControlledUidlNode targetNode, Expression<Func<ViewModel<TData, TState, Empty.Input>, TValue>> destinationProperty)
            {
                _targetNode = targetNode;
                _destinationProperty = destinationProperty;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ToModel(Expression<Func<ViewModel<TData, TState, Empty.Input>, TValue>> source)
            {
                var binding = new UidlModelBinding("MB1", _targetNode) {
                    DestinationQualifiedName = null, // null means the widget that declares the binding
                    DestinationExpression = _destinationProperty.ToNormalizedNavigationString("model"),
                    SourceExpression = source.ToNormalizedNavigationString("model")
                };
                binding.DestinationNavigations = binding.DestinationExpression.Split('.');

                _targetNode.DataBindings.Add(binding);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ToGlobalAppState<TAppState>(Expression<Func<TAppState, TValue>> source)
            {
                var binding = new UidlAppStateBinding("MB1", _targetNode) {
                    DestinationQualifiedName = null, // null means the widget that declares the binding
                    DestinationExpression = _destinationProperty.ToNormalizedNavigationString("model"),
                    SourceExpression = source.ToNormalizedNavigationString("appState")
                };
                binding.DestinationNavigations = binding.DestinationExpression.Split('.');

                _targetNode.DataBindings.Add(binding);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ToApi<TApiContract>(Expression<Func<TApiContract, TValue>> apiCall) { }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ToApi<TApiContract, TReply>(
                Expression<Func<TApiContract, TReply>> apiCall,
                Expression<Func<TReply, TValue>> valueSelector) { }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ToEntity<TEntity>(
                Action<IQueryable<TEntity>> query,
                Expression<Func<TEntity[], TValue>> valueSelector)
                where TEntity : class { }
        }
    }
}
