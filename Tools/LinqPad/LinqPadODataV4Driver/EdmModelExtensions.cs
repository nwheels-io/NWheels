using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OData.Edm;

namespace LinqPadODataV4Driver
{
    public static class EdmModelExtensions
    {
        public static string GetEntityNamespace(this IEdmModel model)
        {
            return model.DeclaredNamespaces.FirstOrDefault(ns => ns != "Default");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetGeneratedContextClassFullName(this IEdmModel model)
        {
            return GetEntityNamespace(model) + ".GeneratedDataServiceContext";
        }
    }
}
