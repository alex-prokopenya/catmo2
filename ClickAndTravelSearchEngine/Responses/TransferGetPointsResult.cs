using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelMiddleOffice.Containers.Transfers;

namespace ClickAndTravelMiddleOffice.Responses
{
    public class TransferGetPointsResult: _Response
    {
        private TransferPoint[] _transferPoints;

        public TransferPoint[] TransferPoints
        {
          get { return _transferPoints; }
          set { _transferPoints = value; }
        }
    }
}