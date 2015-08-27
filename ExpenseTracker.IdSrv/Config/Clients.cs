using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;


namespace ExpenseTracker.IdSrv.Config
{
    public class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new[]
            {
                new Client
                {
                    Enabled = true,
                    ClientName = "ExpenseTracker MVC Client (Hybrid Flow)",
                    ClientId = "mvc",
                    Flow = Flows.Hybrid,
                    RequireConsent = true,

                    RedirectUris = new List<string> {ExpenseTrackerConstants.ExpenseTrackerClient}
                },

                new Client
                {
                    Enabled = true,
                    ClientName = "Implicit Client",
                    ClientId = "implicitclient",
                    Flow = Flows.Implicit,
                    RequireConsent = true,

                    RedirectUris = new List<string>
                    {
                      // WPF client
                        "oob://localhost/wpfclient",
                    },

                    //For this client, restrict scopes to just these:
                    ScopeRestrictions = new List<string>
                    { 
                        Constants.StandardScopes.OpenId, 
                        Constants.StandardScopes.Roles,
                        "expensetrackerapi"
                    },
                }
            };
        }
    }
}