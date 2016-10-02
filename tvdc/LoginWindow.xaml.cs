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
using System.Windows.Shapes;

namespace tvdc
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {

        public string Oauth { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
        }

        public bool? ShowDialog(bool showLoginBtn)
        {

            if (showLoginBtn)
            {
                btnLogin.Visibility = Visibility.Visible;
                pb.Visibility = Visibility.Collapsed;
                lblStatus.Visibility = Visibility.Collapsed;
            } else
            {
                btnLogin.Visibility = Visibility.Collapsed;
                pb.Visibility = Visibility.Visible;
                lblStatus.Visibility = Visibility.Visible;
            }

            return ShowDialog();

        }

        public void Show(bool showLoginBtn)
        {

            if (showLoginBtn)
            {
                btnLogin.Visibility = Visibility.Visible;
                pb.Visibility = Visibility.Collapsed;
                lblStatus.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnLogin.Visibility = Visibility.Collapsed;
                pb.Visibility = Visibility.Visible;
                lblStatus.Visibility = Visibility.Visible;
            }

            Show();

        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {

            AuthenticationWindow aw = new AuthenticationWindow();
            aw.ShowDialog();
            if (aw.oauth != "")
            {
                Oauth = aw.oauth;
                DialogResult = true;
            } else
            {
                DialogResult = false;
            }

            Close();

        }
    }
}
