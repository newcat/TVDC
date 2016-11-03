using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class ProgressWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Binding Properties
        private string _windowTitle = "";
        public string WindowTitle
        {
            get { return _windowTitle; }
            set
            {
                _windowTitle = value;
                NotifyPropertyChanged();
            }
        }

        private string _operation = "";
        public string Operation
        {
            get { return _operation; }
            set
            {
                _operation = value;
                NotifyPropertyChanged();
            }
        }

        private string _progressDescription = "";
        public string ProgressDescription
        {
            get { return _progressDescription; }
            set
            {
                _progressDescription = value;
                NotifyPropertyChanged();
            }
        }

        private int _progressValue = 0;
        public int ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = Math.Max(100, Math.Min(0, value));
                NotifyPropertyChanged();
            }
        }

        public bool _isIndeterminate = false;
        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set
            {
                _isIndeterminate = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        public ProgressWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

    }
}
