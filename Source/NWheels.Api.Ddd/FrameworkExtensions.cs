using System;

namespace NWheels.Api.Ddd
{
    public static class FrameworkExtensions
    {
        public static T NewDomainObject<T>(this IFramework framework, Action<T> initializer = null)
            where T : class
        {
            throw new NotImplementedException();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public static TRef MakeEntityReference<TRef, TEntity, TId>(this IFramework framework, TId id)
            where TEntity : class
            where TRef : IEntityReference<TEntity, TId>
        {
            throw new NotImplementedException();
        }
    }
}