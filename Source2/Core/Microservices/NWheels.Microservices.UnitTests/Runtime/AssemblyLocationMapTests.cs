using FluentAssertions;
using NWheels.Microservices;
using NWheels.Microservices.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace NWheels.Microservices.UnitTests.Runtime
{
    public class AssemblyLocationMapTests
    {
        [Fact]
        public void CanAddAssemblyDirectories()
        {
            //- arrange

            var mapUnderTest = new AssemblyLocationMap();

            //- act

            mapUnderTest.AddDirectory(@"C:\First");
            mapUnderTest.AddDirectory(@"C:\Second");

            //- assert

            mapUnderTest.Directories.Count().Should().Be(2);
            mapUnderTest.Directories.Should().ContainInOrder(new[] { @"C:\First", @"C:\Second" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanAddAssemblyLocations()
        {
            //- arrange

            var mapUnderTest = new AssemblyLocationMap();

            //- act

            mapUnderTest.AddLocations(new Dictionary<string, string> {
                ["Assembly1.dll"] = @"C:\First\Assembly1.dll",
                ["Assembly2.dll"] = @"C:\Second\Assembly2.dll"
            });
            mapUnderTest.AddLocations(new Dictionary<string, string> {
                ["Assembly3.dll"] = @"C:\Third\Assembly3.dll"
            });

            //- assert

            mapUnderTest.FilePathByAssemblyName.Count.Should().Be(3);
            mapUnderTest.FilePathByAssemblyName["Assembly1.dll"].Should().Be(@"C:\First\Assembly1.dll");
            mapUnderTest.FilePathByAssemblyName["Assembly2.dll"].Should().Be(@"C:\Second\Assembly2.dll");
            mapUnderTest.FilePathByAssemblyName["Assembly3.dll"].Should().Be(@"C:\Third\Assembly3.dll");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanIgnoreDuplicateAssemblyMappings()
        {
            //- arrange

            var mapUnderTest = new AssemblyLocationMap();

            //- act

            mapUnderTest.AddLocations(new Dictionary<string, string> {
                ["Assembly1.dll"] = @"C:\First\Assembly1.dll",
                ["Assembly2.dll"] = @"C:\Second\Assembly2.dll"
            });
            mapUnderTest.AddLocations(new Dictionary<string, string> {
                ["Assembly1.dll"] = @"Z:\Zzz\Zzz.dll",
                ["Assembly3.dll"] = @"C:\Third\Assembly3.dll"
            });

            //- assert

            mapUnderTest.FilePathByAssemblyName.Count.Should().Be(3);
            mapUnderTest.FilePathByAssemblyName["Assembly1.dll"].Should().Be(@"C:\First\Assembly1.dll");
            mapUnderTest.FilePathByAssemblyName["Assembly2.dll"].Should().Be(@"C:\Second\Assembly2.dll");
            mapUnderTest.FilePathByAssemblyName["Assembly3.dll"].Should().Be(@"C:\Third\Assembly3.dll");
        }
    }
}
