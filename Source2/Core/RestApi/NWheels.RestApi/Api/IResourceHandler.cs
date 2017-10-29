using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NWheels.RestApi.Api
{
    public interface IResourceHandler<TData> : IResourceDescription
    {
        Task<IEnumerable<TData>> GetByQuery(IResourceQuery query);
        Task PostNew(TData data);
        Task PatchByQuery(IResourceQuery query, TData patch);
        Task DeleteByQuery(IResourceQuery query);
    }
    
    //---------------------------------------------------------------------------------------------------------------------------------------------------------
    
    public interface IResourceHanlder<in TId, TData> : IResourceHandler<TData>
    {
        Task<TData> GetById(TId id);
        Task PostById(TId id, TData data);
        Task PatchById(TId id, TData patch);
        Task DeleteById(TId id);
    }
}
