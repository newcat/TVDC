using System.Diagnostics;
using System.ComponentModel;

namespace System.Windows.Input
{
    public class RelayCommand : CommandBase
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
        [DebuggerStepThrough()]
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException("execute");
            this._execute = execute;
            _canExecute = canExecute ?? (() => this.Enabled);
        }
        [DebuggerStepThrough()]
        public override bool CanExecute(object parameter) { return _canExecute(); }
        [DebuggerStepThrough()]
        public override void Execute(object parameter) { this._execute(); }
    }

    public class RelayCommand<T> : CommandBase
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;
        [DebuggerStepThrough()]
        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException("execute");
            this._execute = execute;
            _canExecute = canExecute;
        }
        [DebuggerStepThrough()]
        public override bool CanExecute(object parameter) { return _canExecute == null ? this.Enabled : _canExecute((T)parameter); }
        [DebuggerStepThrough()]
        public override void Execute(object parameter) { this._execute((T)parameter); }
    }

    public abstract class CommandBase : NotifyPropertyChanged, ICommand
    {

        //verhindert Ereigniskette beim gegenseitigen Aufruf von CanExecute und Setter der Enabled-Property
        private int EventDisabled = 0;
        public static readonly PropertyChangedEventArgs EnabledChangedArgs = new PropertyChangedEventArgs("Enabled");
        private bool _Enabled = true;

        public abstract void Execute(object parameter);
        public abstract bool CanExecute(object parameter);

        //das CanExecuteChanged-Event vereinigt sich mit dem statischen CommandManager.RequerySuggested-Event. Eiglich müsste es LaunchRequery heißen oderso 
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool Enabled
        {
            get { return _Enabled; }
            set
            {
                if (ChangePropIfDifferent(value, ref _Enabled, EnabledChangedArgs))
                {
                    if (EventDisabled++ == 0) CommandManager.InvalidateRequerySuggested();
                    EventDisabled--;
                }
            }
        }
        bool ICommand.CanExecute(object parameter)
        {
            if (EventDisabled++ == 0) Enabled = CanExecute(parameter);
            EventDisabled--;
            return _Enabled;
        }
    }
}