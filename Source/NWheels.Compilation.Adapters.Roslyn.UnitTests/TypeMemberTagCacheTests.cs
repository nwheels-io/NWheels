using FluentAssertions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests
{
    public class TypeMemberTagCacheTests
    {
        private Dictionary<int, string> _behcnmarkInstanceField;

        [ThreadStatic]
        private static Dictionary<int, string> _s_benchmarkThreadStaticField;

        //[Fact] //benchmark
        public void ThreadStaticBenchmark()
        {
            var clock1 = Stopwatch.StartNew();
            _behcnmarkInstanceField = new Dictionary<int, string>();
            for (int i = 0; i < 1000000; i++)
            {
                _behcnmarkInstanceField.TryGetValue(i, out string value);
            }
            clock1.Stop();

            var clock2 = Stopwatch.StartNew();
            _s_benchmarkThreadStaticField = new Dictionary<int, string>();
            for (int i = 0; i < 1000000; i++)
            {
                _s_benchmarkThreadStaticField.TryGetValue(i, out string value);
            }
            clock2.Stop();

            clock1.Elapsed.Should().Be(clock2.Elapsed, because: $"Instance field: [{clock1.Elapsed}], ThreadStatic field: [{clock2.Elapsed}]");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CannotUseTagCacheIfNotCreatedOnCurrentThread()
        {
            //-- arrange

            TypeMember typeInt = typeof(int);

            //-- act & assert

            Assert.Throws<InvalidOperationException>(() => {
                var tag = typeInt.SafeBackendTag();
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CannotUseTagCacheAfterItIsDisposedOnCurrentThread()
        {
            //-- arrange

            TypeMember typeInt = typeof(int);

            using (TypeMemberTagCache.CreateOnCurrentThread())
            {
            }

            //-- act & assert

            Assert.Throws<InvalidOperationException>(() => {
                var tag = typeInt.SafeBackendTag();
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CannotCreateTagCacheIfAlreadyExistsOnCurrentThread()
        {
            //-- act & assert

            using (TypeMemberTagCache.CreateOnCurrentThread())
            {
                Assert.Throws<InvalidOperationException>(() => {

                    using (TypeMemberTagCache.CreateOnCurrentThread())
                    {
                    }

                });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanUseTagCacheWhileCreatedOnCurrentThread()
        {
            //-- arrange

            TypeMember typeInt = typeof(int);
            TypeMember typeString = typeof(string);

            RoslynTypeFactoryBackend.BackendTag intTag1;
            RoslynTypeFactoryBackend.BackendTag intTag2;
            RoslynTypeFactoryBackend.BackendTag stringTag1;
            RoslynTypeFactoryBackend.BackendTag stringTag2;

            //-- act

            using (TypeMemberTagCache.CreateOnCurrentThread())
            {
                intTag1 = typeInt.SafeBackendTag();
                intTag2 = typeInt.SafeBackendTag();

                stringTag1 = typeString.SafeBackendTag();
                stringTag2 = typeString.SafeBackendTag();
            }

            //-- assert

            intTag1.Should().NotBeNull();
            intTag2.Should().BeSameAs(intTag1);

            stringTag1.Should().NotBeNull();
            stringTag2.Should().BeSameAs(stringTag1);

            intTag1.Should().NotBeSameAs(stringTag1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void BackendTagsArePerInstanceOfTagCache()
        {
            //-- arrange

            TypeMember typeInt = typeof(int);

            //-- act

            RoslynTypeFactoryBackend.BackendTag tag1;
            RoslynTypeFactoryBackend.BackendTag tag2;

            using (TypeMemberTagCache.CreateOnCurrentThread())
            {
                tag1 = typeInt.SafeBackendTag();
            }

            using (TypeMemberTagCache.CreateOnCurrentThread())
            {
                tag2 = typeInt.SafeBackendTag();
            }

            //-- assert

            tag1.Should().NotBeNull();
            tag2.Should().NotBeNull();

            tag2.Should().NotBeSameAs(tag1);
        }
    }
}
