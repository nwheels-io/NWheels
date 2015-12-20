using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using NWheels.Conventions.Core;
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
        private readonly IDomainObjectFactory _domainObjectFactory;
        private readonly IEntityObjectFactory _entityObjectFactory;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly UidlDocument _document;
        private UidlApplication _applicationBeingAdded = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlBuilder(ITypeMetadataCache metadataCache, IDomainObjectFactory domainObjectFactory, IEntityObjectFactory entityObjectFactory)
        {
            _metadataCache = metadataCache;
            _domainObjectFactory = domainObjectFactory;
            _entityObjectFactory = entityObjectFactory;
            _document = new UidlDocument();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddApplication(UidlApplication application)
        {
            _applicationBeingAdded = application;
            _applicationBeingAdded.MetadataCache = _metadataCache;

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
            Type collectionItemType;
            if ( type.IsCollectionType(out collectionItemType) )
            {
                return RegisterMetaType(collectionItemType);
            }

            var allTypesToAdd = new HashSet<Type>();
            allTypesToAdd.Add(type);

            while ( allTypesToAdd.Count > 0 )
            {
                var typeToAdd = allTypesToAdd.First();
                var typeKey = MakeTypeKey(type);

                if ( !_document.MetaTypes.ContainsKey(typeKey) )
                {
                    _document.MetaTypes.Add(typeKey, CreateMetaType(typeToAdd, allTypesToAdd));
                }

                allTypesToAdd.Remove(typeToAdd);
            }

            return type.AssemblyQualifiedNameNonVersioned();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BuildManuallyInstantiatedNodes(params AbstractUidlNode[] nodes)
        {
            foreach ( var node in nodes.Where(n => n != null) )
            {
                InstantiateDeclaredMemberNodes(node);
            }

            BuildNodes(nodes);
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
                    property.SetValue(parent, GetOrCreateScreenPartInstance(property));
                }
                else
                {
                    var instance = (AbstractUidlNode)Activator.CreateInstance(property.PropertyType, property.Name, parent);
                    instance.MetadataCache = _metadataCache;
                    
                    property.SetValue(parent, instance);
                    instantiatedNodes.Add(instance);
                    TryApplyWidgetTemplate(instance, property);
                    parent.OnDeclaredMemberNodeCreated(property, instance);
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

        private string MakeTypeKey(Type type)
        {
            if ( DataObjectContractAttribute.IsDataObjectContract(type) || DataObjectPartContractAttribute.IsDataObjectPartContract(type) )
            {
                ITypeMetadata metaType = _metadataCache.GetTypeMetadata(type);
                return metaType.QualifiedName;
            }
            else
            {
                return type.AssemblyQualifiedNameNonVersioned();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private UidlMetaType CreateMetaType(Type type, HashSet<Type> relatedTypes)
        {
            UidlMetaType metaType;

            if ( DataObjectContractAttribute.IsDataObjectContract(type) || DataObjectPartContractAttribute.IsDataObjectPartContract(type) )
            {
                var domainObjectType = GetDomainObjectType(type);
                metaType = new UidlObjectMetaType(type, domainObjectType, _metadataCache.GetTypeMetadata(type), relatedTypes);
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

        private Type GetDomainObjectType(Type contractType)
        {
            var metaType = _metadataCache.GetTypeMetadata(contractType);

            if ( metaType.IsAbstract )
            {
                return contractType;
            }

            Type persistableObjectType;
            
            if ( metaType.TryGetImplementation(_entityObjectFactory.GetType(), out persistableObjectType) )
            {
                return _domainObjectFactory.GetOrBuildDomainObjectType(contractType);
            }
            else
            {
                var allImplementations = metaType.GetAllImplementations();
                return (allImplementations.Any() ? allImplementations.First().Value : null);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private UidlScreenPart GetOrCreateScreenPartInstance(PropertyInfo declaration)
        {
            var screenPartType = declaration.PropertyType;
            
            //var existingInstance = _applicationBeingAdded.ScreenParts.FirstOrDefault(s => s.GetType() == screenPartType);

            //if ( existingInstance != null )
            //{
            //    return existingInstance;
            //}

            var newInstance = (UidlScreenPart)Activator.CreateInstance(screenPartType, declaration.Name, _applicationBeingAdded);
            newInstance.MetadataCache = _metadataCache;
            _applicationBeingAdded.ScreenParts.Add(newInstance);
            
            InstantiateDeclaredMemberNodes(newInstance);
            
            return newInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static UidlDocument GetApplicationDocument(
            UidlApplication application, 
            ITypeMetadataCache metadataCache, 
            ILocalizationProvider localizationProvider,
            IComponentContext components)
        {
            var builder = new UidlBuilder(metadataCache, components.Resolve<IDomainObjectFactory>(), components.Resolve<IEntityObjectFactory>());
            builder.AddApplication(application);

            var localStrings = localizationProvider.GetCurrentLocale().GetAllLocalStrings(builder.GetTranslatables());
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

        private static bool IsManuallyAssignedDeclaredMemberNodeProperty(AbstractUidlNode parent, PropertyInfo property)
        {
            return (
                property.CanWrite &&
                property.DeclaringType.IsAssignableFrom(parent.GetType()) &&
                typeof(AbstractUidlNode).IsAssignableFrom(property.PropertyType) &&
                property.HasAttribute<ManuallyAssignedAttribute>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IBuildableUidlNode
        {
            void Build(UidlBuilder builder);
            void DescribePresenter(UidlBuilder builder);
        }
    }
}
