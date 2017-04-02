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
    class NewIssueViewModel: ObservableObject
    {

        public NewIssueViewModel()
        {
            var iss = Issue.GetAllIssues().LastOrDefault();
            if (iss != null)
            {
                Number = iss.Number + 1;
                Year = iss.Year;
                //PageCount = iss.Pages.Length;
            }

            RaisePropertyChangedEvent("Year");
            RaisePropertyChangedEvent("Number");
            //RaisePropertyChangedEvent("PageCount");

        }


        public int Year { get; set; }
        public int Number { get; set; }
       // public int PageCount { get; set; }

        public string NewspaperName
        {
            get
            {
                return "Газета: " + NewspaperDatabase.Current.NewspaperName;
            }
        }

        public ICommand CreateIssueCommand
        {
            get
            {
                return new DelegateCommand<System.Windows.Window>((w) =>
                {

                    try
                    {
                        Issue.CreateIssue(Year, Number);
                    }
                    catch 
                    {
                        MessageBox.Show("Не удалось создать выпуск газеты с такими параметрами. Может быть он уже существует.", "Ошибочка вышла", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    w.Close();
                    MainViewModel.Instance.RefreshIssues();
                });
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand<System.Windows.Window>((w) =>
                {
                    w.Close();
                    MainViewModel.Instance.RefreshIssues();
                });
            }
        }

    }
}
