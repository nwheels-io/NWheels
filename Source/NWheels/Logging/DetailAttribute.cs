using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class DetailAttribute : Attribute
    {
        public DetailAttribute()
        {
            ContentTypes = LogContentTypes.Text;
            MaxStringLength = 255;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogContentTypes ContentTypes { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int MaxStringLength { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IncludeInSingleLineText { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Indexed { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static DetailAttribute FromParameter(ParameterInfo parameter)
        {
            return parameter.GetCustomAttribute<DetailAttribute>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsDefinedOn(ParameterInfo parameter)
        {
            return (parameter.GetCustomAttribute<DetailAttribute>() != null);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class CommunicationMessageDetailAttribute : DetailAttribute
    {
        public CommunicationMessageDetailAttribute()
        {
            base.ContentTypes = LogContentTypes.CommunicationMessage;
            base.MaxStringLength = 1024;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class DataEntityDetailAttribute : DetailAttribute
    {
        public DataEntityDetailAttribute()
        {
            base.ContentTypes = LogContentTypes.DataEntity;
            base.MaxStringLength = 1024;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class PerformanceMeasurementDetailAttribute : DetailAttribute
    {
        public PerformanceMeasurementDetailAttribute()
        {
            base.ContentTypes = LogContentTypes.PerformanceStats;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class UserStoryDetailAttribute : DetailAttribute
    {
        public UserStoryDetailAttribute()
        {
            base.ContentTypes = LogContentTypes.UserStory;
        }
    }
}
