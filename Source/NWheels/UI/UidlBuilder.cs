using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.UI.Uidl;

namespace NWheels.UI
{
    public class UidlBuilder
    {
        private readonly ITypeMetadataCache _metadataCache;
        private readonly UidlDocument _document;
        private readonly HashSet<string> _translatables;
        private UidlApplication _applicationBeingAdded = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlBuilder(ITypeMetadataCache metadataCache)
        {
            _metadataCache = metadataCache;
            _document = new UidlDocument();
            _translatables = new HashSet<string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddApplication(UidlApplication application)
        {
            _applicationBeingAdded = application;

            try
            {
                ((IBuildableUidlNode)application).Build(this);
                _document.Applications.Add(application);
            }
            finally
            {
                _applicationBeingAdded = null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddLocale(CultureInfo culture, Dictionary<string, string> translations)
        {
            _document.Locales.Add(culture.Name, new UidlLocale() {
                CultureName = culture.Name,
                Translations = translations
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<string> GetTranslatables()
        {
            return _translatables;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlDocument GetDocument()
        {
            return _document;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void BuildNodes(params AbstractUidlNode[] nodes)
        {
            nodes.OfType<IBuildableUidlNode>().ForEach(node => node.Build(this));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal AbstractUidlNode[] InstantiateDeclaredMemberNodes(ControlledUidlNode parent)
        {
            var instantiatedNodes = new List<AbstractUidlNode>();
            var memberProperties = parent.GetType().GetProperties().Where(p => IsDeclaredMemberNodeProperty(parent, p)).ToArray();

            foreach ( var property in memberProperties )
            {
                if ( property.PropertyType.IsInstanceOfType(_applicationBeingAdded) )
                {
                    property.SetValue(parent, _applicationBeingAdded);
                }
                else
                {
                    var instance = (AbstractUidlNode)Activator.CreateInstance(property.PropertyType, property.Name, parent);
                    property.SetValue(parent, instance);
                    instantiatedNodes.Add(instance);
                }
            }

            parent.Behaviors.AddRange(instantiatedNodes.OfType<BehaviorUidlNode>());
            parent.Commands.AddRange(instantiatedNodes.OfType<UidlCommand>());
            parent.DataBindings.AddRange(instantiatedNodes.OfType<DataBindingUidlNode>());
            parent.Notifications.AddRange(instantiatedNodes.OfType<UidlNotification>());

            _translatables.UnionWith(parent.Translatables);

            return instantiatedNodes.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal string RegisterMetaType(Type type)
        {
            var typeKey = type.AssemblyQualifiedNameNonVersioned();

            if ( !_document.MetaTypes.ContainsKey(typeKey) )
            {
                _document.MetaTypes.Add(typeKey, CreateMetaType(type));
            }

            return typeKey;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal string RegisterUserAlert(MethodInfo alertMethod)
        {
            var alert = new UidlUserAlert(alertMethod, _applicationBeingAdded);

            if ( !_applicationBeingAdded.UserAlerts.ContainsKey(alert.QualifiedName) )
            {
                _applicationBeingAdded.UserAlerts.Add(alert.QualifiedName, alert);
            }

            return alert.QualifiedName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private UidlMetaType CreateMetaType(Type type)
        {
            UidlMetaType metaType;

            if ( DataObjectContractAttribute.IsDataObjectContract(type) )
            {
                metaType = new UidlObjectMetaType(type, _metadataCache.GetTypeMetadata(type));
            }
            else if (typeof(IEntityId).IsAssignableFrom(type))
            {
                metaType = new UidlKeyMetaType(type);
            }
            else
            {
                metaType = new UidlValueMetaType(type);
            }

            return metaType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool IsDeclaredMemberNodeProperty(AbstractUidlNode parent, PropertyInfo property)
        {
            return (
                property.DeclaringType == parent.GetType() && 
                typeof(AbstractUidlNode).IsAssignableFrom(property.PropertyType));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IBuildableUidlNode
        {
            void Build(UidlBuilder builder);
        }
    }
}
