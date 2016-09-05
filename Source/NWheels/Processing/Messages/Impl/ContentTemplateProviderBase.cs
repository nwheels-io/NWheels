using SmartFormat;

namespace NWheels.Processing.Messages.Impl
{
    public abstract class ContentTemplateProviderBase : IContentTemplateProvider
    {
        public abstract string GetTemplate(object contentType);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string Format(string template, object data)
        {
            return Smart.Format(template, data);
        }
    }
}
