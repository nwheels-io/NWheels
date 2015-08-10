#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using NWheels.DataObjects;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class LazyLoadDomainObjectConvention : DecorationConvention
    {
        private readonly ITypeMetadata _metaType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LazyLoadDomainObjectConvention(ITypeMetadata metaType)
            : base(Will.DecorateMethods)
        {
            _metaType = metaType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DecorationConvention

        protected override void OnMethod(MethodMember member, Func<MethodDecorationBuilder> decorate)
        {
            
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly MethodInfo _s_
    }
}

#endif