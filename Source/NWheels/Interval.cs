using System;

namespace NWheels
{
    /// <summary>
    /// Represents vectorless interval of the form [a, b] or (a, b) or any
    /// combination of exclusive and inclusive end points.
    /// </summary>
    /// <typeparam name="T">Any comparent type</typeparam>
    /// <remarks>
    /// This is a vectorless interval, therefore if end component is larger
    /// than start component, the interval will swap the two ends around
    /// such that a is always %lt; b.
    /// </remarks>
    public struct Interval<T> : IInterval 
        where T : struct, IComparable
    {
        public T LowerBound { get; private set; }
        public T UpperBound { get; private set; }
        public IntervalType LowerBoundIntervalType { get; private set; }
        public IntervalType UpperBoundIntervalType { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Interval(
            T lowerbound,
            T upperbound,
            IntervalType lowerboundIntervalType = IntervalType.Closed,
            IntervalType upperboundIntervalType = IntervalType.Closed)
            : this()
        {
            var a = lowerbound;
            var b = upperbound;
            var comparison = a.CompareTo(b);

            if (comparison > 0)
            {
                a = upperbound;
                b = lowerbound;
            }

            LowerBound = a;
            UpperBound = b;

            //TODO: swap bound types together with the bounds 
            LowerBoundIntervalType = lowerboundIntervalType;
            UpperBoundIntervalType = upperboundIntervalType;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Check if given point lies within the interval.
        /// </summary>
        /// <param name="point">Point to check</param>
        /// <returns>True if point lies within the interval, otherwise false</returns>
        public bool Contains(T point)
        {
            if (LowerBound.GetType() != typeof(T)
                || UpperBound.GetType() != typeof(T))
            {
                throw new ArgumentException("Type mismatch", "point");
            }

            var lower = LowerBoundIntervalType == IntervalType.Open
                ? LowerBound.CompareTo(point) < 0
                : LowerBound.CompareTo(point) <= 0;
            var upper = UpperBoundIntervalType == IntervalType.Open
                ? UpperBound.CompareTo(point) > 0
                : UpperBound.CompareTo(point) >= 0;

            return lower && upper;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Convert to mathematical notation using open and closed parenthesis:
        /// (, ), [, and ].
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "{0}{1}, {2}{3}",
                LowerBoundIntervalType == IntervalType.Open ? "(" : "[",
                LowerBound,
                UpperBound,
                UpperBoundIntervalType == IntervalType.Open ? ")" : "]"
            );
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        bool IInterval.Contains(object value)
        {
            return this.Contains((T)value);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Static class to generate regular Intervals using common types.
    /// </summary>
    public static class Interval
    {
        public static Interval<double> Range(double lowerbound, double upperbound, IntervalType lowerboundIntervalType = IntervalType.Closed, IntervalType upperboundIntervalType = IntervalType.Closed)
        {
            return new Interval<double>(lowerbound, upperbound, lowerboundIntervalType, upperboundIntervalType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Interval<decimal> Range(decimal lowerbound, decimal upperbound, IntervalType lowerboundIntervalType = IntervalType.Closed, IntervalType upperboundIntervalType = IntervalType.Closed)
        {
            return new Interval<decimal>(lowerbound, upperbound, lowerboundIntervalType, upperboundIntervalType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Interval<int> Range(int lowerbound, int upperbound, IntervalType lowerboundIntervalType = IntervalType.Closed, IntervalType upperboundIntervalType = IntervalType.Closed)
        {
            return new Interval<int>(lowerbound, upperbound, lowerboundIntervalType, upperboundIntervalType);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// An interval could be open and closed or combination of both at either
    /// end.
    /// </summary>
    public enum IntervalType
    {
        Open,
        Closed
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Non-typed view over interval instance.
    /// </summary>
    public interface IInterval
    {
        /// <summary>
        /// Checks whether specified value is contained by the current interval.
        /// </summary>
        /// <param name="value">
        /// The value to check. The type of the value must be compatible with the type of the interval, otherwise InvalidCastException will be thrown.
        /// </param>
        /// <returns>
        /// True if the specified value is contained by the current interval; false otherwise.
        /// </returns>
        bool Contains(object value);
    }
}
