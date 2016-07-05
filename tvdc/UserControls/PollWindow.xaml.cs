using System.Windows;

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
