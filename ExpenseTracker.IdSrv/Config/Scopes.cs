using System;
using System.Web;
using System.Linq;
using System.IdentityModel;
using IdentityServer3.Core;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using Thinktecture.IdentityServer.Core.Models;
using Scope = Thinktecture.IdentityServer.Core.Models.Scope;

namespace ExpenseTracker.IdSrv.Config
{
    public class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            var scopes = new List<Scope>
            {
                //Identity Scopes
                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.Roles,
                
                new Scope
                {
                    Name = "expensetrackerapi",
                    DisplayName = "ExpenseTracker API Scope",
                    Type = ScopeType.Resource,
                    Emphasize = false,
                    Enabled = true,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim("role")
                    }
                }
            };
            return scopes;
        }
    }
}