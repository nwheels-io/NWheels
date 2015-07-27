using System.Collections.Generic;

namespace NWheels.DataObjects.Core
{
    public interface ITypeMetadataVisitor
    {
        TValue VisitAttribute<TValue>(string name, TValue value);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TElementImpl VisitElement<TElement, TElementImpl>(TElementImpl element) 
            where TElement : class, IMetadataElement
            where TElementImpl : class, TElement, new();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TElementImpl VisitElement<TElement, TElementImpl>(string name, TElementImpl element)
            where TElement : class, IMetadataElement
            where TElementImpl : class, TElement, new();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TElementImpl VisitElementReference<TElement, TElementImpl>(string name, TElementImpl element)
            where TElement : class, IMetadataElement
            where TElementImpl : class, TElement, new();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void VisitElementList<TElement, TElementImpl>(string listName, IList<TElementImpl> elementList)
            where TElement : class, IMetadataElement
            where TElementImpl : class, TElement, new();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void VisitElementReferenceList<TElement, TElementImpl>(string listName, IList<TElementImpl> elementList)
            where TElement : class, IMetadataElement
            where TElementImpl : class, TElement, new();
    }
}
