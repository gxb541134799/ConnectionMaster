using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ConnectionManster.UI.PC.Commands
{
    public class Command : ICommand
    {
        private Action executeAction;

        public Command([NotNull]Action executeAction, Func<bool> canExecuteAction = null)
        {
            this.executeAction = executeAction;
            this.canExecuteAction = canExecuteAction;
        }

        private Func<bool> canExecuteAction;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute()
        {
            return canExecuteAction?.Invoke() ?? true;
        }

        bool ICommand.CanExecute(object parameter)
        {
            return this.CanExecute();
        }

        public void Execute()
        {
            executeAction();
        }

        void ICommand.Execute(object parameter)
        {
            this.Execute();
        }

        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class Command<T> : ICommand
    {
        private Action<T> executeAction;
        private Func<T, bool> canExecuteAction;

        public Command([NotNull]Action<T> executeAction, Func<T, bool> canExecuteAction = null)
        {
            this.executeAction = executeAction;
            this.canExecuteAction = canExecuteAction;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(T parameter)
        {
            return canExecuteAction?.Invoke(parameter) ?? true;
        }

        bool ICommand.CanExecute(object parameter)
        {
            return this.CanExecute((T)parameter);
        }

        public void Execute(T parameter)
        {
            executeAction(parameter);
        }

        void ICommand.Execute(object parameter)
        {
            this.Execute((T)parameter);
        }

        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
