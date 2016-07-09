using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.Extensions;
using NWheels.UI.Uidl;

namespace NWheels.Globalization.Core
{
    public class LocaleEntryKey : IEquatable<LocaleEntryKey>
    {
        private readonly string _key;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LocaleEntryKey(string stringId, string origin)
        {
            this.StringId = stringId;
            this.Origin = origin;
            _key = MakeKey(stringId, origin);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LocaleEntryKey(string stringId, AbstractUidlNode uidlNode, string nodePropertyName)
            : this(
                stringId, 
                uidlNode.QualifiedName + (nodePropertyName != null ? "." + nodePropertyName : string.Empty))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEquatable<LocaleEntry>

        public bool Equals(LocaleEntryKey other)
        {
            if (other != null && other._key == this._key)
            {
                return true;
            }

            return false;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of Object

        public override bool Equals(object obj)
        {
            var otherEntry = obj as LocaleEntryKey;

            if (otherEntry != null)
            {
                return Equals(otherEntry);
            }

            return base.Equals(obj);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override int GetHashCode()
        {
            return this._key.GetHashCode();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return this._key;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string StringId { get; private set; }
        public string Origin { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<LocaleEntryKey> Enumerate(AbstractUidlNode uidlNode, params string[] stringPropertyPairs)
        {
            if ((stringPropertyPairs.Length % 2) != 0)
            {
                throw new ArgumentException("Bad pairing of stringId/propertyName array elements.", paramName: "stringPropertyPairs");
            }

            int index = 0;

            while (index < stringPropertyPairs.Length - 1)
            {
                var stringId = stringPropertyPairs[index];
                var propertyName = stringPropertyPairs[index + 1];

                if (!string.IsNullOrEmpty(stringId))
                {
                    yield return new LocaleEntryKey(stringId, uidlNode, propertyName);
                }
                
                index += 2;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<KeyGroup> GroupByStringId(IEnumerable<LocaleEntryKey> keys)
        {
            return keys.GroupBy(k => k.StringId).Select(g => new KeyGroup(g.Key, g.Select(k => k.Origin)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string MakeKey(string stringId, string origin)
        {
            return "#" + origin.OrDefaultIfNullOrEmpty(string.Empty) + "#" + stringId;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class KeyGroup
        {
            public KeyGroup(string stringId, IEnumerable<string> origins)
            {
                this.StringId = stringId;
                this.Origins = origins.ToArray();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string StringId { get; private set; }
            public IReadOnlyList<string> Origins { get; private set; }
        }
    }
}
