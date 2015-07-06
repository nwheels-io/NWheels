using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Library.Expressions;
using Microsoft.Data.Edm.Validation;
using NUnit.Framework;
using NWheels.Domains.Security;
using NWheels.Extensions;

namespace NWheels.Stacks.ODataBreeze.SystemTests
{
    [TestFixture]
    public class MetadataTests
    {
        [Test]
        public void CanWriteMetadata()
        {
            //var objectIdTyoe = new EdmPrimitiveTypeDefinition()
            //var objectIdTypeReference = new EdmPrimitiveTypeReference();

            var model = new EdmModel();
            var entityContainer = new EdmEntityContainer("", "Default");

            var operationPermissionEntityType = new EdmEntityType(typeof(IOperationPermissionEntity).Namespace, typeof(IOperationPermissionEntity).Name.TrimLead("I").TrimTail("Entity"));
            var operationPermissionEntityTypeIdProperty = operationPermissionEntityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.String, false);
            operationPermissionEntityType.AddKeys(operationPermissionEntityTypeIdProperty);
            operationPermissionEntityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String, false);
            operationPermissionEntityType.AddStructuralProperty("ClaimType", EdmPrimitiveTypeKind.String, true);
            operationPermissionEntityType.AddStructuralProperty("ClaimValue", EdmPrimitiveTypeKind.String, false);
            
            var userRoleEntityType = new EdmEntityType(typeof(IUserAccountEntity).Namespace, typeof(IUserAccountEntity).Name.TrimLead("I").TrimTail("Entity"));
            var userRoleEntityTypeIdProperty = userRoleEntityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.String, false);
            userRoleEntityType.AddKeys(userRoleEntityTypeIdProperty);
            userRoleEntityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String, false);
            userRoleEntityType.AddStructuralProperty("ClaimType", EdmPrimitiveTypeKind.String, true);
            userRoleEntityType.AddStructuralProperty("ClaimValue", EdmPrimitiveTypeKind.String, false);

            var associatedPermissionsProperty = userRoleEntityType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo() {
                Name = "AssociatedPermissions",
                ContainsTarget = false,
                Target = operationPermissionEntityType,
                TargetMultiplicity = EdmMultiplicity.Many,
                DependentProperties = new[] { operationPermissionEntityTypeIdProperty },
                OnDelete = EdmOnDeleteAction.None
            });

            var operationPermissionEntitySet = entityContainer.AddEntitySet("OperationPermission", operationPermissionEntityType);
            var userRoleEntitySet = entityContainer.AddEntitySet("UserRole", userRoleEntityType);
            
            model.AddElement(entityContainer);
            model.AddElement(userRoleEntityType);

            var output = new StringBuilder();
            IEnumerable<EdmError> errors;

            using ( var xmlWriter = XmlWriter.Create(output) )
            {
                if ( !EdmxWriter.TryWriteEdmx(model, xmlWriter, EdmxTarget.OData, out errors) )
                {
                    if ( errors != null )
                    {
                        foreach ( var error in errors )
                        {
                            Console.WriteLine(error.ToString());
                        }
                    }

                    Assert.Fail("ERRORS!");
                }

                xmlWriter.Flush();
            }
            
            Console.WriteLine(output.ToString());
        }
    }
}
