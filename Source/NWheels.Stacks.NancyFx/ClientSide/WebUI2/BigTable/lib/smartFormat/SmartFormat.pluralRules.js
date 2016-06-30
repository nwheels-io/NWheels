/**
 * SmartFormat.plurals.js
 *
 * This file contains the plural rules for most languages.
 *
 * Feel free to delete rules for languages that your application doesn't support.
 *
 */

(function() {
	// Much of this language information came from the following url:
	// http://www.gnu.org/s/hello/manual/gettext/Plural-forms.html
	// The following language codes came from:
	// http://www.loc.gov/standards/iso639-2/php/code_list.php
	Smart.getPluralRule = function(twoLetterISOLanguageName) {
		switch (twoLetterISOLanguageName)
		{
			// Germanic family
			//     English, German, Dutch, Swedish, Danish, Norwegian, Faroese
			// Romanic family
			//     Spanish, Portuguese, Italian, Bulgarian
			// Latin/Greek family
			//     Greek
			// Finno-Ugric family
			//     Finnish, Estonian
			// Semitic family
			//     Hebrew
			// Artificial
			//     Esperanto
			// Finno-Ugric family
			//     Hungarian
			// Turkic/Altaic family
			//     Turkish
			case "en": case "de": case "nl": case "sv": case "da": case "no": case "nn": case "nb": case "fo":
			case "es": case "pt": case "it": case "bg":
			case "el":
			case "fi": case "et":
			case "he":
			case "eo":
			case "hu":
			case "tr":
				return pluralRules.English;

			// Romanic family
			//     Brazilian Portuguese, French
			case "fr":
				return pluralRules.French;

			// Baltic family
			//     Latvian
			case "lv":
				return pluralRules.Latvian;

			// Celtic
			//     Gaeilge (Irish)
			case "ga":
				return pluralRules.Irish;

			// Romanic family
			//     Romanian
			case "ro":
				return pluralRules.Romanian;

			//Baltic family
			//    Lithuanian
			case "lt":
				return pluralRules.Lithuanian;

			// Slavic family
			//     Russian, Ukrainian, Serbian, Croatian
			case "ru": case "uk": case "sr": case "hr":
				return pluralRules.Russian;

			// Slavic family
			//     Czech, Slovak
			case "cs": case "sk":
				return pluralRules.Czech;

			// Slavic family
			//     Polish
			case "pl":
				return pluralRules.Polish;

			// Slavic family
			//     Slovenian
			case "sl":
				return pluralRules.Slovenian;


			default:
				return null;
		}
	};

	var pluralRules = {
		English: function(value, pluralCount)
		{
			// Two forms, singular used for one only
			if (pluralCount == 2)
			{
				return (value == 1 ? 0 : 1);
			}
			// Three forms (zero, one, plural)
			if (pluralCount == 3)
			{
				return (value == 0) ? 0 : (value == 1) ? 1 : 2;
			}
			// Four forms (negative, zero, one, plural)
			if (pluralCount == 4)
			{
				return (value < 0) ? 0 : (value == 0) ? 1 : (value == 1) ? 2 : 3;
			}

			return -1; // Too many parameters!
		}
		,
		French: function(value, pluralCount)
		{
			// Two forms, singular used for zero and one
			if (pluralCount == 2)
			{
				return (value == 0 || value == 1) ? 0 : 1;
			}
			return -1;
		}
		,
		Latvian: function(value, pluralCount)
		{
			// Three forms, special case for zero
			if (pluralCount == 3)
			{
				return (value % 10 == 1 && value % 100 != 11) ? 0 : (value != 0) ? 1 : 2;
			}
			return -1;
		}
		,
		Irish: function(value, pluralCount)
		{
			// Three forms, special cases for one and two
			if (pluralCount == 3)
			{
				return (value == 1) ? 0 : (value == 2) ? 1 : 2;

			}
			return -1;
		}
		,
		Romanian: function(value, pluralCount)
		{
			// Three forms, special case for numbers ending in 00 or [2-9][0-9]
			if (pluralCount == 3)
			{
				return (value == 1) ? 0 : (value == 0 || (value % 100 > 0 && value % 100 < 20)) ? 1 : 2;
			}
			return -1;
		}
		,
		Lithuanian: function(value, pluralCount)
		{
			// Three forms, special case for numbers ending in 1[2-9]
			if (pluralCount == 3)
			{
				return (value % 10 == 1 && value % 100 != 11) ? 0 : (value % 10 >= 2 && (value % 100 < 10 || value % 100 >= 20)) ? 1 : 2;
			}
			return -1;
		}
		,
		Russian: function(value, pluralCount)
		{
			// Three forms, special cases for numbers ending in 1 and 2, 3, 4, except those ending in 1[1-4]
			if (pluralCount == 3)
			{
				return (value % 10 == 1 && value % 100 != 11) ? 0 : (value % 10 >= 2 && value % 10 <= 4 && (value % 100 < 10 || value % 100 >= 20)) ? 1 : 2;
			}
			return -1;
		}
		,
		Czech: function(value, pluralCount)
		{
			// Three forms, special cases for 1 and 2, 3, 4
			if (pluralCount == 3)
			{
				return (value == 1) ? 0 : (value >= 2 && value <= 4) ? 1 : 2;
			}
			return -1;
		}
		,
		Polish: function(value, pluralCount)
		{
			// Three forms, special case for one and some numbers ending in 2, 3, or 4
			if (pluralCount == 3)
			{
				return (value == 1) ? 0 : (value % 10 >= 2 && value % 10 <= 4 && (value % 100 < 10 || value % 100 >= 20)) ? 1 : 2;
			}
			return -1;
		}
		,
		Slovenian: function(value, pluralCount)
		{
			// Four forms, special case for one and all numbers ending in 02, 03, or 04
			if (pluralCount == 4)
			{
				return (value % 100 == 1) ? 0 : (value % 100 == 2) ? 1 : (value % 100 == 3 || value % 100 == 4) ? 2 : 3;
			}
			return -1;
		}
	};

})();