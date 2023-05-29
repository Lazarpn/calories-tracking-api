using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaloriesTracking.Common.Enums;
public enum ErrorCode
{
    RequestInvalid,
    RequirementsNotMet,
    EntityDoesNotExist,
    EntityAlreadyExists,
    IdentityError,
    InternalServerError,
    InvalidCredentials
}
