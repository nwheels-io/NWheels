using System.Collections.Generic;
using System.Linq;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Domains.DevOps.Alerts.Entities
{
    [EntityContract]
    public interface ISystemAlertConfigurationEntity
    {
        string Id { get; set; }

        ICollection<IEntityPartAlertAction> AlertActions { get; }

        [PropertyContract.Calculated]
        IList<string> AvailableAlertIds { get; }

        [PropertyContract.Calculated]
        string DescriptionText { get; }
        
        [PropertyContract.Calculated]
        string PossibleReasonText { get; }

        [PropertyContract.Calculated]
        string SuggestedActionText { get; }

        [PropertyContract.Calculated]
        string GroupBy { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class SystemAlertConfigurationEntity : ISystemAlertConfigurationEntity
    {
        #region Implementation of ISystemAlertConfigurationEntity

        public abstract string Id { get; set; }
        public abstract ICollection<IEntityPartAlertAction> AlertActions { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityImplementation.CalculatedProperty]
        public virtual IList<string> AvailableAlertIds 
        {
            get
            {
                return Configuration.AlertList.Select(alert => alert.Name).ToArray();
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string DescriptionText
        {
            get
            {
                if (string.IsNullOrEmpty(Id))
                {
                    return null;
                }

                return Configuration.AlertList[Id].Description;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string PossibleReasonText
        {
            get 
            {
                if (string.IsNullOrEmpty(Id))
                {
                    return null;
                }

                return Configuration.AlertList[Id].PossibleReason;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string SuggestedActionText
        {
            get
            {
                if (string.IsNullOrEmpty(Id))
                {
                    return null;
                }

                return Configuration.AlertList[Id].SuggestedAction;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string GroupBy
        {
            get
            {
                if (string.IsNullOrEmpty(Id))
                {
                    return null;
                }

                return Configuration.AlertList[Id].GroupBy;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityImplementation.DependencyProperty]
        protected ISystemAlertConfigurationFeatureSection Configuration { get; set; }
    }
}
