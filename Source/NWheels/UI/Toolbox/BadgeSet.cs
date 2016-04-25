using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class BadgeSet : WidgetBase<BadgeSet, Empty.Data, Empty.State>
    {
        public BadgeSet(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            this.Badges = new List<Badge>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return Enumerable.Concat(
                base.GetTranslatables(), 
                Badges.Where(b => b.ValueText != null).SelectMany(b => b.ValueText.Values));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BadgeSet WithBadgeFor<TEntity>(Expression<Func<TEntity, object>> property, BadgeStyle style, string format)
        {
            this.Badges.Add(new Badge(property.ToNormalizedNavigationString()) {
                Style = style,
                Format = format
            });

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BadgeSet WithBadgeFor<TEntity>(Expression<Func<TEntity, object>> property, BadgeStyle style, object[] valueTextPairs)
        {
            this.Badges.Add(new Badge(property.ToNormalizedNavigationString(), valueTextPairs) {
                Style = style
            });

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember, ManuallyAssigned]
        public List<Badge> Badges { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<BadgeSet, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = UidlDocument.DataContractNamespace)]
        public class Badge
        {
            public Badge(string expression, object[] valueTextPairs = null)
            {
                this.Expression = expression;

                if (valueTextPairs != null && valueTextPairs.Length > 0)
                {
                    this.ValueText = new Dictionary<object, string>();

                    for (int i = 0 ; i < valueTextPairs.Length - 1 ; i += 2)
                    {
                        this.ValueText[valueTextPairs[i]] = valueTextPairs[i + 1].ToStringOrDefault(null);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public string Expression { get; set; }
            [DataMember]
            public string Format { get; set; }
            [DataMember]
            public BadgeStyle Style { get; set; }
            [DataMember]
            public Dictionary<object, string> ValueText { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum BadgeStyle
        {
            Default,
            Primary,
            Success,
            Info,
            Warning,
            Danger
        }
    }
}
