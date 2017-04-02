using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Linq;
using System.Reflection;
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

namespace VitalConnection.AAL.Builder
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            tbVersion.Text = $"Версия {v.Major}.{v.Minor} сборка №{v.Revision}";
            //tbVersion.Text = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
