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
using System.Windows.Controls;
using System.Reflection;
using tvdc.Plugin;
using System.Windows.Media;
using System.Drawing;
using System.IO;

namespace pollPlugin
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl, IPlugin
    {

        private IPluginHost host;

        public UserControl1()
        {
            Assembly.LoadFrom("plugin.dll");
            InitializeComponent();
        }

        public void Initialize(IPluginHost host)
        {
            this.host = host;
        }

        public string pluginName
        {
            get { return "Poll Plugin"; }
        }

        public ImageSource getMenuIcon()
        {

            Bitmap bmp = new Bitmap(28, 28);
            Graphics g = Graphics.FromImage(bmp);
            g.FillEllipse(System.Drawing.Brushes.Yellow, 0, 0, 28, 28);
            g.Flush();

            return BmpToImg(bmp);

        }

        public ImageSource getMenuIconHover() {

            Bitmap bmp = new Bitmap(28, 28);
            Graphics g = Graphics.FromImage(bmp);
            g.FillEllipse(System.Drawing.Brushes.AliceBlue, 0, 0, 28, 28);
            g.Flush();

            return BmpToImg(bmp);

        }

        public void IconClicked()
        {
            MessageBox.Show("PollPlugin");
        }

        private BitmapImage BmpToImg(Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.MemoryBmp);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }
        
    }
}
