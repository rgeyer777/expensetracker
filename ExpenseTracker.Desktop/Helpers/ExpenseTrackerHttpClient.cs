using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ExpenseTracker.Desktop.Helpers
{
    public class ExpenseTrackerHttpClient
    {
        public static HttpClient GetClient()
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(ExpenseTrackerConstants._ExpenseTrackerAzureAPI);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }
    }
}