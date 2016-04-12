using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.DataObjects.Core;
using NWheels.TypeModel;
using NWheels.UI.Uidl;

namespace NWheels.UI
{
    public interface IUidlMutation
    {
        void Apply(IComponentContext components, TypeMetadataCache metadataCache, UidlApplication application);
        Type ApplicationType { get; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class UidlMutationBase<TApp> : IUidlMutation
        where TApp : UidlApplication
    {
        void IUidlMutation.Apply(IComponentContext components, TypeMetadataCache metadataCache, UidlApplication application)
        {
            this.Apply(components, metadataCache, (TApp)application);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IUidlMutation.ApplicationType
        {
            get
            {
                return typeof(TApp);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void Apply(IComponentContext components, TypeMetadataCache metadataCache, TApp application);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DelegatingUidlMutation<TApp> : UidlMutationBase<TApp>
        where TApp : UidlApplication
    {
        private readonly Action<IComponentContext, TypeMetadataCache, TApp> _onApply;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DelegatingUidlMutation(Action<IComponentContext, TypeMetadataCache, TApp> onApply)
        {
            _onApply = onApply;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of MetadataMutationBase<TEntity>

        protected override void Apply(IComponentContext components, TypeMetadataCache metadataCache, TApp application)
        {
            _onApply(components, metadataCache, application);
        }

        #endregion
    }
}
