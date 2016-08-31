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

namespace Aoba.Views
{
    /// <summary>
    /// TwitterAuthWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TwitterAuthWindow : Window
    {
        public TwitterAuthWindow()
        {
            InitializeComponent();

            DataContext = new ViewModels.TwitterAuthWindowViewModel();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
