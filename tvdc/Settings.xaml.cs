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
using System.Diagnostics;
using nUpdate.Updating;
using System.Globalization;

namespace tvdc
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {

        private bool clearedCache = false;

        public Settings()
        {
            InitializeComponent();

            tbNick.Text = Properties.Settings.Default.nick;
            tbOauth.Text = Properties.Settings.Default.oauth;
            tbChannel.Text = Properties.Settings.Default.channel;
            cbDebug.IsChecked = Properties.Settings.Default.debug;
            cbShowEvents.IsChecked = Properties.Settings.Default.showJoinLeave;
        }

        private void linkGenerate_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://www.twitchapps.com/tmi");
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (tbNick.Text.Equals("") || tbOauth.Text.Equals("") || tbChannel.Text.Equals("") || tbOauth.Text.Substring(0, 6) != "oauth:")
            {
                MessageBox.Show("Invalid input!");
                return;
            }

            DialogResult = true;

            Properties.Settings.Default.nick = tbNick.Text;
            Properties.Settings.Default.oauth = tbOauth.Text;
            Properties.Settings.Default.channel = tbChannel.Text;
            Properties.Settings.Default.debug = (bool)cbDebug.IsChecked;
            Properties.Settings.Default.showJoinLeave = (bool)cbShowEvents.IsChecked;
            Properties.Settings.Default.Save();

            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false || clearedCache;
            Close();
        }

        private void btnClearCache_Click(object sender, RoutedEventArgs e)
        {
            EmoticonManager.clearCache();
            clearedCache = true;
            MessageBox.Show(this, "Cache cleared.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            UpdateManager manager = new UpdateManager(new Uri("http://newcat.bplaced.net/tvd/updates.json"), "<RSAKeyValue><Modulus>8l5adrCGpcd1yzQ6gTORp/zUDwl/jxtPW3g3dqFgEFpjS9ZFW05YA6/GdGMyU0nJQ6mdnd6j3fmqYsXSPsfG8iQmsCYvjEj/2eHUl5u3I7alxsqTViifuI1pm4hJ6A5+vVU4cCW7o8twzr1tgWElvIA/AOLLADu7PfeWmH+RwQaO5uGnIklmE8A4Qsz0IBr+0BvXwn9cgj7N6jw7hQNWGnb4bOU7yrVD64J1j9GxXuYS0GRjmddtYk67EaIb8/eHW1XzzMwPiOFVb+n/S4jnfQ11bYbq35K7ruklEnWVCg21JuB+i5D5ho746RXnlLFfNcN4wi5W0X0iadwJChlN64D12QtWZtkgUacte0vlwUZojvQXVKCSRqTXQjExZwA5eyptumjtYn1firqzsRvSjeqEtK/WXFL/ArGlwteJMGIh1OLfgXsSyd0CLvAtiE3TKWsIB1ZsT1ut5Bk304cukSWtKp//ixislc00z/w/NXPSQfaNEC2jE+7fKZ4lu92tLnVVmwGT7sVLUj/hrrhwIX3biu07d5wVNvc6uXLosF7jnPrmAX2XdwghiaN77uyLVfIeUJ2RGMHfF8lVnNCyOORl+R/L1Sxww09jk1nRLWGn8ZhiRHt8t5Mj7E6cZl9kV+TBxNPkTblpZbgdZOe4HThy3NHZU9zJh7+KtNiCsziqk+iySY/ew75uHgRUYWpAncloa7rYzFkvySTBPkpSM9rqnMEmB7c1EM9XX3x+OqC+y25Viop7e7xRGYPnYaolCbiJMNdKT82fIMQEU8iQ5cvboirLd9u8pWzItkIUlg/HPaZeX0r5Nhu4ZeeduHie3OoTeS6RZ0GWZoEmgNotpzc6iSDrH5QZ1zb+LuBW+t+oi1Tyeuy7fIEPV9dkB6/ibMdUVp/qMuzPzDeJdMrNiU+VHZiulrVr2v2RUoU3Eom8pZWWWgEqwsc5FZ6/LWtLWtYBQ3mSaPWS/ilTY6DAAI0ScMU+/ahYgMk9pmexUiOc8TJDtVn4Q5DzdAmO5atyiBeFgokq8BvNnwLJRRLODF7n+qoU9eMMOWVujE61a+NqbM7bkYutF9rNH+U4F8RGz/aDOPq+m2FYcomKLXE6Hgk47DkB2Q3An2ou4Xvw5PdvmBYVAp4brR9dJ1JhsZml5J8ukXYp+XqBdTlghW/FhAIfmbF32EAzoHpa/oHV2IOr2IEGpARKj2pJAIW4oMEhqYCPV+ZEA56h78YVVJKqVeghhZGrmbhLLfe+tx0BcN/YyEvbDhwjkr8nD6QXVH239ziPiszlnJ0kxGP+dnaobYJd9pADXF9qxAC4n5RIaUT587FHJeW//oEfaPXQ5OnYUcTUX+xJbeAwvCwsEcTRbQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>", new CultureInfo("en"));
            manager.CloseHostApplication = true;
            var updaterUI = new UpdaterUI(manager, System.Threading.SynchronizationContext.Current);
            updaterUI.ShowUserInterface();
        }
    }
}
