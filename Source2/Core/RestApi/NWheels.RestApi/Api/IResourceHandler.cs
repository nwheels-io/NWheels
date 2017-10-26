using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NWheels.RestApi.Api
{
    public interface IResourceHandler
    {
        string FullPath { get; }
        string ClassifierPath { get; }
        string Description { get; }
        Type KeyType { get; }
        Type DataType { get; }
        bool CanGetById { get; }
        bool CanGetByQuery { get; }
        bool CanPostNew { get; }
        bool CanPostById { get; }
        bool CanPatchById { get; }
        bool CanPatchByQuery { get; }
        bool CanDeleteById { get; }
        bool CanDeleteByQuery { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IResourceHandler<TData> : IResourceHandler
    {
        Task<IEnumerable<TData>> GetByQuery(IResourceQuery query);
        Task PostNew(TData data);
        Task PatchByQuery(IResourceQuery query, TData patch);
        Task DeleteByQuery(IResourceQuery query);
    }
    
    //---------------------------------------------------------------------------------------------------------------------------------------------------------
    
    public interface IResourceHandler<in TId, TData> : IResourceHandler<TData>
    {
        Task<TData> GetById(TId id);
        Task PostById(TId id, TData data);
        Task PatchById(TId id, TData patch);
        Task DeleteById(TId id);
    }
}
