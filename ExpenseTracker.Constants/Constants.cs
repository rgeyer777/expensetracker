using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpenseTracker
{
    public class ExpenseTrackerConstants
    {
        //public const string ExpenseTrackerAPI = "http://localhost:43321/";
        public const string _ExpenseTrackerAzureAPI = "https://expensetracker.azurewebsites.net/";
        public const string ExpenseTrackerClient = "https://localhost:44310/";
        public const string ExpenseTrackerMobile = "ms-app://s-1-15-2-467734538-4209884262-1311024127-1211083007-3894294004-443087774-3929518054/";

        public const string IdSrvIssuerUri = "https://expensetrackeridsrv3/embedded";

        public const string IdSrvBase = "https://sandboxidsrv.azurewebsites.net/identity";
        public const string IdSrvToken = IdSrvBase + "/connect/token";
        public const string IdSrvAuthorize = IdSrvBase + "/connect/authorize";
        public const string IdSrvUserInfo = IdSrvBase + "/connect/userinfo";
        
    }
}
