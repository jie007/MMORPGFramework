using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.RestApi
{
    public enum RegisterResult
    {
        Ok = 0,
        NoEmail = 1,
        IncorrectEmail = 2,
        NoPassword = 3,
        PasswordNotStrongEnough = 4,
        AlreadyRegistered = 5
    }
}
