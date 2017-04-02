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
using VitalConnection.AAL.Builder.ViewModel;

namespace VitalConnection.AAL.Builder
{
	/// <summary>
	/// Interaction logic for NewAdWindow.xaml
	/// </summary>
	public partial class NewIssueWindow : Window
	{
		public NewIssueWindow()
		{
			InitializeComponent();
            DataContext = new NewIssueViewModel();
		}
	}
}
