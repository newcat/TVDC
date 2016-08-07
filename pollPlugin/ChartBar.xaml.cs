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
using System.Windows.Media.Animation;

namespace pollPlugin
{
    /// <summary>
    /// Interaktionslogik für ChartBar.xaml
    /// </summary>
    public partial class ChartBar : UserControl
    {

        public ChartBar(Color hsColor, Color lsColor)
        {
            InitializeComponent();
            LinearGradientBrush lgb = new LinearGradientBrush(lsColor, hsColor, 0);
            rect.Fill = lgb;
        }

        public void setValue(double height)
        {
            DoubleAnimation da = new DoubleAnimation(height, TimeSpan.FromSeconds(0.5));
            da.AccelerationRatio = 0.3;
            da.DecelerationRatio = 0.3;

            rect.BeginAnimation(HeightProperty, da);
        }

    }
}
