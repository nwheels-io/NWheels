using System;

namespace NWheels.RestApi.Api
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RestApiComponentAttribute : Attribute
    {
    }
}