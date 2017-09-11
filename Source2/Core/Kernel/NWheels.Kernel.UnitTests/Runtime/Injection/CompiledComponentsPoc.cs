using Autofac;
using Autofac.Builder;
using Autofac.Core;
using NWheels.Testability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace NWheels.Kernel.UnitTests.Runtime.Injection
{
    public class CompiledComponentsPoc : TestBase.UnitTest
    {
        [Fact]
        public void CanUpdateExistingContainer()
        {
            var builder = new ContainerBuilder();

            // a feature provided by the Jinn Framework
            // Jinn Army gathers all existing Jinns under one roof
            var feature1 = new JinnArmyFeatureLoader();       

            // a feature provided by the Bottle Framework
            // with this feature, every Bottle will have a generated Jinn inside it
            var feature2 = new JinnInABottleFeatureLoader();

            // a feature provided by application
            // contributes some application-specific Bottles
            var feature3 = new MyBottlesFeatureLoader();

            // a feature provided by application
            // contributes some free Jinns (not sitting inside Bottles)
            var feature4 = new MyFreeJinnsFeatureLoader();  

            // STEP 1. Let all features register components in container builder
            feature1.RegisterComponents(builder);
            feature2.RegisterComponents(builder);
            feature3.RegisterComponents(builder);
            feature4.RegisterComponents(builder);

            // STEP 2. Create container instance from the builder
            var container = builder.Build();

            // STEP 3. Based on already registered components, generate layers of derivative components
            // (in this example, a Jinn component will be generated for every registered Bottle).
            // Register the generated components, so that other components can depend on them.
            feature1.CompileComponents(container);
            feature2.CompileComponents(container);
            feature3.CompileComponents(container);
            feature4.CompileComponents(container);

            // STEP 4. Resolve JinnArmy, and verify that it received Jinns generated in step 3, from the container.
            var army = container.Resolve<JinnArmy>();
            var jinnsInTheArmy = army.Enumerate().ToArray();

            Assert.Equal(4, jinnsInTheArmy.Length);

            // verify that the container delivers generated and manually registered Jinns together
            Assert.Equal(1, jinnsInTheArmy.Count(j => j is GeneratedJinnForBeerBottle));
            Assert.Equal(1, jinnsInTheArmy.Count(j => j is GeneratedJinnForJerrycan));
            Assert.Equal(1, jinnsInTheArmy.Count(j => j is FreeJinn && j.Name == "Jonny"));
            Assert.Equal(1, jinnsInTheArmy.Count(j => j is FreeJinn && j.Name == "Tommy"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Application feature that contributes some application-specific bottles

        [BottleComponent]
        private class BeerBottle { }

        [BottleComponent]
        private class Jerrycan { }

        private class MyBottlesFeatureLoader
        {
            public void RegisterComponents(ContainerBuilder builder)
            {
                // in real code, extension method syntax will be used:
                // builder.ContributeBottle<BeerBottle>();

                BottleContainerBuilderExtensions.ContributeBottle<BeerBottle>(builder);
                BottleContainerBuilderExtensions.ContributeBottle<Jerrycan>(builder);
            }
            public void CompileComponents(IContainer container)
            {
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Application feature that contributes some free Jinns, not related to any bottles

        private class MyFreeJinnsFeatureLoader
        {
            public void RegisterComponents(ContainerBuilder builder)
            {
                // in real code, extension method syntax will be used:
                // builder.ContributeFreeJinn("Jonny");

                JinnContainerBuilderExtensions.ContributeFreeJinn(builder, "Jonny");
                JinnContainerBuilderExtensions.ContributeFreeJinn(builder, "Tommy");
            }
            public void CompileComponents(IContainer container)
            {
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region The Bottle Framework module

        [AttributeUsage(AttributeTargets.Class)]
        private class BottleComponentAttribute : Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static class BottleContainerBuilderExtensions
        {
            public static void ContributeBottle<TBottle>(ContainerBuilder builder)
            {
                builder.RegisterType<TBottle>().AsSelf().InstancePerDependency();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private interface IJinnInABottleObjectFactory
        {
            IJinn CreateJinnFor(Type bottleType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class JinnInABottleTypeFactory : IJinnInABottleObjectFactory
        {
            private readonly Dictionary<Type, Func<IJinn>> _jinnActivatorByBottleType = new Dictionary<Type, Func<IJinn>>();

            public void GenerateJinnTypeFor(Type bottleType)
            {
                if (bottleType == typeof(BeerBottle))
                {
                    _jinnActivatorByBottleType[bottleType] = () => new GeneratedJinnForBeerBottle();
                }
                else if (bottleType == typeof(Jerrycan))
                {
                    _jinnActivatorByBottleType[bottleType] = () => new GeneratedJinnForJerrycan();
                }
                else
                {
                    throw new NotSupportedException("What did you expect? this is just a test.");
                }
            }

            IJinn IJinnInABottleObjectFactory.CreateJinnFor(Type bottleType)
            {
                var activator = _jinnActivatorByBottleType[bottleType];
                var newJinnInstance = activator();
                return newJinnInstance;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class JinnInABottleFeatureLoader
        {
            public void RegisterComponents(ContainerBuilder builder)
            {
                builder.RegisterType<JinnInABottleTypeFactory>().As<JinnInABottleTypeFactory, IJinnInABottleObjectFactory>().SingleInstance();
            }

            public void CompileComponents(IContainer container)
            {
                var registeredBottleTypes = JinnInABottleRegistrationSource.FindRegisteredBottleTypes(container.ComponentRegistry.Registrations);
                var typeFactory = container.Resolve<JinnInABottleTypeFactory>();

                foreach (var bottleType in registeredBottleTypes)
                {
                    typeFactory.GenerateJinnTypeFor(bottleType);
                }

                var jinnInABottleRegistrations = new JinnInABottleRegistrationSource(typeFactory, registeredBottleTypes);
                container.ComponentRegistry.AddRegistrationSource(jinnInABottleRegistrations);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //TODO: implementing registration source is quite a bit of work!
        //      we should see how we offload it from the code of every concrete feature loader
        private class JinnInABottleRegistrationSource : IRegistrationSource
        {
            private readonly IJinnInABottleObjectFactory _jinnObjectFactory;
            private readonly Type[] _bottleTypes;

            public JinnInABottleRegistrationSource(
                IJinnInABottleObjectFactory jinnObjectFactory, 
                IEnumerable<Type> bottleTypes)
            {
                _jinnObjectFactory = jinnObjectFactory;
                _bottleTypes = bottleTypes.ToArray();
            }

            bool IRegistrationSource.IsAdapterForIndividualComponents => false;

            IEnumerable<IComponentRegistration> IRegistrationSource.RegistrationsFor(
                Service service, 
                Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
            {
                if (!IsServiceOfTypeIJinn(service))
                {
                    return Enumerable.Empty<IComponentRegistration>();
                }

                var newRegistrations = new List<IComponentRegistration>();

                foreach (var bottleType in _bottleTypes)
                {
                    var bottleTypeCopy = bottleType;
                    newRegistrations.Add(RegistrationBuilder.ForDelegate(
                        typeof(IJinn),
                        (c, p) => {
                            return _jinnObjectFactory.CreateJinnFor(bottleTypeCopy);
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

            private static bool IsServiceOfTypeIJinn(Service service)
            {
                return (
                    service is IServiceWithType serviceWithType && 
                    serviceWithType.ServiceType == typeof(IJinn));
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region The Jinn Framework module

        private interface IJinn
        {
            void FulfillWish(string wish);
            string Name { get; }
        }

        private class JinnArmy
        {
            private readonly IReadOnlyList<IJinn> _allJinns;

            public JinnArmy(IEnumerable<IJinn> allJinns)
            {
                _allJinns = allJinns.ToArray();
            }

            public IEnumerable<IJinn> Enumerate() => _allJinns;
        }

        static class JinnContainerBuilderExtensions
        {
            public static void ContributeFreeJinn(ContainerBuilder builder, string jinnName)
            {
                builder.RegisterType<FreeJinn>()
                    .As<IJinn>()
                    .WithParameter(TypedParameter.From(jinnName))
                    .InstancePerDependency();
            }
        }

        private class FreeJinn : IJinn
        {
            public FreeJinn(string name)
            {
                this.Name = name;
            }
            public void FulfillWish(string wish)
            {
            }
            public string Name { get; }
        }

        private class JinnArmyFeatureLoader
        {
            public void RegisterComponents(ContainerBuilder builder)
            {
                builder.RegisterType<JinnArmy>().SingleInstance();
            }
            public void CompileComponents(IContainer container)
            {
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Generated components

        private class GeneratedJinnForBeerBottle : IJinn
        {
            public void FulfillWish(string wish)
            {
            }
            public string Name => "Beer Bottle Jinn";
        }

        private class GeneratedJinnForJerrycan : IJinn
        {
            public void FulfillWish(string wish)
            {
            }
            public string Name => "Jerrycan Jinn";
        }

        #endregion
    }
}
