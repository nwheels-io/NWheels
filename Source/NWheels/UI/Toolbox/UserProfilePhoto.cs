using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;
using NWheels.UI.Uidl;
using NWheels.Utilities;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class UserProfilePhoto : WidgetBase<UserProfilePhoto, Empty.Data, UserProfilePhoto.IState>
    {
        public UserProfilePhoto(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<UserProfilePhoto, Empty.Data, IState> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void SetEntity<TEntity>(Expression<Func<TEntity, object>> imageType, Expression<Func<TEntity, object>> imageContent)
        {
            this.EntityName = MetadataCache.GetTypeMetadata(typeof(TEntity)).QualifiedName;
            this.ImageTypeProperty = imageType.GetPropertyInfo().Name;
            this.ImageContentProperty = imageContent.GetPropertyInfo().Name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string EntityName { get; set; }
        [DataMember]
        public string ImageTypeProperty { get; set; }
        [DataMember]
        public string ImageContentProperty { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IState
        {
            string UserId { get; set; }
            string[] UserRoles { get; set; }
            string PersonFullName { get; set; }
            string ProfilePhotoId { get; set; }
        }
    }
}
