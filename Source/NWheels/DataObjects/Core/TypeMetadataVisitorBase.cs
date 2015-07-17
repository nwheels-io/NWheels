using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.Extensions;

namespace NWheels.DataObjects.Core
{
    public class TypeMetadataVisitorBase : ITypeMetadataVisitor
    {
        public virtual TValue VisitAttribute<TValue>(string name, TValue value)
        {
            return value;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual TElementImpl VisitElement<TElement, TElementImpl>(TElementImpl element)
            where TElement : class, IMetadataElement
            where TElementImpl : class, TElement, new()
        {
            return VisitElement<TElement, TElementImpl>(GetDefaultElementName(element), element);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual TElementImpl VisitElement<TElement, TElementImpl>(string name, TElementImpl element) where TElement : class, IMetadataElement
            where TElementImpl : class, TElement, new()
        {
            if ( element == null )
            {
                return null;
            }

            if ( element is TypeMetadataBuilder )
            {
                return (TElementImpl)(object)VisitType((TypeMetadataBuilder)(object)element);
            }
            else if ( element is KeyMetadataBuilder )
            {
                return (TElementImpl)(object)VisitKey((KeyMetadataBuilder)(object)element);
            }
            else if ( element is PropertyMetadataBuilder )
            {
                return (TElementImpl)(object)VisitProperty((PropertyMetadataBuilder)(object)element);
            }
            else if ( element is TypeRelationalMappingBuilder )
            {
                return (TElementImpl)(object)VisitTypeRelationalMapping((TypeRelationalMappingBuilder)(object)element);
            }
            else if ( element is RelationMetadataBuilder )
            {
                return (TElementImpl)(object)VisitRelation((RelationMetadataBuilder)(object)element);
            }
            else if ( element is PropertyValidationMetadataBuilder )
            {
                return (TElementImpl)(object)VisitPropertyValidation((PropertyValidationMetadataBuilder)(object)element);
            }
            else if ( element is PropertyRelationalMappingBuilder )
            {
                return (TElementImpl)(object)VisitPropertyRelationalMapping((PropertyRelationalMappingBuilder)(object)element);
            }
            else
            {
                throw new NotSupportedException("Metadata element not supported: " + element.GetType().FullName);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual TElementImpl VisitElementReference<TElement, TElementImpl>(string name, TElementImpl element) where TElement : class, IMetadataElement
            where TElementImpl : class, TElement, new()
        {
            if ( element == null )
            {
                return null;
            }

            if ( element is KeyMetadataBuilder )
            {
                return (TElementImpl)(object)VisitKeyReference((KeyMetadataBuilder)(object)element);
            }
            else if ( element is TypeMetadataBuilder )
            {
                return (TElementImpl)(object)VisitTypeReference((TypeMetadataBuilder)(object)element);
            }
            else if ( element is PropertyMetadataBuilder )
            {
                return (TElementImpl)(object)VisitPropertyReference((PropertyMetadataBuilder)(object)element);
            }
            else
            {
                throw new NotSupportedException("Metadata element reference not supported: " + element.GetType().FullName);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void VisitElementList<TElement, TElementImpl>(string listName, IList<TElementImpl> elementList) where TElement : class, IMetadataElement
            where TElementImpl : class, TElement, new()
        {
            for ( int i = 0 ; i < elementList.Count ; i++ )
            {
                elementList[i] = VisitElement<TElement, TElementImpl>(elementList[i]);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void VisitElementReferenceList<TElement, TElementImpl>(string listName, IList<TElementImpl> elementList)
            where TElement : class, IMetadataElement where TElementImpl : class, TElement, new()
        {
            for ( int i = 0 ; i < elementList.Count ; i++ )
            {
                elementList[i] = VisitElementReference<TElement, TElementImpl>(listName, elementList[i]);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual TypeMetadataBuilder VisitType(TypeMetadataBuilder metadata)
        {
            return metadata;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual KeyMetadataBuilder VisitKey(KeyMetadataBuilder metadata)
        {
            if ( metadata != null )
            {
                metadata.AcceptVisitor(this);
            }

            return metadata;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual PropertyMetadataBuilder VisitProperty(PropertyMetadataBuilder metadata)
        {
            if ( metadata != null )
            {
                metadata.AcceptVisitor(this);
            }

            return metadata;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual RelationMetadataBuilder VisitRelation(RelationMetadataBuilder metadata)
        {
            if ( metadata != null )
            {
                metadata.AcceptVisitor(this);
            }

            return metadata;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual PropertyValidationMetadataBuilder VisitPropertyValidation(PropertyValidationMetadataBuilder metadata)
        {
            if ( metadata != null )
            {
                metadata.AcceptVisitor(this);
            }
            
            return metadata;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual TypeRelationalMappingBuilder VisitTypeRelationalMapping(TypeRelationalMappingBuilder metadata)
        {
            if ( metadata != null )
            {
                metadata.AcceptVisitor(this);
            }

            return metadata;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual PropertyRelationalMappingBuilder VisitPropertyRelationalMapping(PropertyRelationalMappingBuilder metadata)
        {
            if ( metadata != null )
            {
                metadata.AcceptVisitor(this);
            }
            
            return metadata;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual TypeMetadataBuilder VisitTypeReference(TypeMetadataBuilder metadata)
        {
            return metadata;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual KeyMetadataBuilder VisitKeyReference(KeyMetadataBuilder metadata)
        {
            return metadata;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual PropertyMetadataBuilder VisitPropertyReference(PropertyMetadataBuilder metadata)
        {
            return metadata;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetDefaultElementName<TElement>(TElement element)
            where TElement : IMetadataElement
        {
            if ( element != null )
            {
                return element.ElementType.Name.TrimLead("I").TrimTail("Metadata");
            }
            else
            {
                return null;
            }
        }
    }
}
