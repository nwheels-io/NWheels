using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.Authorization.Impl;
using NWheels.Testing;
using Shouldly;

namespace NWheels.UnitTests.Authorization.Impl
{
    [TestFixture]
    public class EntityAccessControlTests : UnitTestBase
    {
        [Test]
        public void CanAuthroizeQuery()
        {
            //- arrange

            var metaType = Framework.MetadataCache.GetTypeMetadata(typeof(TestingSamples.IEntityOne));
            var control = new EntityAccessControl<TestingSamples.IEntityOne>(metaType);

            IEntityAccessControlBuilder<TestingSamples.IEntityOne> controlBuilder = control;
            controlBuilder
                .IsDefinedHard(canRetrieve: true)
                .IsFilteredByQuery(accessContext => entity => entity.Id > 100);

            var dataSource = new One[] {
                new One(id: 151), 
                new One(id: 50), 
                new One(id: 51), 
                new One(id: 150), 
            };

            IQueryable<TestingSamples.IEntityOne> originalQuery = dataSource
                .AsQueryable()
                .OrderBy(x => x.Id);

            //- act

            IQueryable<TestingSamples.IEntityOne> authorizedQuery =
                control.AuthorizeQuery(null, originalQuery)
                .Cast<TestingSamples.IEntityOne>();

            var results = authorizedQuery.ToArray();

            //- assert

            results.Length.ShouldBe(2);
            results[0].Id.ShouldBe(150);
            results[1].Id.ShouldBe(151);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanUseBaseTypeControlToAuthorizeDerivedTypeQuery()
        {
            //- arrange

            var baseMetaType = Framework.MetadataCache.GetTypeMetadata(typeof(TestingSamples.IEntityTwoBase));
            var baseControl = new EntityAccessControl<TestingSamples.IEntityTwoBase>(baseMetaType);

            // (!) base control can be cast to derived entity type
            IEntityAccessControl<TestingSamples.IEntityTwoDerivativeA> derivedControl = baseControl; 
            
            IEntityAccessControlBuilder<TestingSamples.IEntityTwoBase> baseControlBuilder = baseControl;
            baseControlBuilder
                .IsDefinedHard(canRetrieve: true)
                .IsFilteredByQuery(accessContext => entity => entity.Id > 100);

            var dataSource = new TwoDerivativeA[] {
                new TwoDerivativeA(id: 50, intValue: 111), 
                new TwoDerivativeA(id: 51, intValue: 222), 
                new TwoDerivativeA(id: 150, intValue: 333), 
                new TwoDerivativeA(id: 151, intValue: 444), 
            };

            IQueryable<TestingSamples.IEntityTwoDerivativeA> originalQuery = dataSource
                .AsQueryable()
                .OrderBy(x => x.Id);

            //- act

            // (!!) base control (cast to derived entity type) can be applied to query on derived type
            IQueryable<TestingSamples.IEntityTwoDerivativeA> authorizedQuery =
                derivedControl.AuthorizeQuery(null, originalQuery)
                .Cast<TestingSamples.IEntityTwoDerivativeA>();

            var results = authorizedQuery.ToArray();

            //- assert

            results.Length.ShouldBe(2);
            results[0].Id.ShouldBe(150);
            results[1].Id.ShouldBe(151);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanUseBaseTypeControlToAuthorizeBaseTypeQueryWithMixedDerivedEntities()
        {
            //- arrange

            var baseMetaType = Framework.MetadataCache.GetTypeMetadata(typeof(TestingSamples.IEntityTwoBase));
            var baseControl = new EntityAccessControl<TestingSamples.IEntityTwoBase>(baseMetaType);


            IEntityAccessControlBuilder<TestingSamples.IEntityTwoBase> baseControlBuilder = baseControl;
            baseControlBuilder
                .IsDefinedHard(canRetrieve: true)
                .IsFilteredByQuery(accessContext => entity => entity.Id > 100);

            var dataSource = new TwoBase[] {
                new TwoBase(id: 50), 
                new TwoDerivativeA(id: 51, intValue: 222), 
                new TwoBase(id: 150), 
                new TwoDerivativeA(id: 151, intValue: 444), 
            };

            IQueryable<TestingSamples.IEntityTwoBase> originalDerivedQuery = dataSource
                .AsQueryable()
                .OrderBy(x => x.Id);

            //- act

            IQueryable<TestingSamples.IEntityTwoBase> authorizedDerivedQuery =
                baseControl.AuthorizeQuery(null, originalDerivedQuery)
                .Cast<TestingSamples.IEntityTwoBase>();

            var results = authorizedDerivedQuery.ToArray();

            results.Length.ShouldBe(2);
            results[0].Id.ShouldBe(150);
            results[1].Id.ShouldBe(151);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class One : TestingSamples.IEntityOne
        {
            public One(int id)
            {
                this.Id = id;
            }

            public int Id { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TwoBase : TestingSamples.IEntityTwoBase
        {
            public TwoBase(int id)
            {
                this.Id = id;
            }

            public int Id { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TwoDerivativeA : TwoBase, TestingSamples.IEntityTwoDerivativeA
        {
            public TwoDerivativeA(int id, int intValue) : base(id)
            {
                this.IntValue = intValue;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int IntValue { get; set; }
        }
    }
}
