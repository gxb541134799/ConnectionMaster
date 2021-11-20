using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ConnectionManster.UI.PC.Commands
{
    public class AsyncCommand : ICommand
    {
        private readonly Func<Task> executeHandler;
        private readonly Func<bool> canExecuteHandler;
        private bool executing = false;

        public AsyncCommand(Func<Task> executeHandler, Func<bool> canExecuteHandler = null)
        {
            this.executeHandler = executeHandler;
            this.canExecuteHandler = canExecuteHandler;
        }

        public event EventHandler CanExecuteChanged;

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }

        async void ICommand.Execute(object parameter)
        {
            await ExecuteAsync();
        }

        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute()
        {
            return !executing && (canExecuteHandler?.Invoke() ?? true);
        }

        public async Task ExecuteAsync()
        {
            if (!CanExecute())
            {
                return;
            }
            executing = true;
            OnCanExecuteChanged();
            try
            {
                await executeHandler();
            }
            finally
            {
                executing = false;
            }
            OnCanExecuteChanged();
        }

        public async void Execute()
        {
            await ExecuteAsync();
        }
    }

    public class AsyncCommand<T> : ICommand
    {
        private readonly Func<T, Task> executeHandler;
        private readonly Func<T, bool> canExecute;
        private bool executing = false;

        public AsyncCommand(Func<T, Task> executeHandler, Func<T, bool> canExecute = null)
        {
            this.executeHandler = executeHandler;
            this.canExecute = canExecute;
        }

        public bool CanExecute(T parameter)
        {
            return !executing && (canExecute?.Invoke(parameter) ?? true);
        }

        public async Task ExecuteAsync(T parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }
            executing = true;
            OnCanExecuteChanged();
            try
            {
                await executeHandler(parameter);
            }
            finally
            {
                executing = false;
            }
            OnCanExecuteChanged();
        }

        public async void Execute(T parameter)
        {
            await ExecuteAsync(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (parameter == null && typeof(T).IsValueType)
            {
                return true;
            }
            return CanExecute((T)parameter);
        }

        async void ICommand.Execute(object parameter)
        {
            await ExecuteAsync((T)parameter);
        }
    }
}
