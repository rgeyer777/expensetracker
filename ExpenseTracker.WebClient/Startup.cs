using Owin;
using System;
using System.Web;
using System.Linq;
using System.Text;
using Microsoft.Owin;
using System.Diagnostics;
using System.Web.Helpers;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using Microsoft.Owin.Security.Cookies;
using ExpenseTracker.WebClient.Helpers;
using Microsoft.Owin.Security.OpenIdConnect;

[assembly: OwinStartup(typeof(ExpenseTracker.WebClient.Startup))]

namespace ExpenseTracker.WebClient
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //Don't map Microsoft's Json WebTokenhandler claim types to .NET claim types. So reset the handler to an empty dictionary
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            //have mvc use this claim as its key:
            AntiForgeryConfig.UniqueClaimTypeIdentifier = "unique_user_key";

            app.UseResourceAuthorization(new AuthorizationManager());

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = "mvc",
                Authority = ExpenseTrackerConstants.IdSrvBase,
                RedirectUri = ExpenseTrackerConstants.ExpenseTrackerClient,
                SignInAsAuthenticationType = "Cookies",

                ResponseType = "code id_token token",
                Scope = "openid profile roles expensetrackerapi",

                Notifications = new OpenIdConnectAuthenticationNotifications()
                {
                    MessageReceived = async n =>
                    {
                        EndpointAndTokenHelper.DecodeAndWrite(n.ProtocolMessage.IdToken);
                        EndpointAndTokenHelper.DecodeAndWrite(n.ProtocolMessage.AccessToken);

                        //var userInfo = await EndpointAndTokenHelper.CallUserInfoEndpoint(n.ProtocolMessage.AccessToken);
                    },

                    SecurityTokenValidated = async n =>
                    {
                        var userInfo = await EndpointAndTokenHelper.CallUserInfoEndpoint(n.ProtocolMessage.AccessToken);

                        var givenNameClaim = new Claim(
                            Thinktecture.IdentityModel.Client.JwtClaimTypes.GivenName,
                            userInfo.Value<string>("given_name"));

                        var familyNameClaim = new Claim(
                            Thinktecture.IdentityModel.Client.JwtClaimTypes.FamilyName,
                            userInfo.Value<string>("family_name"));

                        var roles = userInfo.Value<JArray>("role").ToList();

                        var newIdentity = new ClaimsIdentity(
                           n.AuthenticationTicket.Identity.AuthenticationType,
                           Thinktecture.IdentityModel.Client.JwtClaimTypes.GivenName,
                           Thinktecture.IdentityModel.Client.JwtClaimTypes.Role);

                        newIdentity.AddClaim(givenNameClaim);
                        newIdentity.AddClaim(familyNameClaim);

                        foreach (var role in roles)
                        {
                            newIdentity.AddClaim(new Claim(
                                Thinktecture.IdentityModel.Client.JwtClaimTypes.Role, role.ToString()));
                        }

                        var issuerClaim = n.AuthenticationTicket.Identity
                            .FindFirst(Thinktecture.IdentityModel.Client.JwtClaimTypes.Issuer);
                        var subjectClaim = n.AuthenticationTicket.Identity
                            .FindFirst(Thinktecture.IdentityModel.Client.JwtClaimTypes.Subject);

                        newIdentity.AddClaim(new Claim("unique_user_key", issuerClaim.Value + "_" + subjectClaim.Value));

                        newIdentity.AddClaim(new Claim("access_token", n.ProtocolMessage.AccessToken));

                        n.AuthenticationTicket = new AuthenticationTicket(newIdentity, n.AuthenticationTicket.Properties);

                    },
                }
            });
        }
    }
}