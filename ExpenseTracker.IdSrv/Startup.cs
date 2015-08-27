using System;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using ExpenseTracker.IdSrv.Config;
using Microsoft.Owin;
using Owin;
using Thinktecture.IdentityServer.Core.Configuration;

[assembly: OwinStartupAttribute(typeof(ExpenseTracker.IdSrv.Startup))]
namespace ExpenseTracker.IdSrv
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);

            app.Map("/identity", idsrvApp => idsrvApp.UseIdentityServer(new IdentityServerOptions
            {
                SiteName = "Embedded IdentityServer",
                IssuerUri = ExpenseTrackerConstants.IdSrvIssuerUri,

                Factory = InMemoryFactory.Create(
                    Users.Get(),
                    Clients.Get(),
                    Scopes.Get()),
                SigningCertificate = LoadCertificate()
            }));
        }

        X509Certificate2 LoadCertificate()
        {
            return new X509Certificate2(
                string.Format(@"{0}\bin\idsrv3test.pfx",
                AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
        }
    }
}
