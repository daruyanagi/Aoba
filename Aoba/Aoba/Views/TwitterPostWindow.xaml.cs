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
    /// TwitterPostWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TwitterPostWindow : Window
    {
        public TwitterPostWindow() : base()
        {
            InitializeComponent();

            DataContext = new ViewModels.TwitterPostWindowViewModel();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
