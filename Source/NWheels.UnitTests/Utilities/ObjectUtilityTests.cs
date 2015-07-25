using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Moq;
using NUnit.Framework;
using NWheels.DataObjects.Core;
using NWheels.Extensions;
using NWheels.Utilities;

namespace NWheels.UnitTests.Utilities
{
    [TestFixture]
    public class ObjectUtilityTests
    {
        [Test]
        public void CanInjectDependenciesToSingleObject()
        {
            //-- arrange

            var componentContainer = new ContainerBuilder().Build();
            var dependantMock = new Mock<IHaveDependencies>(MockBehavior.Strict);

            dependantMock.Setup(x => x.InjectDependencies(componentContainer));

            //-- act

            ObjectUtility.InjectDependenciesToObject(dependantMock.Object, componentContainer);

            //-- assert

            dependantMock.Verify(x => x.InjectDependencies(componentContainer), Times.Exactly(1));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanInjectDependenciesIntoEnumerableOfFlatObjects()
        {
            //-- arrange

            var componentContainer = new ContainerBuilder().Build();
            
            var dependantMock1 = new Mock<IHaveDependencies>(MockBehavior.Strict);
            var dependantMock2 = new Mock<IHaveDependencies>(MockBehavior.Strict);
            var dependantMock3 = new Mock<IHaveDependencies>(MockBehavior.Strict);

            dependantMock1.Setup(x => x.InjectDependencies(componentContainer));
            dependantMock2.Setup(x => x.InjectDependencies(componentContainer));

            var input = new object[] { dependantMock1.Object, dependantMock2.Object, dependantMock3.Object }; 

            //-- act

            var output = input.InjectDependenciesFrom(componentContainer).Take(2).ToArray();

            //-- assert

            Assert.That(output[0], Is.SameAs(input[0]));
            Assert.That(output[1], Is.SameAs(input[1]));

            dependantMock1.Verify(x => x.InjectDependencies(componentContainer), Times.Exactly(1));
            dependantMock2.Verify(x => x.InjectDependencies(componentContainer), Times.Exactly(1));
            dependantMock3.Verify(x => x.InjectDependencies(It.IsAny<IComponentContext>()), Times.Never);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Test]
        public void CanInjectDependenciesToCompositeObject()
        {
            //-- arrange

            var componentContainer = new ContainerBuilder().Build();
            var parent = new Mock<IHaveDependencies>(MockBehavior.Strict);
            var child1 = new Mock<IHaveDependencies>(MockBehavior.Strict);
            var child2 = new Mock<IHaveDependencies>(MockBehavior.Strict);

            parent.Setup(x => x.InjectDependencies(componentContainer));
            parent.As<IHaveNestedObjects>()
                .Setup(x => x.DeepListNestedObjects(It.IsAny<HashSet<object>>()))
                .Callback((HashSet<object> list) => list.UnionWith(new[] { child1.Object, child2.Object }));

            child1.Setup(x => x.InjectDependencies(componentContainer));
            child2.Setup(x => x.InjectDependencies(componentContainer));

            //-- act

            ObjectUtility.InjectDependenciesToObject(parent.Object, componentContainer);

            //-- assert

            parent.Verify(x => x.InjectDependencies(componentContainer), Times.Exactly(1));
            child1.Verify(x => x.InjectDependencies(componentContainer), Times.Exactly(1));
            child2.Verify(x => x.InjectDependencies(componentContainer), Times.Exactly(1));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

    }
}
