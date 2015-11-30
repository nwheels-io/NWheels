using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.UI;

namespace NWheels.Domains.Security.UI
{
    [ViewModelContract]
    public interface ILogUserInRequest
    {
        [PropertyContract.Required]
        string LoginName { get; set; }

        [PropertyContract.Required, PropertyContract.Semantic.Password]
        string Password { get; set; }

        [PropertyContract.Semantic.Password]
        string NewPassword { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [ViewModelContract]
    public interface ILogUserInReply
    {
        int Id { get; set; }
        string FullName { get; set; }
        string[] Roles { get; set; }
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
        int Id { get; set; }
    }
}
