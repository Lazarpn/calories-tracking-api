using CaloriesTracking.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaloriesTracking.Common.Exceptions;

public class AuthorizationException : SystemException
{
    public AuthorizationException(ErrorCode errorCode, object responseParams = null) : base(errorCode, responseParams) { }
    public AuthorizationException(List<ExceptionDetail> details) : base(details)
    { }
}
