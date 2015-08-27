using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Windows;
using System.Net.Http;
using IdentityModel;
using Newtonsoft.Json;
using IdentityModel.Client;
using MahApps.Metro.Controls;
using System.Windows.Documents;
using System.Collections.Generic;
using ExpenseTracker.Desktop.Helpers;
using Newtonsoft.Json.Linq;
using ExpenseTracker.Repository.Entities;

namespace ExpenseTracker.Desktop
{
    public partial class MainWindow : MetroWindow
    {
        private LoginWindow _LoginWindow;
        private IdentityModel.Client.AuthorizeResponse _response;

        HttpClient _Client = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();

            _Client = ExpenseTrackerHttpClient.GetClient();

            _LoginWindow = new LoginWindow();

            _LoginWindow.Done += LoginWindow_Done;

            //LoadDataButton.IsEnabled = false;
        }

        private void LoginWindow_Done(object sender, IdentityModel.Client.AuthorizeResponse e)
        {
            _response = e;
            OutputTextBox.Document.Blocks.Clear();
            OutputTextBox.Document.Blocks.Add(new Paragraph(new Run(e.Raw)));
        }

        async private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // for reference: if you ever need to get the correct callbackUri
            // for your app
            // var callbackUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri();

            RequestToken("openid roles expensetrackerapi", "id_token token");

            LoadDataButton.IsEnabled = true;
        }

        

        private void RequestToken(string scope, string responseType)
        {
            string nonce = Guid.NewGuid().ToString() + DateTime.Now.Ticks;

            var request = new AuthorizeRequest(ExpenseTrackerConstants.IdSrvAuthorize);
            var startUrl = request.CreateAuthorizeUrl(
                clientId: "implicitclient",
                responseType: responseType,
                scope: scope,
                redirectUri: @"oob://localhost/wpfclient",
                state: "random_state",
                nonce: "random_nonce");
                //nonce: "random_nonce" /**,
                //loginHint: "alice",
                //acrValues: "idp:Google b c" **/);

            _LoginWindow.Show();
            _LoginWindow.Start(new Uri(startUrl), new Uri(@"oob://localhost/wpfclient"));

        }

        private void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            
            LoadExpenseGroups();
        }

        async private void LoadExpenseGroups()
        {
            MetroProgressBar.Visibility = Visibility.Visible;

            if (_response != null && _response.Values.ContainsKey("access_token"))
            {
                _Client.SetBearerToken(_response.AccessToken);
            }
            
            HttpResponseMessage response = await _Client.GetAsync("api/expensegroups?sort=expensegroupstatusid");
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                var pagingInfo = HeaderParser.FindAndParsePagingInfo(response.Headers);

                var listExpenseGroups = JsonConvert.DeserializeObject<IEnumerable<ExpenseGroup>>(content);

                List<ExpenseGroup> expenseGroups = listExpenseGroups.ToList();

                ExpenseGroupsGrid.ItemsSource = expenseGroups;

            }
            else
            {
                MessageBox.Show(response.StatusCode.ToString());
            }

            MetroProgressBar.Visibility = Visibility.Hidden;
        }

        private async void UserInfoButton_Click(object sender, RoutedEventArgs e)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(ExpenseTrackerConstants.IdSrvUserInfo)
            };

            // authorization header
            if (_response != null && _response.Values.ContainsKey("access_token"))
            {
                string at = _response.AccessToken;

                client.SetBearerToken(at);
            }

            var response = await client.GetAsync("");

            // form post
            //HttpResponseMessage response;
            //if (_response != null && _response.Values.ContainsKey("access_token"))
            //{
            //    var body = new Dictionary<string, string>
            //    {
            //        { "access_token", _response.AccessToken }
            //    };

            //    response = await client.PostAsync("", new FormUrlEncodedContent(body));
            //}
            //else
            //{
            //    return;
            //}

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var json = await response.Content.ReadAsStringAsync();

                string jsonString = JObject.Parse(json).ToString();

                OutputTextBox.Document.Blocks.Clear();
                OutputTextBox.Document.Blocks.Add(new Paragraph(new Run(jsonString)));
                
            }
            else
            {
                MessageBox.Show(response.StatusCode.ToString());
            }
        }

        private void ShowAccessTokenButton_Click(object sender, RoutedEventArgs e)
        {
            if (_response.Values.ContainsKey("access_token"))
            {
                var token = _response.Values["access_token"]; 
                var parts = token.Split('.');
                var part = Encoding.UTF8.GetString(Base64Url.Decode(parts[1]));

                var jwt = JObject.Parse(part);

                OutputTextBox.Document.Blocks.Clear();
                OutputTextBox.Document.Blocks.Add(new Paragraph(new Run(jwt.ToString())));
            }
        }
    }
}
