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
using NWheels.Globalization.Core;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI
{
    public class UidlBuilder
    {
        private readonly IDomainObjectFactory _domainObjectFactory;
        private readonly IEntityObjectFactory _entityObjectFactory;
        private readonly UidlExtensionRegistration[] _registeredExtensions;
        private readonly IComponentContext _components;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly UidlDocument _document;
        private readonly HashSet<LocaleEntryKey> _translatables;
        private readonly List<Action> _deferredInitializers;
        private UidlApplication _applicationBeingAdded = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlBuilder(
            IComponentContext components,
            ITypeMetadataCache metadataCache, 
            IDomainObjectFactory domainObjectFactory, 
            IEntityObjectFactory entityObjectFactory,
            IEnumerable<UidlExtensionRegistration> registeredExtensions)
        {
            _components = components;
            _metadataCache = metadataCache;
            _domainObjectFactory = domainObjectFactory;
            _entityObjectFactory = entityObjectFactory;
            _registeredExtensions = registeredExtensions.ToArray();
            _document = new UidlDocument();
            _translatables = new HashSet<LocaleEntryKey>();
            _deferredInitializers = new List<Action>();
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

                    foreach (var initializer in _deferredInitializers)
                    {
                        initializer();
                    }
                }

                _translatables.UnionWith(application.GetTranslatables());
                _document.Applications.Add(application);
            }
            finally
            {
                _applicationBeingAdded = null;
                _deferredInitializers.Clear();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddLocales(IEnumerable<ILocale> locales)
        {
            foreach (var locale in locales)
            {
                AddLocale(locale, _translatables);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddLocale(ILocale locale, IEnumerable<LocaleEntryKey> translatables)
        {
            var culture = locale.Culture;
            var translations = locale.As<ICoreLocale>().GetAllTranslations(translatables, includeOriginFallbacks: true);

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

        public IEnumerable<LocaleEntryKey> GetTranslatables()
        {
            return _translatables;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlDocument GetDocument()
        {
            if (_document.GetTranslatables() == null)
            {
                _document.SetTranslatables(_translatables);
            }

            return _document;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentContext Components
        {
            get { return _components; }
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

        internal void AddDeferredInitializer(Action initializer)
        {
            _deferredInitializers.Add(initializer);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void InstantiateDeclaredMemberNodes(AbstractUidlNode parent)
        {
            InstantiateDeclaredMemberNodes(parent, declaringTarget: parent);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal IEnumerable<AbstractUidlNode> InstantiateDeclaredMemberNodes(AbstractUidlNode parent, object declaringTarget)
        {
            var instantiatedNodes = new List<AbstractUidlNode>();
            var memberProperties = declaringTarget.GetType().GetProperties().Where(p => IsAssignableDeclaredMemberNodeProperty(declaringTarget, p)).ToArray();

            foreach (var property in memberProperties)
            {
                if (property.PropertyType.IsInstanceOfType(_applicationBeingAdded))
                {
                    property.SetValue(declaringTarget, _applicationBeingAdded);
                }
                else if (typeof(UidlScreenPart).IsAssignableFrom(property.PropertyType))
                {
                    var screenPart = GetOrCreateScreenPartInstance(property);
                    property.SetValue(declaringTarget, screenPart);
                    AttachExtensions(screenPart);
                }
                else
                {
                    var instance = (AbstractUidlNode)Activator.CreateInstance(property.PropertyType, property.Name, parent);
                    instance.MetadataCache = _metadataCache;

                    property.SetValue(declaringTarget, instance);
                    instantiatedNodes.Add(instance);
                    TryApplyWidgetTemplate(instance, property);
                    parent.OnDeclaredMemberNodeCreated(property, instance);
                    AttachExtensions(instance);
                }
            }

            AddNodesToParentCollections(parent, instantiatedNodes);
            instantiatedNodes.ForEach(InstantiateDeclaredMemberNodes);

            return instantiatedNodes;
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
                parentAsControlled.Commands.AddRange(instantiatedNodes.OfType<UidlCommandBase>());
                parentAsControlled.DataBindings.AddRange(instantiatedNodes.OfType<DataBindingUidlNode>());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AttachExtensions(AbstractUidlNode node)
        {
            var controlledNode = (node as ControlledUidlNode);

            if (controlledNode != null)
            {
                controlledNode.AttachExtensions(_registeredExtensions);
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
                return _domainObjectFactory.GetOrBuildDomainObjectType(contractType, _entityObjectFactory.GetType());
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

        private string GetLocaleEntryValue(LocaleEntryKey entry)
        {
            //TODO: include entry origin as a prefix or a suffix
            return entry.StringId;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static UidlDocument GetApplicationDocument(
            UidlApplication application, 
            ITypeMetadataCache metadataCache, 
            ILocalizationProvider localizationProvider,
            IComponentContext components)
        {
            var builder = new UidlBuilder(
                components,
                metadataCache, 
                components.Resolve<IDomainObjectFactory>(), 
                components.Resolve<IEntityObjectFactory>(),
                components.Resolve<IEnumerable<UidlExtensionRegistration>>());
            
            builder.AddApplication(application);
            builder.AddLocales(localizationProvider.GetAllSupportedLocales());

            return builder.GetDocument();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool IsAssignableDeclaredMemberNodeProperty(object target, PropertyInfo property)
        {
            return (
                property.CanWrite &&
                property.DeclaringType.IsAssignableFrom(target.GetType()) && 
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
            void AttachExtensions(UidlExtensionRegistration[] registeredExtensions);
        }
    }
}
