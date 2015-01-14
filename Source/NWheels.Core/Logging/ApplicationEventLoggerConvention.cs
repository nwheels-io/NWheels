//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Hapil;
//using Hapil.Writers;

//namespace NWheels.Core.Logging
//{
//    public class ApplicationEventLoggerConvention : ImplementationConvention
//    {
//        public ApplicationEventLoggerConvention()
//            : base(Will.ImplementPrimaryInterface)
//        {
//        }

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
//        {
//            writer.AllMethods().Implement(ImplementApplicationEventMethod);
//        }

//        private void ImplementApplicationEventMethod(TemplateMethodWriter writer)
//        {
//            var declaration = writer.OwnerMethod.MethodDeclaration;

//            if ( declaration.IsVoid() 
//        }


//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        private PropertyWriterBase.IPropertyWriterGetter ImplementApplicationEventProperty(TemplatePropertyWriter writer)
//        {
//            var declaration = writer.OwnerProperty.PropertyDeclaration;
                
                
                

//        }

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------


//    }
//}
