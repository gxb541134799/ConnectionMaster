using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ConnectionManster.UI.PC
{
    public static class Notify
    {
        private static Dictionary<Type, Type> viewModelWindows = new Dictionary<Type, Type>();

        static Notify()
        {
            var windowType = typeof(Window);
            viewModelWindows = typeof(Notify).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(windowType))
                .Select(t => new
                {
                    WindowType = t,
                    ViewModelType = t.GetCustomAttribute<ViewModelAttribute>()?.ViewModelType
                })
                .Where(e => e.ViewModelType != null)
                .ToDictionary(e => e.ViewModelType, e => e.WindowType);
        }

        public static void ShowError(string message,string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowInformation(string message,string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowViewModel<T>(T viewModel)
        {
            Type windowType;
            if(!viewModelWindows.TryGetValue(typeof(T),out windowType))
            {
                return;
            }
            var window = (Window)Activator.CreateInstance(windowType);
            window.DataContext = viewModel;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.ShowDialog();
        }
    }
}
