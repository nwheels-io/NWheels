using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.DataObjects;

namespace NWheels.Testing.DataObjects
{
    public class Jsonlike : IMetadataElementVisitor
    {
        private readonly HashSet<Type> _elementTypeFilter;
        private readonly StringBuilder _output = new StringBuilder();
        private bool _isFirstProperty = true;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Jsonlike(Type[] elementTypeFilter)
        {
            if ( elementTypeFilter != null && elementTypeFilter.Length > 0 )
            {
                _elementTypeFilter = new HashSet<Type>(elementTypeFilter);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IMetadataElementVisitor Members

        TValue IMetadataElementVisitor.VisitAttribute<TValue>(string name, TValue value)
        {
            if ( (object)value != null && !value.Equals(default(TValue)))
            {
                var valueString = GetAttributeValueString(value);

                if ( StringNeedsEscaping(valueString) )
                {
                    valueString = "\"" + valueString.Replace("\"", @"\""") + "\"";
                }

                _output.AppendFormat("{0}{1}:{2}", _isFirstProperty ? "" : ",", name.ToCamelCase(), valueString);
                _isFirstProperty = false;
            }

            return value;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TElementImpl IMetadataElementVisitor.VisitElement<TElement, TElementImpl>(TElementImpl element)
        {
            if ( element == null )
            {
                return null;
            }

            var elementName = GetDefaultElementName(element);
            return ((IMetadataElementVisitor)this).VisitElement<TElement, TElementImpl>(elementName, element);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TElementImpl IMetadataElementVisitor.VisitElement<TElement, TElementImpl>(string name, TElementImpl element)
        {
            if ( element == null )
            {
                return null;
            }

            if ( ShouldIncludeElement(element) )
            {
                _output.AppendFormat("{0}{1}:{{", _isFirstProperty ? "" : ",", name.ToCamelCase());
                _isFirstProperty = true;

                element.AcceptVisitor(this);

                _output.Append("}");
                _isFirstProperty = false;
            }

            return element;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IMetadataElementVisitor.VisitElementList<TElement, TElementImpl>(string listName, IList<TElementImpl> elementList)
        {
            if ( elementList == null || !elementList.Any(ShouldIncludeElement) )
            {
                return;
            }

            _output.AppendFormat("{0}{1}:[", _isFirstProperty ? "" : ",", listName.ToCamelCase());

            for ( int i = 0 ; i < elementList.Count ; i++ )
            {
                if ( i > 0 )
                {
                    _output.Append(",");
                }

                if ( ShouldIncludeElement(elementList[i]) )
                {
                    _output.Append("{");
                    _isFirstProperty = true;

                    elementList[i].AcceptVisitor(this);

                    _output.Append("}");
                }
            }

            _output.Append("]");
            _isFirstProperty = false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TElementImpl IMetadataElementVisitor.VisitElementReference<TElement, TElementImpl>(string name, TElementImpl element)
        {
            if ( element == null )
            {
                return null;
            }

            if ( ShouldIncludeElement(element) )
            {
                _output.AppendFormat("{0}{1}:{2}", _isFirstProperty ? "" : ",", name.ToCamelCase(), element.ReferenceName);
                _isFirstProperty = false;
            }

            return element;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IMetadataElementVisitor.VisitElementReferenceList<TElement, TElementImpl>(string listName, IList<TElementImpl> elementList)
        {
            if ( elementList == null || !elementList.Any(ShouldIncludeElement) )
            {
                return;
            }

            _output.AppendFormat("{0}{1}:[", _isFirstProperty ? "" : ",", listName.ToCamelCase());

            for ( int i = 0 ; i < elementList.Count ; i++ )
            {
                if ( i > 0 )
                {
                    _output.Append(",");
                }

                if ( ShouldIncludeElement(elementList[i]) )
                {
                    _output.Append(elementList[i].ReferenceName);
                }
            }

            _output.Append("]");
            _isFirstProperty = false;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Output
        {
            get
            {
                return _output.ToString();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ShouldIncludeElement(IMetadataElement element)
        {
            return (_elementTypeFilter == null || _elementTypeFilter.Contains(element.ElementType));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string Stringify(IMetadataElement metadata, params Type[] elementTypeFilter)
        {
            var visitor = new Jsonlike(elementTypeFilter);

            metadata.AcceptVisitor(visitor);

            return "{" + visitor.Output + "}";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private static string GetDefaultElementName<TElement>(TElement element)
            where TElement : IMetadataElement
        {
            return element.ElementType.Name.TrimPrefix("I").TrimSuffix("Metadata");
        }
    
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string GetAttributeValueString(object value)
        {
            if ( value is Type )
            {
                return ((Type)value).FriendlyName();
            }

            if ( value is PropertyInfo )
            {
                return ((PropertyInfo)value).DeclaringType.FriendlyName() + "." + ((PropertyInfo)value).Name;
            }

            return value.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool StringNeedsEscaping(string s)
        {
            for ( int i = 0 ; i < s.Length ; i++ )
            {
                if ( char.IsLetterOrDigit(s, i) )
                {
                    continue;
                }

                if ( "<>-.".IndexOf(s[i]) >= 0 )
                {
                    continue;
                }

                return true;
            }

            return false;
        }
    }
}
