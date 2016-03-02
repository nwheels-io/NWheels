using System;
using System.Globalization;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon.Caliburn.Validation {
    /// <summary>
    /// Performs a range validation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RangeValidationRule<T> : ValidationRule<T>
        where T : IComparable<T> {
        private readonly T minimum;
        private readonly T maximum;
        private readonly string errorMessage;
        private readonly string unitSymbol;

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeValidationRule{T}"/> class.
        /// </summary>
        /// <param name="minimum">The minimum value.</param>
        /// <param name="maximum">The maximum value.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="unitSymbol">The unit symbol.</param>
        public RangeValidationRule(T minimum, T maximum, string errorMessage, string unitSymbol) {
            this.minimum = minimum;
            this.maximum = maximum;
            this.errorMessage = errorMessage;
            this.unitSymbol = unitSymbol;
        }

        /// <summary>
        /// Performs validation checks on a value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="cultureInfo">The culture to use in this rule.</param>
        /// <returns>A <see cref="ValidationResult" /> object.</returns>
        protected override ValidationResult OnValidate(T value, CultureInfo cultureInfo) {
            if (value.CompareTo(minimum) < 0 || value.CompareTo(maximum) > 0)
                return ValidationResult.Failure(cultureInfo, errorMessage, minimum, maximum, value, unitSymbol);

            return ValidationResult.Success();
        }
    }
}
