using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BuildMon
{
    public class DelegateCommand : ICommand
    {
        private Action _executeAction;
        private Func<bool> _canExectute;

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