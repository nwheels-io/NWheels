/**
 * SmartFormat.JS - https://github.com/scottrippey/SmartFormat.JS
 *
 * SmartFormat.core.js
 * Core functionality for Smart.format.
 * Only contains the default toString formatter.
 *
 * Usage:
 *
 * Formatting a string, using all default extensions:
 * var result = Smart.format(template, data)
 *
 * Creating a custom formatter, with a custom set of extensions:
 * var formatter = new Smart.SmartFormatter(selectors, formatters)
 */
(function(global) {

	var Smart = global.Smart = {
		/**
		 * Formats the template using the provided data.
		 *
		 * @param {String} template
		 * @param {Object} data
		 * @return {String}
		 */
		format: function(template, data) {
			if (!Smart.defaultInstance) {
				Smart.defaultInstance = new Smart.SmartFormatter(Smart.defaultSelectors, Smart.defaultFormatters);
			}
			return Smart.defaultInstance.format(template, data);
		}
		,
		/**
		 * Adds the extensions to the list of default extensions
		 * @param type - Either 'selector' or 'formatter'
		 * @param hash
		 */
		addExtensions: function (type, hash) {
			var target = null;
			if (type == 'selector') {
				target = Smart.defaultSelectors;
			} else if (type == 'formatter') {
				target = Smart.defaultFormatters;
			} else {
				throw "Unknown extension type";
			}

			for (var name in hash) {
				var extension = hash[name];
				Smart.allExtensions[name] = extension;

				target.unshift(extension);
			}
		}
		,
		/**
		 * Holds the default instance of SmartFormatter
		 */
		defaultInstance: null
		,
		defaultSelectors: [ ]
		,
		defaultFormatters: [ ]
		,
		/**
		 * Holds a reference to all extensions, for the heck of it
		 */
		allExtensions: { }
	};

	Smart.SmartFormatter = function(selectors, formatters) {
		this.selectors = selectors;
		this.formatters = formatters;
	};
	Smart.SmartFormatter.prototype = {
		format: function(template, data) {
			var parser = /\\?\{([^}:]*):?([^}]*)\}/g;
			var self = this;
			return template.replace(parser, function(match, properties, format) {
				var value = self.evaluateSelector(data, properties);
				var result = self.evaluateFormat(value, format);
				return result;
			});
		}
		,
		evaluateSelector: function(data, properties) {
			var value = data;
			each(properties.split('.'), function(property) {
				each(this.selectors, function(selector) {
					var result = selector(value, property);
					if (result !== undefined) {
						value = result;
						return true;
					}
				});
			}, this);
			return value;
		}
		,
		evaluateFormat: function(value, format) {
			var result;
			each(this.formatters, function(formatter) {
				result = formatter(value, format);
				if (result !== undefined) {
					return true;
				}
			});
			return result;
		}

	};

	// Helper method: short-circuiting loop:
	function each(array, callback, bind) {
		for (var i = 0, l = array.length; i < l; i++) {
			if (callback.call(bind, array[i], i))
				return true;
		}
		return false;
	}






	/* Default Extensions */

	Smart.addExtensions('selector', {
		/**
		 * Supports parsing of dot-notation and evaluating simple functions
		 */
		'defaultSelector': function(value, selector) {
			if (value !== undefined && value !== null) {
				var fn = selector.split('()');
				if ((fn.length == 2) && (typeof value[fn[0]] === 'function')) {
					// Simple-function:
					value = value[fn[0]]();
				} else {
					// Dot-notation:
					value = value[selector];
				}
			}
			return (value === undefined) ? null : value;
		}
	});

	Smart.addExtensions('formatter', {
		/**
		 * Converts the value into a string;
		 * also supports calling `toString(format)`
		 */
		'defaultFormatter': function(value, format) {
			if (value !== undefined && value !== null && format) {
				if (typeof value.toString === 'function') {
					return value.toString(format);
				}
			}
			return String(value);
		}
	});

})(this);
