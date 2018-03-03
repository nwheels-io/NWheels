using System;
using System.Collections.Generic;
using System.Text;

namespace ElectricityBilling.Domain.Accounts
{
    public enum LoginFault
    {
        LoginIncorrect,
        AccountLocked,
        PasswordExpired
    }
}
