using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickAndTravelMiddleOffice.Exceptions
{
    public class CatmoException : Exception
    {
        public CatmoException()
        { }

        public CatmoException(string Message)
            : base(Message)
        { }

        public CatmoException(string Message, int Code)
            : base("" + Code + "~" + Message)
        {
            this.Code = Code;
        }

        public int Code;
    }
}