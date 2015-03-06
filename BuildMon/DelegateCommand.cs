using System;
using System.Windows.Input;

namespace BuildMon
{
    public class DelegateCommand : ICommand
    {
        private readonly Func<bool> _canExectute;
        private readonly Action _executeAction;

        public DelegateCommand(Action executeAction, Func<bool> canExecute)
        {
            _executeAction = executeAction;
            _canExectute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExectute == null)
                return false;
            return _canExectute();
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (_executeAction == null)
                return;
            _executeAction();
        }
    }
}