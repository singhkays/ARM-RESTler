using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading;
using System.Configuration;
using System.Text.RegularExpressions;

namespace ARMRESTler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        Properties.Settings defaultSettings = Properties.Settings.Default;

        public MainWindow ()
        {
            InitializeComponent();
            this.Button_Setup.Click += Button_Setup_Click;
        }

        private void Button_Setup_Click (object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://msdn.microsoft.com/en-us/library/azure/dn790557.aspx");
        }

        private async void Button_Save_Click (object sender, RoutedEventArgs e)
        {
            defaultSettings.oauthEndpoint = this.TextBox_OAuthEndpoint.Text;
            defaultSettings.clientId = this.TextBox_ClientId.Text;
            defaultSettings.redirectUri = this.TextBox_RedirectUri.Text;
            defaultSettings.Save();
            await this.ShowMessageAsync("Saved!", "Your input was succesfully saved", MessageDialogStyle.Affirmative);
        }

        private void Button_Load_Click (object sender, RoutedEventArgs e)
        {
            this.TextBox_OAuthEndpoint.Text = defaultSettings.oauthEndpoint;
            this.TextBox_ClientId.Text = defaultSettings.clientId;
            this.TextBox_RedirectUri.Text = defaultSettings.redirectUri;
        }

        private void Button_GetToken_Click (object sender, RoutedEventArgs e)
        {
            string token = GetAToken(sender, e, this);

            if (token.Equals("regex match error"))
            {
                this.TextBox_BearerToken.Text = "ERROR! Check your OAuth 2.0 Authorization Endpoint again";
                this.Button_GetToken.Content = "ERROR!";
                this.Button_GetToken.Background = Brushes.OrangeRed;
            } else
                this.TextBox_BearerToken.Text = "Bearer " + token;
        }

        // Get the Bearer token from AD
        private static string GetAToken(object sender, RoutedEventArgs e, MainWindow mainWindow)
        {
            // Get the GUID aka tenantId out of the OAuth endpoint
            string txt = mainWindow.TextBox_OAuthEndpoint.Text;
            string re1 = ".*?";	// Non-greedy match on filler
            string re2 = "([A-Z0-9]{8}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{12})";	// SQL GUID 1

            Regex r = new Regex(re1 + re2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = r.Match(txt);
            String tenantId = "";
            if (m.Success)
            {
                tenantId = m.Groups[1].ToString();
            }
            else
            {
                return "regex match error";
            }

            var authenticationContext = new AuthenticationContext("https://login.windows.net/" + tenantId);
            var result = authenticationContext.AcquireToken("https://management.azure.com/", mainWindow.TextBox_ClientId.Text, new Uri(mainWindow.TextBox_RedirectUri.Text));
  
            if (result == null) {
            throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            string token = result.AccessToken;
  
            return token;
        }

        private void Button_GetToken_Copy_Click (object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(this.TextBox_BearerToken.Text);
            this.Button_GetToken_Copy.Content = "copied!";
            this.Button_GetToken_Copy.Background = Brushes.ForestGreen;
        }
    }
}
