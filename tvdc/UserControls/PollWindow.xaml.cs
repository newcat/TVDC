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

namespace tvdc.UserControls
{
    /// <summary>
    /// Interaction logic for PollWindow.xaml
    /// </summary>
    public partial class PollWindow : Window
    {

        public bool isOpen { get; private set; }

        public PollWindow()
        {
            InitializeComponent();
        }

        public new void Show()
        {
            base.Show();
            isOpen = true;
        }



    }
}
