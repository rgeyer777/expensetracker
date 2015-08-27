using System;
using System.Web;
using System.Net.Http;
using System.Security.Claims;
using System.Net.Http.Headers;

namespace ExpenseTracker.WebClient.Helpers
{
    public class ExpenseTrackerHttpClient
    {
        public static HttpClient GetClient()
        {
            HttpClient client = new HttpClient();

            //Get access token
            var token = (HttpContext.Current.User.Identity as ClaimsIdentity).FindFirst("access_token");
            if (token != null)
            {
                client.SetBearerToken(token.Value);
            }

            client.BaseAddress = new Uri(ExpenseTrackerConstants._ExpenseTrackerAzureAPI);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }
    }
}