using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NostreetsExtensions.DataControl.Enums
{
    public enum TokenPurpose
    {
        TwoFactorAuth,
        EmailValidtion,
        PhoneValidtion,
        PasswordReset
    }

    public enum State
    {
        Success,
        Error,
        Warning,
        Info,
        Question
    }
}
