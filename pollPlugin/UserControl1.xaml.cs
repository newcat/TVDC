using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Drawing;
using tvdc;

namespace pollPlugin
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : Window, IPlugin
    {

        public UserControl1()
        {
            Assembly.LoadFrom("plugin.dll");
            InitializeComponent();
        }

        public string pluginName
        {
            get
            {
                return "Poll Plugin";
            }
        }

        public event EventHandler<SendMessageEventArgs> sendMessage;

        public Image getMenuIcon()
        {

            Bitmap bmp = new Bitmap(28, 28);
            Graphics g = Graphics.FromImage(bmp);
            g.FillEllipse(Brushes.Yellow, 0, 0, 28, 28);
            g.Flush();
            g.Dispose();
            return bmp;

        }

        public Image getMenuIconHover() {

            Bitmap bmp = new Bitmap(28, 28);
            Graphics g = Graphics.FromImage(bmp);
            g.FillEllipse(Brushes.AliceBlue, 0, 0, 28, 28);
            g.Flush();
            g.Dispose();
            return bmp;

        }

        public void IconClicked() { }

        public void IRC_Connected() { }

        public void IRC_ConnectionError() { }

        public void IRC_InitCompleted() { }

        public void IRC_Join(string username) { }

        public void IRC_MessageReceived(string username, string message, Dictionary<string, string> tags)
        {
            throw new NotImplementedException();
        }

        public void IRC_ModeChanged(string username, bool isMod) { }

        public void IRC_Part(string username) { }

        public void IRC_Userstate(Dictionary<string, string> tags) { }
    }
}
