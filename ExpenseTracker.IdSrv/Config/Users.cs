using System;
using System.Web;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Constants = Thinktecture.IdentityServer.Core.Constants;

namespace ExpenseTracker.IdSrv.Config
{
    public class Users
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser>()
            {
                new InMemoryUser
                {
                    Username = "Ron",
                    Password = "secret",
                    Subject = "1",

                    Claims = new[]
                    {
                        new Claim(Constants.ClaimTypes.GivenName, "Ron"), 
                        new Claim(Constants.ClaimTypes.FamilyName, "Geyer"), 
                        new Claim(Constants.ClaimTypes.Role, "WebReadUser"), 
                        new Claim(Constants.ClaimTypes.Role, "WebWriteUser"), 
                        new Claim(Constants.ClaimTypes.Role, "DesktopReadUser"), 
                        new Claim(Constants.ClaimTypes.Role, "DesktopWriteUser"),
                        new Claim(Constants.ClaimTypes.Role, "MobileReadUser"), 
                        new Claim(Constants.ClaimTypes.Role, "MobileWriteUser")
                    }
                },

                new InMemoryUser
                {
                    Username = "bob",
                    Password = "secret",
                    Subject = "2",

                    Claims = new[]
                    {
                        new Claim(Constants.ClaimTypes.GivenName, "Bob"), 
                        new Claim(Constants.ClaimTypes.FamilyName, "Bobbit"), 
                        new Claim(Constants.ClaimTypes.Role, "WebReadUser"), 
                        new Claim(Constants.ClaimTypes.Role, "DesktopReadUser"), 
                        new Claim(Constants.ClaimTypes.Role, "MobileReadUser")
                    }
                }
            };
        }
    }
}