using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace tvdc
{
    /// <summary>
    /// Interaction logic for AuthenticationWindow.xaml
    /// </summary>
    public partial class AuthenticationWindow : Window
    {

        public string oauth { get; private set; }

        public AuthenticationWindow()
        {
            InitializeComponent();
            HideScriptErrors(wb, true);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            wb.Navigate("https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id=" +
                Properties.Resources.client_id +
                "&redirect_uri=http://newcat.bplaced.net/tvd/&scope=chat_login+user_subscriptions+user_read&force_verify=true");
        }

        private void wb_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.Uri.ToString().StartsWith("http://newcat.bplaced.net/tvd/"))
            {
                oauth = "oauth:" + e.Uri.Fragment.Substring(14, e.Uri.Fragment.IndexOf('&') - 14);
                Close();
            }
        }

        private void HideScriptErrors(WebBrowser webBrowser, bool hide)
        {
            var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;
            var objComWebBrowser = fiComWebBrowser.GetValue(webBrowser);
            if (objComWebBrowser == null)
            {
                webBrowser.Loaded += (o, s) => HideScriptErrors(webBrowser, hide); //In case we are to early
                return;
            }
            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
        }
    }
}
