using VitalConnection.AAL.Builder.Model;
using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using VitalConnection.AAL.Builder.ViewModel;
using InDesign;
using VitalConnection.AAL.Builder.IndesignExport;

namespace VitalConnection.AAL.Builder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
	{
        public object InDesignConverter { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
			DataContext = new MainViewModel(PageSelectionStackPanel, PageLayoutGrid);
        }

        private MainViewModel ViewModel => DataContext as MainViewModel;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

		private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{

		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{



		}

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AdModulesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.OnAdModulesSelected(AdModulesList.SelectedItems.Cast<AdModule>());
        }
    }

 

}
