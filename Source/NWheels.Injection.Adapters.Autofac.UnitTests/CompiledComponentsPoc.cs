using Autofac;
using Autofac.Builder;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace NWheels.Injection.Adapters.Autofac.UnitTests
{
    public class CompiledComponentsPoc
    {
        [Fact]
        public void CanUpdateExistingContainer()
        {
            var builder = new ContainerBuilder();

            var feature1 = new GinArmyFeatureLoader();
            var feature2 = new GinInABottleFeatureLoader();
            var feature3 = new MyBottlesFeatureLoader();

            feature1.RegisterComponents(builder);
            feature2.RegisterComponents(builder);
            feature3.RegisterComponents(builder);

            var container = builder.Build();

            feature1.CompileComponents(container);
            feature2.CompileComponents(container);
            feature3.CompileComponents(container);

            var army = container.Resolve<GinArmy>();
            var ginsInTheArmy = army.Enumerate().ToArray();

            Assert.Equal(2, ginsInTheArmy.Length);
            Assert.IsType<GeneratedGinForBeerBottle>(ginsInTheArmy[0]);
            Assert.IsType<GeneratedGinForJerrycan>(ginsInTheArmy[1]);
        }

        [BottleComponent]
        private class BeerBottle { }

        [BottleComponent]
        private class Jerrycan { }

        private class MyBottlesFeatureLoader
        {
            public void RegisterComponents(ContainerBuilder builder)
            {
                // in real code, the following will be extension methods on builder:
                // builder.ContributeBottle<BeerBottle>();

                ContainerBuilderExtensions.ContributeBottle<BeerBottle>(builder);
                ContainerBuilderExtensions.ContributeBottle<Jerrycan>(builder);
            }
            public void CompileComponents(IContainer container)
            {
            }
        }

        private class GinInABottleFeatureLoader
        {
            public void RegisterComponents(ContainerBuilder builder)
            {
                builder.RegisterType<GinInABottleTypeFactory>().As<GinInABottleTypeFactory, IGinInABottleObjectFactory>().SingleInstance();
            }

            public void CompileComponents(IContainer container)
            {
                var registeredBottleTypes = GinInABottleRegistrationSource.FindRegisteredBottleTypes(container.ComponentRegistry.Registrations);
                var typeFactory = container.Resolve<GinInABottleTypeFactory>();

                foreach (var bottleType in registeredBottleTypes)
                {
                    typeFactory.GenerateGinTypeFor(bottleType);
                }

                var ginRegistrationSource = new GinInABottleRegistrationSource(typeFactory, registeredBottleTypes);
                container.ComponentRegistry.AddRegistrationSource(ginRegistrationSource);
            }
        }

        private class GinInABottleRegistrationSource : IRegistrationSource
        {
            private readonly IGinInABottleObjectFactory _ginObjectFactory;
            private readonly Type[] _bottleTypes;

            public GinInABottleRegistrationSource(
                IGinInABottleObjectFactory ginObjectFactory, 
                IEnumerable<Type> bottleTypes)
            {
                _ginObjectFactory = ginObjectFactory;
                _bottleTypes = bottleTypes.ToArray();
            }

            bool IRegistrationSource.IsAdapterForIndividualComponents => false;

            IEnumerable<IComponentRegistration> IRegistrationSource.RegistrationsFor(
                Service service, 
                Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
            {
                if (!IsServiceOfTypeIGin(service))
                {
                    return Enumerable.Empty<IComponentRegistration>();
                }

                var newRegistrations = new List<IComponentRegistration>();

                foreach (var bottleType in _bottleTypes)
                {
                    var bottleTypeCopy = bottleType;
                    newRegistrations.Add(RegistrationBuilder.ForDelegate(
                        typeof(IGin),
                        (c, p) => {
                            return _ginObjectFactory.CreateGinFor(bottleTypeCopy);
                        }
                    ).CreateRegistration());
                }

                return newRegistrations;
            }

            public static Type[] FindRegisteredBottleTypes(IEnumerable<IComponentRegistration> existingRegistrations)
            {
                return existingRegistrations
                    .SelectMany(reg => reg.Services)
                    .OfType<IServiceWithType>()
                    .Select(s => s.ServiceType)
                    .Where(t => t.GetTypeInfo().IsDefined(typeof(BottleComponentAttribute)))
                    .ToArray();
            }

            private static bool IsServiceOfTypeIGin(Service service)
            {
                return (
                    service is IServiceWithType serviceWithType && 
                    serviceWithType.ServiceType == typeof(IGin));
            }
        }

        private class GinArmyFeatureLoader
        {
            public void RegisterComponents(ContainerBuilder builder)
            {
                builder.RegisterType<GinArmy>().SingleInstance();
            }
            public void CompileComponents(IContainer container)
            {
            }
        }

        [AttributeUsage(AttributeTargets.Class)]
        private class BottleComponentAttribute : Attribute
        {
        }

        internal interface IGin
        {
            void FulfillWish(string wish);
        }

        private class GinArmy
        {
            private readonly IReadOnlyList<IGin> _allGins;

            public GinArmy(IEnumerable<IGin> allGins)
            {
                _allGins = allGins.ToArray();
            }

            public IEnumerable<IGin> Enumerate() => _allGins;
        }

        private interface IGinInABottleObjectFactory
        {
            IGin CreateGinFor(Type bottleType);
        }

        private class GinInABottleTypeFactory : IGinInABottleObjectFactory
        {
            private readonly Dictionary<Type, Func<IGin>> _ginFactoryByBottleType = new Dictionary<Type, Func<IGin>>();

            public void GenerateGinTypeFor(Type bottleType)
            {
                if (bottleType == typeof(BeerBottle))
                { 
                    _ginFactoryByBottleType[bottleType] = () => new GeneratedGinForBeerBottle();
                }
                else if (bottleType == typeof(Jerrycan))
                {
                    _ginFactoryByBottleType[bottleType] = () => new GeneratedGinForJerrycan();
                }
                else
                {
                    throw new NotSupportedException("What did you expect? this is just a test.");
                }
            }

            IGin IGinInABottleObjectFactory.CreateGinFor(Type bottleType)
            {
                var ginFactory = _ginFactoryByBottleType[bottleType];
                var newGinInstance = ginFactory();
                return newGinInstance;
            }
        }

        private class GeneratedGinForBeerBottle : IGin
        {
            public void FulfillWish(string wish)
            {
                throw new NotImplementedException();
            }
        }

        private class GeneratedGinForJerrycan : IGin
        {
            public void FulfillWish(string wish)
            {
                throw new NotImplementedException();
            }
        }

        static class ContainerBuilderExtensions
        {
            public static void ContributeBottle<TBottle>(ContainerBuilder builder)
            {
                builder.RegisterType<TBottle>().AsSelf().InstancePerDependency();
            }
        }
    }
}
