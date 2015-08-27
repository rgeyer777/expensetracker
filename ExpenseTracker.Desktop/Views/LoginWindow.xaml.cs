using mshtml;
using System;
using System.Linq;
using System.Windows;
using IdentityModel.Client;
using System.ComponentModel;
using MahApps.Metro.Controls;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ExpenseTracker.Desktop
{
    public partial class LoginWindow : MetroWindow
    {
        public AuthorizeResponse AuthorizeResponse { get; set; }
        public event EventHandler<AuthorizeResponse> Done;

        Uri _CallbackUri;
        
        public LoginWindow()
        {
            InitializeComponent();

            webView.Navigating += webView_Navigating;
            Closing += LoginWebView_Closing;
        }
       
        private void LoginWebView_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        public void Start(Uri startUri, Uri callbackUri)
        {
            _CallbackUri = callbackUri;
            webView.Navigate(startUri);
        }

         private void webView_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri.ToString().StartsWith(_CallbackUri.AbsoluteUri))
            {
                if (e.Uri.AbsoluteUri.Contains("#"))
                {
                    AuthorizeResponse = new AuthorizeResponse(e.Uri.AbsoluteUri);
                }
                else
                {
                    var document = (IHTMLDocument3)((WebBrowser)sender).Document;
                    var inputElements = document.getElementsByTagName("INPUT").OfType<IHTMLElement>();
                    var resultUrl = "?";

                    foreach (var input in inputElements)
                    {
                        resultUrl += input.getAttribute("name") + "=";
                        resultUrl += input.getAttribute("value") + "&";
                    }

                    resultUrl = resultUrl.TrimEnd('&');
                    AuthorizeResponse = new AuthorizeResponse(resultUrl);
                }

                e.Cancel = true;
                this.Visibility = Visibility.Hidden;

                if (Done != null)
                {
                    Done.Invoke(this, AuthorizeResponse);
                }
            }
        }
    }
}
