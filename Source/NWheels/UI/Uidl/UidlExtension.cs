using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace NWheels.UI.Uidl
{
    public class UidlExtensionRegistration
    {
        public UidlExtensionRegistration(Type nodeType, Type extensionType)
        {
            this.NodeType = nodeType;
            this.ExtensionType = extensionType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type NodeType { get; private set; }
        public Type ExtensionType { get; private set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IUidlExtension
    {
        void ApplyTo(ControlledUidlNode node, UidlBuilder builder);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IUidlExtension<TNode> : IUidlExtension
        where TNode : ControlledUidlNode
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class UidlExtension<TNode, TData, TState> : IUidlExtension, IUidlExtension<TNode>
        where TNode : ControlledUidlNode
        where TData : class
        where TState : class
    {
        void IUidlExtension.ApplyTo(ControlledUidlNode node, UidlBuilder builder)
        {
            var typedNode = (TNode)node;

            if (ShouldApplyTo(typedNode))
            {
                var instantiatedNodes = builder.InstantiateDeclaredMemberNodes(node, this).ToArray();
                var presenterBuilder = new PresenterBuilder<TNode, TData, TState>(builder, node);
                ExtendPresenter(typedNode, presenterBuilder);

                builder.BuildNodes(instantiatedNodes);
                builder.DescribeNodePresenters(instantiatedNodes);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void ExtendPresenter(TNode node, PresenterBuilder<TNode, TData, TState> presenter);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual bool ShouldApplyTo(TNode node)
        {
            return true;
        }
    }
}
