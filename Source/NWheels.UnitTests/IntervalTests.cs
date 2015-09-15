using System;
using NUnit.Framework;
using Shouldly;

namespace NWheels.UnitTests
{
    [TestFixture]
    public class IntervalTests
    {
        static readonly object[] TestCases = {
			// Closed interval: [a, b]
			new object[] { Interval.Range(1, 10), 1, true },
			new object[] { Interval.Range(1, 10), 10, true },
			new object[] { Interval.Range(1, 10), 11, false },
			new object[] { Interval.Range(1, 10), 0, false },

			new object[] { Interval.Range(1m, 10m), 1m, true },
			new object[] { Interval.Range(1m, 10m), 10m, true },
			new object[] { Interval.Range(1m, 10m), 11m, false },
			new object[] { Interval.Range(1m, 10m), 0m, false },

			// Empty interval: (a, a], [a, a), (a, a)
			new object[] { Interval.Range(1, 1, IntervalType.Open), 1, false },
			new object[] { Interval.Range(1, 1, IntervalType.Open, IntervalType.Open), 1, false },

			// Degernate interval: [a, a] = {a}
			new object[] { Interval.Range(1, 1), 1, true },

			// Lower bounded interval: (a, +∞), [a, +∞), (a, +∞], [a, +∞]
			new object[] { Interval.Range(-100d, double.PositiveInfinity), double.PositiveInfinity, true }, // []
			new object[] { Interval.Range(-100d, double.PositiveInfinity), 1d, true }, // []
			new object[] { Interval.Range(-100d, double.PositiveInfinity), -100d, true }, // []
			new object[] { Interval.Range(-100d, double.PositiveInfinity), -101d, false }, // []
			new object[] { Interval.Range(-100d, double.PositiveInfinity, IntervalType.Open), 1d, true }, // (]
			new object[] { Interval.Range(-100d, double.PositiveInfinity, IntervalType.Open), -100d, false }, // (]
			new object[] { Interval.Range(-100d, double.PositiveInfinity, IntervalType.Closed, IntervalType.Open), double.PositiveInfinity, false }, // [)
			new object[] { Interval.Range(-100d, double.PositiveInfinity, IntervalType.Closed, IntervalType.Open), 1d, true }, // [)
			new object[] { Interval.Range(-100d, double.PositiveInfinity, IntervalType.Open, IntervalType.Open), 1d, true }, // ()

			// Upper bounded interval: (-∞, b), [-∞, b), (-∞, b], [-∞, b]
			new object[] { Interval.Range(double.NegativeInfinity, 0), -1d, true },
			new object[] { Interval.Range(double.NegativeInfinity, 0), double.NegativeInfinity, true },
			new object[] { Interval.Range(double.NegativeInfinity, 0, IntervalType.Open), double.NegativeInfinity, false },

			// Unbounded interval: (-∞, +∞), [-∞, +∞] etc
			new object[] { Interval.Range(double.NegativeInfinity, double.PositiveInfinity), 1d, true },
			new object[] { Interval.Range(double.NegativeInfinity, double.PositiveInfinity), double.NegativeInfinity, true },
			new object[] { Interval.Range(double.NegativeInfinity, double.PositiveInfinity), double.PositiveInfinity, true },
			new object[] { Interval.Range(double.NegativeInfinity, double.PositiveInfinity, IntervalType.Open, IntervalType.Open), 1d, true },
			new object[] { Interval.Range(double.NegativeInfinity, double.PositiveInfinity, IntervalType.Open, IntervalType.Open), double.NegativeInfinity, false },
			new object[] { Interval.Range(double.NegativeInfinity, double.PositiveInfinity, IntervalType.Open, IntervalType.Open), double.PositiveInfinity, false },

			// Implicit swapping of a and b, i.e., when b < a
			new object[] { Interval.Range(10, 1), 1, true },
			new object[] { Interval.Range(10, 1), 0, false },
			new object[] { Interval.Range(10, 1), 11, false }
		};

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, TestCaseSource("TestCases")]
        public void Contains_should_return_correct_value_for<T>(Interval<T> interval, T point, bool expected) where T : struct, IComparable
        {
            interval.Contains(point).ShouldBe(expected);
        }
    }
}
