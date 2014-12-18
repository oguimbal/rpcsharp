using System;
using System.Windows.Input;

namespace Client
{
    public class ActionCommand : ICommand
    {
        readonly Action _action;

        public ActionCommand(Action action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action();
        }

        public event EventHandler CanExecuteChanged;
    }    

    public class ActionCommand<T> : ICommand
        where T:class
    {
        readonly Action<T> _action;

        public ActionCommand(Action<T> action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return parameter is T;
        }

        public void Execute(object parameter)
        {
            _action(parameter as T);
        }

        public event EventHandler CanExecuteChanged;
    }
}