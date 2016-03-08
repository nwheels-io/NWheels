using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.DataObjects.Core;

namespace NWheels.TypeModel
{
    public interface IMetadataMutation
    {
        void Apply(IComponentContext components, TypeMetadataCache metadataCache, TypeMetadataBuilder metaType);
        Type EntityContract { get; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class MetadataMutationBase<TEntity> : IMetadataMutation
        where TEntity : class
    {
        void IMetadataMutation.Apply(IComponentContext components, TypeMetadataCache metadataCache, TypeMetadataBuilder metaType)
        {
            this.Apply(components, metadataCache, metaType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IMetadataMutation.EntityContract
        {
            get
            {
                return typeof(TEntity);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void Apply(IComponentContext components, TypeMetadataCache metadataCache, TypeMetadataBuilder metaType);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DelegatingMetadataMutation<TEntity> : MetadataMutationBase<TEntity>
        where TEntity : class
    {
        private readonly Action<IComponentContext, TypeMetadataCache, TypeMetadataBuilder> _onApply;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DelegatingMetadataMutation(Action<IComponentContext, TypeMetadataCache, TypeMetadataBuilder> onApply)
        {
            _onApply = onApply;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of MetadataMutationBase<TEntity>

        protected override void Apply(IComponentContext components, TypeMetadataCache metadataCache, TypeMetadataBuilder metaType)
        {
            _onApply(components, metadataCache, metaType);
        }

        #endregion
    }
}
