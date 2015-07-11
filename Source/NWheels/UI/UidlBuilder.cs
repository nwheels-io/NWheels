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
using NWheels.Globalization;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI
{
    public class UidlBuilder
    {
        private readonly ITypeMetadataCache _metadataCache;
        private readonly UidlDocument _document;
        private UidlApplication _applicationBeingAdded = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlBuilder(ITypeMetadataCache metadataCache)
        {
            _metadataCache = metadataCache;
            _document = new UidlDocument();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddApplication(UidlApplication application)
        {
            _applicationBeingAdded = application;

            try
            {
                var buildable = application as IBuildableUidlNode;

                if ( buildable != null )
                {
                    InstantiateDeclaredMemberNodes(application);
                    buildable.Build(this);
                    buildable.DescribePresenter(this);
                }

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
                IdName = culture.Name,
                FullName = culture.DisplayName,
                IsRightToLeft = culture.TextInfo.IsRightToLeft,
                ListSeparator = culture.TextInfo.ListSeparator,
                Translations = translations
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string RegisterMetaType(Type type)
        {
            var typeKey = type.AssemblyQualifiedNameNonVersioned();

            if (!_document.MetaTypes.ContainsKey(typeKey))
            {
                _document.MetaTypes.Add(typeKey, CreateMetaType(type));
            }

            return typeKey;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<string> GetTranslatables()
        {
            var translatables = new HashSet<string>();

            foreach ( var application in _document.Applications )
            {
                translatables.UnionWith(application.GetTranslatables().Where(s => !string.IsNullOrEmpty(s)));
            }

            return translatables;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlDocument GetDocument()
        {
            return _document;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeMetadataCache MetadataCache
        {
            get { return _metadataCache; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void BuildNodes(params AbstractUidlNode[] nodes)
        {
            nodes.OfType<IBuildableUidlNode>().Where(node => !(node is UidlApplication)).ForEach(node => node.Build(this));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void DescribeNodePresenters(params AbstractUidlNode[] nodes)
        {
            nodes.OfType<IBuildableUidlNode>().ForEach(node => node.DescribePresenter(this));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void InstantiateDeclaredMemberNodes(AbstractUidlNode parent)
        {
            var instantiatedNodes = new List<AbstractUidlNode>();
            var memberProperties = parent.GetType().GetProperties().Where(p => IsAssignableDeclaredMemberNodeProperty(parent, p)).ToArray();

            foreach ( var property in memberProperties )
            {
                if ( property.PropertyType.IsInstanceOfType(_applicationBeingAdded) )
                {
                    property.SetValue(parent, _applicationBeingAdded);
                }
                else if ( typeof(UidlScreenPart).IsAssignableFrom(property.PropertyType) )
                {
                    property.SetValue(parent, GetOrCreateScreenPartInstance(property.PropertyType));
                }
                else
                {
                    var instance = (AbstractUidlNode)Activator.CreateInstance(property.PropertyType, property.Name, parent);
                    property.SetValue(parent, instance);
                    instantiatedNodes.Add(instance);
                    TryApplyWidgetTemplate(instance, property);
                }
            }

            AddNodesToParentCollections(parent, instantiatedNodes);
            instantiatedNodes.ForEach(InstantiateDeclaredMemberNodes);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal AbstractUidlNode[] GetDeclaredMemberNodes(AbstractUidlNode parent)
        {
            var declaredMemberNodes = parent.GetType()
                .GetProperties()
                .Where(p => IsAssignableDeclaredMemberNodeProperty(parent, p))
                .Select(p => p.GetValue(parent))
                .Cast<AbstractUidlNode>()
                .ToArray();

            return declaredMemberNodes;
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

        private static void TryApplyWidgetTemplate(AbstractUidlNode instance, PropertyInfo property)
        {
            var widgetInstance = instance as WidgetUidlNode;

            if ( widgetInstance != null )
            {
                TemplateAttribute.ApplyIfDefined(property, widgetInstance);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddNodesToParentCollections(AbstractUidlNode parent, List<AbstractUidlNode> instantiatedNodes)
        {
            var parentAsInteractive = parent as InteractiveUidlNode;
            var parentAsControlled = parent as ControlledUidlNode;

            if ( parentAsInteractive != null )
            {
                parentAsInteractive.Notifications.AddRange(instantiatedNodes.OfType<UidlNotification>());
            }

            if ( parentAsControlled != null )
            {
                parentAsControlled.Behaviors.AddRange(instantiatedNodes.OfType<BehaviorUidlNode>());
                parentAsControlled.Commands.AddRange(instantiatedNodes.OfType<UidlCommand>());
                parentAsControlled.DataBindings.AddRange(instantiatedNodes.OfType<DataBindingUidlNode>());
            }
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

        private UidlScreenPart GetOrCreateScreenPartInstance(Type screenPartType)
        {
            var existingInstance = _applicationBeingAdded.ScreenParts.FirstOrDefault(s => s.GetType() == screenPartType);

            if ( existingInstance != null )
            {
                return existingInstance;
            }

            var newInstance = (UidlScreenPart)Activator.CreateInstance(screenPartType, screenPartType.Name, _applicationBeingAdded);
            _applicationBeingAdded.ScreenParts.Add(newInstance);
            
            InstantiateDeclaredMemberNodes(newInstance);
            
            return newInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static UidlDocument GetApplicationDocument(
            UidlApplication application, 
            ITypeMetadataCache metadataCache, 
            ILocalizationProvider localizationProvider)
        {
            var builder = new UidlBuilder(metadataCache);
            builder.AddApplication(application);

            var localStrings = localizationProvider.GetLocalStrings(builder.GetTranslatables(), CultureInfo.CurrentUICulture);
            builder.AddLocale(CultureInfo.CurrentUICulture, localStrings);

            return builder.GetDocument();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool IsAssignableDeclaredMemberNodeProperty(AbstractUidlNode parent, PropertyInfo property)
        {
            return (
                property.CanWrite &&
                property.DeclaringType.IsAssignableFrom(parent.GetType()) && 
                typeof(AbstractUidlNode).IsAssignableFrom(property.PropertyType) && 
                !property.HasAttribute<ManuallyAssignedAttribute>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IBuildableUidlNode
        {
            void Build(UidlBuilder builder);
            void DescribePresenter(UidlBuilder builder);
        }
    }
}
