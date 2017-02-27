using NWheels.Orchestration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit;

namespace NWheels.Implementation.UnitTests.Microservices.Orchestration
{
    public class RevertableSequenceTests
    {
        [Fact]
        public void NewInstance_State_NotPerformed()
        {
            //-- Arrange

            var codeBehind = new TestCodeBehind();

            //-- Act

            var sequence = new RevertableSequence(codeBehind);

            //-- Assert

            Assert.Equal(sequence.State, RevertableSequenceState.NotPerformed);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void OnceSteps_PerformSucceeded_StatePerformed()
        {
            //-- Arrange

            var codeBehind = new TestCodeBehind();
            var sequence = new RevertableSequence(codeBehind);

            //-- Act

            sequence.Perform();

            //-- Assert

            Assert.Equal(sequence.State, RevertableSequenceState.Performed);
            Assert.Equal(codeBehind.TakeLog(), new[] {
                "PerformOne()", "PerformTwo()", "PerformThree()"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void OnceSteps_Revert_ReverseOrder()
        {
            //-- Arrange

            var codeBehind = new TestCodeBehind();
            var sequence = new RevertableSequence(codeBehind);

            sequence.Perform();

            //-- Act

            var log1 = codeBehind.TakeLog();

            sequence.Revert();

            var log2 = codeBehind.TakeLog();

            //-- Assert

            Assert.Equal(sequence.State, RevertableSequenceState.Reverted);
            Assert.Equal(log1, new[] {
                "PerformOne()", "PerformTwo()", "PerformThree()"
            });
            Assert.Equal(log2, new[] {
                "RevertFour()", "RevertTwo()", "RevertOne()"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void OnceSteps_PerformFailed_Reverted()
        {
            //-- Arrange

            var codeBehind = new TestCodeBehind()
            {
                ThrowFromPerformThree = true
            };
            var sequence = new RevertableSequence(codeBehind);

            //-- Act

            var exception = Assert.Throws<TestSequenceException>(() => {
                sequence.Perform();
            });

            //-- Assert

            Assert.Equal(exception.Message, "PerformThree()");
            Assert.Equal(sequence.State, RevertableSequenceState.Reverted);
            Assert.Equal(codeBehind.TakeLog(), new[] {
                "PerformOne()", "PerformTwo()", "THROWING-FROM:PerformThree()", "RevertTwo()", "RevertOne()"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void OnceSteps_PerformFailedThenRevertFailed_AggregateExceptionThrown()
        {
            //-- Arrange

            var codeBehind = new TestCodeBehind()
            {
                ThrowFromPerformThree = true,
                ThrowFromRevertOne = true
            };
            var sequence = new RevertableSequence(codeBehind);

            //-- Act

            var exception = Assert.Throws<AggregateException>(() => {
                sequence.Perform();
            });
               
            //-- Assert

            Assert.Equal(sequence.State, RevertableSequenceState.RevertFailed);
            Assert.Equal(codeBehind.TakeLog(), new[] {
                "PerformOne()", "PerformTwo()", "THROWING-FROM:PerformThree()", "RevertTwo()", "THROWING-FROM:RevertOne()"
            });

            Assert.True(exception.InnerExceptions.All(e => e is TestSequenceException), "Exception type");
            Assert.Equal(exception.InnerExceptions.Select(e => e.Message), new[] {
                "PerformThree()", "RevertOne()"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void OnceSteps_RevertFailed_AggregateExceptionThrown()
        {
            //-- Arrange

            var codeBehind = new TestCodeBehind()
            {
                ThrowFromRevertFour = true,
                ThrowFromRevertTwo = true
            };
            var sequence = new RevertableSequence(codeBehind);

            sequence.Perform();
            codeBehind.TakeLog();

            //-- Act
            
            var exception = Assert.Throws<AggregateException>(() => {
                sequence.Revert();
            });
            
            //-- Assert

            Assert.Equal(sequence.State, RevertableSequenceState.RevertFailed);
            Assert.Equal(codeBehind.TakeLog(), new[] {
                "THROWING-FROM:RevertFour()", "THROWING-FROM:RevertTwo()", "RevertOne()"
            });

            Assert.True(exception.InnerExceptions.All(e => e is TestSequenceException), "Exception type");
            Assert.Equal(exception.InnerExceptions.Select(e => e.Message), new[] {
                "RevertFour()", "RevertTwo()"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void OnceSteps_RevertFailed_RetryRevertAndSucceed()
        {
            //-- Arrange

            var codeBehind = new TestCodeBehind()
            {
                ThrowFromRevertFour = true,
                ThrowFromRevertTwo = true
            };
            var sequence = new RevertableSequence(codeBehind);

            sequence.Perform();

            var exception = Assert.Throws<AggregateException>(() => {
                sequence.Revert();
            });
            
            codeBehind.TakeLog();

            codeBehind.ThrowFromRevertFour = false;
            codeBehind.ThrowFromRevertTwo = false;

            //-- Act

            sequence.Revert();

            //-- Assert

            Assert.Equal(sequence.State, RevertableSequenceState.Reverted);
            Assert.Equal(codeBehind.TakeLog(), new[] {
                "RevertFour()", "RevertTwo()"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CollectionSteps_PerformSucceeded_CollectionItemsPerformed()
        {
            //-- Arrange

            var codeBehind = new TestCodeBehind(fiveItems: new[] { 111, 222, 333 });
            var sequence = new RevertableSequence(codeBehind);

            //-- Act

            sequence.Perform();

            //-- Assert

            Assert.Equal(sequence.State, RevertableSequenceState.Performed);
            Assert.Equal(codeBehind.TakeLog(), new[] {
                "PerformOne()",
                "PerformTwo()",
                "PerformThree()",
                "PerformFiveItem(111,index=0,last=F)",
                "PerformFiveItem(222,index=1,last=F)",
                "PerformFiveItem(333,index=2,last=T)"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CollectionSteps_PerformFailed_PerformedItemsReverted()
        {
            //-- Arrange

            var codeBehind = new TestCodeBehind(fiveItems: new[] { 111, 222, 333 });
            var sequence = new RevertableSequence(codeBehind);

            codeBehind.ThrowFromPerformFiveItemIndex = 2;

            //-- Act

            var exception = Assert.Throws<TestSequenceException>(() => {
                sequence.Perform();
            });
             
            //-- Assert

            Assert.Equal(exception.Message, "PerformFiveItem(333,index=2,last=T)");
            Assert.Equal(sequence.State, RevertableSequenceState.Reverted);
            Assert.Equal(codeBehind.TakeLog(), new[] {
                "PerformOne()",
                "PerformTwo()",
                "PerformThree()",
                "PerformFiveItem(111,index=0,last=F)",
                "PerformFiveItem(222,index=1,last=F)",
                "THROWING-FROM:PerformFiveItem(333,index=2,last=T)",
                "RevertFiveItem(222,index=1,last=F)",
                "RevertFiveItem(111,index=0,last=F)",
                "RevertFour()",
                "RevertTwo()",
                "RevertOne()"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void MultipleCollectionSteps_PerformSucceeded_AllItemsPerformed()
        {
            //-- Arrange

            var codeBehind = new TestCodeBehind(
                fiveItems: new[] { 111, 222 },
                sixItems: new[] { "AAA", "BBB" },
                sevenItems: new[] { DayOfWeek.Friday, DayOfWeek.Sunday });
            var sequence = new RevertableSequence(codeBehind);

            //-- Act

            sequence.Perform();

            Assert.Equal(sequence.State, RevertableSequenceState.Performed);
            Assert.Equal(codeBehind.TakeLog(), new[] {
                "PerformOne()",
                "PerformTwo()",
                "PerformThree()",
                "PerformFiveItem(111,index=0,last=F)",
                "PerformFiveItem(222,index=1,last=T)",
                "PerformSixItem(AAA,index=0,last=F)",
                "PerformSixItem(BBB,index=1,last=T)",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void MultipleCollectionSteps_RevertSucceeded_AllItemsReverted()
        {
            //-- Arrange

            var codeBehind = new TestCodeBehind(
                fiveItems: new[] { 111, 222 },
                sixItems: new[] { "AAA", "BBB" },
                sevenItems: new[] { DayOfWeek.Friday, DayOfWeek.Sunday });
            var sequence = new RevertableSequence(codeBehind);

            sequence.Perform();
            codeBehind.TakeLog();

            //-- Act

            sequence.Revert();

            Assert.Equal(sequence.State, RevertableSequenceState.Reverted);
            Assert.Equal(codeBehind.TakeLog(), new[] {
                "RevertSevenItem(Sunday,index=1,last=T)",
                "RevertSevenItem(Friday,index=0,last=F)",
                "RevertFiveItem(222,index=1,last=T)",
                "RevertFiveItem(111,index=0,last=F)",
                "RevertFour()",
                "RevertTwo()",
                "RevertOne()",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void MultipleCollectionSteps_PerformFailedThenRevertFailed_AllErrorsInAggregateException()
        {
            //-- Arrange

            var codeBehind = new TestCodeBehind(
                fiveItems: new[] { 111, 222 },
                sixItems: new[] { "AAA", "BBB" },
                sevenItems: new[] { DayOfWeek.Friday, DayOfWeek.Sunday });
            var sequence = new RevertableSequence(codeBehind);

            codeBehind.ThrowFromPerformSixItemIndex = 1;
            codeBehind.ThrowFromRevertFiveItemIndex = 0;
            codeBehind.ThrowFromRevertFour = true;

            //-- Act


            var exception = Assert.Throws<AggregateException>(() => {
                sequence.Perform();
            });

            Assert.Equal(sequence.State, RevertableSequenceState.RevertFailed);
            Assert.Equal(codeBehind.TakeLog(), new[] {
                "PerformOne()",
                "PerformTwo()",
                "PerformThree()",
                "PerformFiveItem(111,index=0,last=F)",
                "PerformFiveItem(222,index=1,last=T)",
                "PerformSixItem(AAA,index=0,last=F)",
                "THROWING-FROM:PerformSixItem(BBB,index=1,last=T)",
                "RevertFiveItem(222,index=1,last=T)",
                "THROWING-FROM:RevertFiveItem(111,index=0,last=F)",
                "THROWING-FROM:RevertFour()",
                "RevertTwo()",
                "RevertOne()",
            });

            Assert.True(exception.InnerExceptions.All(e => e is TestSequenceException), "Exception type");
            Assert.Equal(exception.InnerExceptions.Select(e => e.Message), new[] {
                "PerformSixItem(BBB,index=1,last=T)", "RevertFiveItem(111,index=0,last=F)", "RevertFour()"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void MultipleCollectionSteps_RevertFailed_RetryAndRevertAll()
        {
            //-- Arrange

            var codeBehind = new TestCodeBehind(
                fiveItems: new[] { 111, 222 },
                sixItems: new[] { "AAA", "BBB" },
                sevenItems: new[] { DayOfWeek.Friday, DayOfWeek.Sunday });
            var sequence = new RevertableSequence(codeBehind);

            codeBehind.ThrowFromPerformSixItemIndex = 1;
            codeBehind.ThrowFromRevertFiveItemIndex = 0;
            codeBehind.ThrowFromRevertFour = true;

            var exception = Assert.Throws<AggregateException>(() => {
                sequence.Perform();
            });
            
            codeBehind.ThrowFromRevertFiveItemIndex = null;
            codeBehind.ThrowFromRevertFour = false;
            codeBehind.TakeLog();

            //-- Act

            sequence.Revert();

            //-- Assert

            Assert.Equal(sequence.State, RevertableSequenceState.Reverted);
            Assert.Equal(codeBehind.TakeLog(), new[] {
                "RevertFiveItem(111,index=0,last=F)",
                "RevertFour()",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestCodeBehind : IRevertableSequenceCodeBehind
        {
            private readonly List<string> _log = new List<string>();
            private readonly int[] _fiveItems;
            private readonly string[] _sixItems;
            private readonly DayOfWeek[] _sevenItems;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestCodeBehind(int[] fiveItems = null, string[] sixItems = null, DayOfWeek[] sevenItems = null)
            {
                _fiveItems = fiveItems;
                _sixItems = sixItems;
                _sevenItems = sevenItems;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IRevertableSequenceCodeBehind.BuildSequence(IRevertableSequenceBuilder sequence)
            {
                sequence.Once().OnPerform(PerformOne).OnRevert(RevertOne);
                sequence.Once().OnPerform(PerformTwo).OnRevert(RevertTwo);
                sequence.Once().OnPerform(PerformThree);
                sequence.Once().OnRevert(RevertFour);

                if (_fiveItems != null)
                {
                    sequence.ForEach(() => _fiveItems).OnPerform(PerformFiveItem).OnRevert(RevertFiveItem);
                }

                if (_sixItems != null)
                {
                    sequence.ForEach(() => _sixItems).OnPerform(PerformSixItem);
                }

                if (_sevenItems != null)
                {
                    sequence.ForEach(() => _sevenItems).OnRevert(RevertSevenItem);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string[] TakeLog()
            {
                var result = _log.ToArray();
                _log.Clear();
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool ThrowFromPerformOne { get; set; }
            public bool ThrowFromRevertOne { get; set; }
            public bool ThrowFromPerformTwo { get; set; }
            public bool ThrowFromRevertTwo { get; set; }
            public bool ThrowFromPerformThree { get; set; }
            public bool ThrowFromRevertFour { get; set; }
            public int? ThrowFromPerformFiveItemIndex { get; set; }
            public int? ThrowFromRevertFiveItemIndex { get; set; }
            public int? ThrowFromPerformSixItemIndex { get; set; }
            public int? ThrowFromRevertSevenItemIndex { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void PerformOne()
            {
                LogAndThrowIf(ThrowFromPerformOne);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void RevertOne()
            {
                LogAndThrowIf(ThrowFromRevertOne);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void PerformTwo()
            {
                LogAndThrowIf(ThrowFromPerformTwo);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void RevertTwo()
            {
                LogAndThrowIf(ThrowFromRevertTwo);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void PerformThree()
            {
                LogAndThrowIf(ThrowFromPerformThree);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void RevertFour()
            {
                LogAndThrowIf(ThrowFromRevertFour);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void PerformFiveItem(int number, int itemIndex, bool isLastItem)
            {
                LogAndThrowIf(
                    arguments: string.Format("{0},index={1},last={2}", number, itemIndex, isLastItem.ToString().Substring(0, 1)),
                    throwCondition: ThrowFromPerformFiveItemIndex.GetValueOrDefault(-1) == itemIndex);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void RevertFiveItem(int number, int itemIndex, bool isLastItem)
            {
                LogAndThrowIf(
                    arguments: string.Format("{0},index={1},last={2}", number, itemIndex, isLastItem.ToString().Substring(0, 1)),
                    throwCondition: ThrowFromRevertFiveItemIndex.GetValueOrDefault(-1) == itemIndex);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void PerformSixItem(string s, int itemIndex, bool isLastItem)
            {
                LogAndThrowIf(
                    arguments: string.Format("{0},index={1},last={2}", s, itemIndex, isLastItem.ToString().Substring(0, 1)),
                    throwCondition: ThrowFromPerformSixItemIndex.GetValueOrDefault(-1) == itemIndex);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void RevertSevenItem(DayOfWeek day, int itemIndex, bool isLastItem)
            {
                LogAndThrowIf(
                    arguments: string.Format("{0},index={1},last={2}", day, itemIndex, isLastItem.ToString().Substring(0, 1)),
                    throwCondition: ThrowFromRevertSevenItemIndex.GetValueOrDefault(-1) == itemIndex);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void LogAndThrowIf(bool throwCondition, string arguments = "", [CallerMemberName] string methodName = null)
            {
                var message = methodName + "(" + arguments + ")";

                _log.Add((throwCondition ? "THROWING-FROM:" : "") + message);

                if (throwCondition)
                {
                    throw new TestSequenceException(message);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestSequenceException : Exception
        {
            public TestSequenceException(string message)
                : base(message)
            {
            }
        }
    }
}
