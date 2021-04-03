using ConnectionManster.UI.PC.ViewModels;
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

namespace ConnectionManster.UI.PC
{
    /// <summary>
    /// IPEndPointWindow.xaml 的交互逻辑
    /// </summary>
    [ViewModel(typeof(IPEndPointViewModel))]
    public partial class IPEndPointWindow : Window
    {
        public IPEndPointWindow()
        {
            InitializeComponent();
        }

        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (IPEndPointViewModel)e.NewValue;
            if(viewModel == null)
            {
                return;
            }
            viewModel.SaveSuccess += (s, e) => Close();
        }
    }
}
