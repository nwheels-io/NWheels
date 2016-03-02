using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Caliburn.Micro;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon.Caliburn.Validation {
    /// <summary>
    /// A container for all <see cref="IValidator"/> instances used by an object.
    /// </summary>
    public class ValidationAdapter
#if !NET || NET45
        : INotifyDataErrorInfo
#endif
    {
        private readonly IList<IValidator> validators = new List<IValidator>();
        private readonly IDictionary<string, IList<string>> validationErrors = new Dictionary<string, IList<string>>();

        /// <summary>
        /// Gets the validators.
        /// </summary>
        public ICollection<IValidator> Validators {
            get { return validators; }
        }

        /// <summary>
        /// Validates the property.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="value">The value to validate.</param>
        /// <returns>True, if validation succeeded.</returns>
        public bool ValidateProperty<TProperty>(Expression<Func<TProperty>> property, object value) {
            var propertyName = property.GetMemberInfo().Name;
            return ValidateProperty(propertyName, value);
        }

        /// <summary>
        /// Validates the property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value to validate.</param>
        /// <returns>True, if validation succeeded.</returns>
        public bool ValidateProperty(string propertyName, object value) {
            var values = ValidatePropertyImpl(propertyName, value);

            if (values.Count == 0)
                validationErrors.Remove(propertyName);
            else
                validationErrors[propertyName] = values;

            OnErrorsChanged(propertyName);
            return values.Count == 0;
        }

        private IList<string> ValidatePropertyImpl(string propertyName, object value) {
            var allErrors = new List<string>();
            foreach (var errors in validators.Select(validator => validator.ValidateProperty(propertyName, value))) {
                allErrors.AddRange(errors);
            }
            return allErrors;
        }

        /// <summary>
        /// Validates all properties of the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>True, if any property has validation errors.</returns>
        public bool Validate(object instance) {
            validationErrors.Clear();

            var properties = instance.GetType().GetProperties();
            foreach (var property in properties) {
                if (!validators.Any(validator => validator.CanValidateProperty(property.Name)))
                    continue;

                var getter = DynamicGetter.From(property);
                var errors = ValidatePropertyImpl(property.Name, getter(instance));
                if (errors.Count > 0)
                    validationErrors.Add(property.Name, errors);
            }

            OnErrorsChanged(string.Empty);
            return validationErrors.Count == 0;
        }

        /// <summary>
        /// Gets all validation errors of the spezified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>List of validation errors.</returns>
        public IEnumerable<string> GetPropertyError(string propertyName) {
            IList<string> errors;
            if (validationErrors.TryGetValue(propertyName, out errors))
                return errors;
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets all validation errors of the spezified property.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <returns>List of validation errors.</returns>
        public IEnumerable<string> GetPropertyError<TProperty>(Expression<Func<TProperty>> property) {
            var propertyName = property.GetMemberInfo().Name;
            return GetPropertyError(propertyName);
        }

        /// <summary>
        /// Determines whether the spezified property has validation errors.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>True, if the property has validation errors.</returns>
        public bool HasPropertyError(string propertyName) {
            IList<string> errors;
            if (validationErrors.TryGetValue(propertyName, out errors))
                return errors.Count > 0;
            return false;
        }

        /// <summary>
        /// Determines whether the spezified property has validation errors.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <returns>True, if the property has validation errors.</returns>
        public bool HasPropertyError<TProperty>(Expression<Func<TProperty>> property) {
            var propertyName = property.GetMemberInfo().Name;
            return HasPropertyError(propertyName);
        }

        #region INotifyDataErrorInfo

#if !NET || NET45
        /// <summary>
        /// Occurs when the validation errors have changed for a property or for the entire entity.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
#endif

        /// <summary>
        /// Gets all validation errors of the spezified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>List of validation errors.</returns>
        public IEnumerable GetErrors(string propertyName) {
            return GetPropertyError(propertyName);
        }

        /// <summary>
        /// Gets a value indicating whether any property has validation errors.
        /// </summary>
        public bool HasErrors {
            get { return validationErrors.Count > 0; }
        }

        /// <summary>
        /// Called when a property was validated.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnErrorsChanged(string propertyName) {
#if !NET || NET45
            var handler = ErrorsChanged;
            if (handler != null) {
                handler(this, new DataErrorsChangedEventArgs(propertyName));
            }
#endif
        }

        #endregion
    }
}
