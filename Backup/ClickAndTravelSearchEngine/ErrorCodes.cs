using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickAndTravelMiddleOffice
{
    public class ErrorCodes
    {
        public const int CanntParseUserInfo = 10;

        public const int CanntParseArrayOfInteger = 11;

        public const int InvalidDogovorCode = 12;

        public const int InvalidMessageSize = 13;

        public const int InvalidBookId = 14;

        public const int InvalidUserMail = 15;

        public const int InvalidBookIdsLength = 16;

        public const int InvalidDate = 17;

        public const int ServiceNotFound = 18;

        public const int DogovorNotFound = 20;

        public const int InvalidParams = 19;

        public const int AnotherException = 99;
    }
}