using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.UI;

namespace NWheels.Domains.Security.UI
{
    public interface ISecurityDomainApi
    {
        [DomainApiFault(typeof(LoginFault))]
        ILogUserInReply LogUserIn(ILogUserInRequest request);

        [DomainApiFault(typeof(LogoutFault))]
        ILogUserOutReply LogUserOut(ILogUserOutRequest request);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [ViewModelContract]
    public interface ILogUserInRequest
    {
        [PropertyContract.Required]
        string LoginName { get; set; }
        
        [PropertyContract.Required, PropertyContract.Semantic.Password]
        string Password { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [ViewModelContract]
    public interface ILogUserInReply
    {
        [PropertyContract.ReadOnly]
        IEntityId<IUserAccountEntity> UserId { get; set; }
        
        [PropertyContract.ReadOnly]
        string[] AuthorizedUidlNodes { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [ViewModelContract]
    public interface ILogUserOutRequest
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [ViewModelContract]
    public interface ILogUserOutReply
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum LoginFault
    {
        LoginIncorrect,
        PasswordExpired,
        AccountLockedOut
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum LogoutFault
    {
        NotLoggedIn
    }
}
