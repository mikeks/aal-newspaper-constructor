using InDesign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VitalConnection.AAL.Builder.Model;

namespace VitalConnection.AAL.Builder.ViewModel
{
    class NewSmViewModel: ObservableObject
    {

        public NewSmViewModel()
        {
        }

        public string Name { get; set; }

        public ICommand AddNewSmCommand => new DelegateCommand<System.Windows.Window>((w) =>
        {
			ClassifiedSM.Add(Name);
            w.Close();
        });

        public ICommand CancelCommand => new DelegateCommand<System.Windows.Window>((w) =>
        {
            w.Close();
        });

    }
}
