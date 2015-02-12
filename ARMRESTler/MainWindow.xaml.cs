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
        private Properties.Settings defaultSettings = Properties.Settings.Default;

        public MainWindow ()
        {
            InitializeComponent();
        }

        private void Button_Setup_Click (object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/singhkay/ARM-RESTler");
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
            if (!this.getTokenButtonText.Text.Equals(ConfigurationManager.AppSettings["getTokenButtonText"]))
            {
                this.getTokenButtonText.Text = ConfigurationManager.AppSettings["getTokenButtonText"];
                this.Button_GetToken.Background = Brushes.White;
                this.TextBox_BearerToken.Text = ConfigurationManager.AppSettings["tokenBoxText"];
            }            

            string token = GetAToken(sender, e, this);

            if (token.Equals("regex match error"))
            {
                this.TextBox_BearerToken.Text = "ERROR! Check your OAuth 2.0 Authorization Endpoint again";
                this.getTokenButtonText.Text = "error!";
                this.Button_GetToken.Background = Brushes.OrangeRed;
            }
            else if (token.Equals("authentication error"))
            {
                this.TextBox_BearerToken.Text = "ERROR! Check your credentials";
                this.getTokenButtonText.Text = "error!";
                this.Button_GetToken.Background = Brushes.OrangeRed;
            } else
                this.TextBox_BearerToken.Text = "Bearer " + token;
        }

        // TODO
        private void Button_GetToken_MouseEnter (object sender, MouseEventArgs e)
        {
            if (!this.getTokenButtonText.Text.Equals(ConfigurationManager.AppSettings["getTokenButtonText"]))
            {
                this.getTokenButtonText.Text = ConfigurationManager.AppSettings["getTokenButtonText"];

                this.Button_GetToken.Background = Brushes.White;
                this.TextBox_BearerToken.Text = ConfigurationManager.AppSettings["tokenBoxText"];
            }
        }

        // Get the Bearer token from AD
        private string GetAToken(object sender, RoutedEventArgs e, MainWindow mainWindow)
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

            AuthenticationContext authenticationContext = new AuthenticationContext("https://login.windows.net/" + tenantId);
            AuthenticationResult result = null;

            try
            {
                result = authenticationContext.AcquireToken("https://management.azure.com/", mainWindow.TextBox_ClientId.Text, new Uri(mainWindow.TextBox_RedirectUri.Text));
            }
            catch (Exception)
            {
            }            
  
            if (result == null) {
                return "authentication error";
            }
            else
            {
                return result.AccessToken;
            }            
        }

        private void Button_GetToken_Copy_Click (object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(this.TextBox_BearerToken.Text);
            this.copyButtonText.Text = "copied!";
            this.Button_GetToken_Copy.Background = Brushes.ForestGreen;
        }

    }
}
