using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using NWheels.Client;
using NWheels.Core;
using NWheels.DataObjects;
using NWheels.Extensions;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Serialization;
using NWheels.Serialization.Factories;
using NWheels.Testing;
using NWheels.UI.Factories;
using Shouldly;
using Repo = NWheels.UnitTests.Serialization.TestObjectRepository;

namespace NWheels.UnitTests.Serialization
{
    [TestFixture]
    public class MetaTypedObjectExtensionTests : UnitTestBase
    {
        [Test]
        public void Roundtrip_Entity_Flat()
        {
            //-- arrange

            var serverSerializer = new CompactSerializer(
                Resolve<IComponentContext>(),
                Resolve<ITypeMetadataCache>(),
                Resolve<CompactSerializerFactory>(),
                new ICompactSerializerExtension[] {
                    new MetaTypedObjectExtension(
                        Framework.As<ICoreFramework>(), 
                        Resolve<ITypeMetadataCache>(), 
                        Resolve<IViewModelObjectFactory>()), 
                });

            var serverDictionary = new StaticCompactSerializerDictionary();
            serverDictionary.RegisterType(typeof(Repo.IEntityA));
            serverDictionary.MakeImmutable();

            var clientFramework = ClientSideFramework.CreateWithDefaultConfiguration(registerComponents: builder => {
                builder.RegisterInstance(new TestFixtureWithNodeHosts.ConsolePlainLog("TEST", LogLevel.Debug, Stopwatch.StartNew())).As<IPlainLog>();
            });
            clientFramework.NewDomainObject<Repo.IEntityA>();
            var clientSerializer = new CompactSerializer(
                clientFramework.Components.Resolve<IComponentContext>(),
                clientFramework.Components.Resolve<ITypeMetadataCache>(),
                clientFramework.Components.Resolve<CompactSerializerFactory>(),
                new ICompactSerializerExtension[] {
                    new MetaTypedObjectExtension(
                        clientFramework.As<ICoreFramework>(), 
                        clientFramework.Components.Resolve<ITypeMetadataCache>(), 
                        clientFramework.Components.Resolve<IViewModelObjectFactory>()), 
                });

            var clientDictionary = new StaticCompactSerializerDictionary();
            clientDictionary.RegisterType(typeof(Repo.IEntityA));
            clientDictionary.MakeImmutable();

            var originalOnServer = Framework.NewDomainObject<Repo.IEntityA>();
            originalOnServer.Id = 123;
            originalOnServer.Name = "ABC";

            //-- act

            var serializedBytesOnServer = serverSerializer.GetBytes(originalOnServer, serverDictionary);
            var deserializedOnClient = clientSerializer.GetObject<Repo.IEntityA>(serializedBytesOnServer, clientDictionary);
            var serializedBytesOnClient = clientSerializer.GetBytes(deserializedOnClient, clientDictionary);
            var deserializedOnServer = serverSerializer.GetObject<Repo.IEntityA>(serializedBytesOnClient, serverDictionary);

            //-- assert

            deserializedOnClient.ShouldNotBeNull();
            deserializedOnClient.Id.ShouldBe(123);
            deserializedOnClient.Name.ShouldBe("ABC");

            deserializedOnServer.ShouldNotBeNull();
            deserializedOnServer.Id.ShouldBe(123);
            deserializedOnServer.Name.ShouldBe("ABC");
        }
    }
}
