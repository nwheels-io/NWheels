using System;

namespace NWheels.RestApi.Api
{
    public interface IResourceDescription
    {
        string UriPath { get; }
        string ClassifierUriPath { get; }
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
}