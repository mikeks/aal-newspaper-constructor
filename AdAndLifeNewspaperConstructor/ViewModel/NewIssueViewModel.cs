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
                PageCount = iss.Pages.Length;
                var classFirst = 0;
                var classLast = 0;
				foreach (var p in iss.Pages)
				{
                    if (p.Grid.IsForClassified) {
                        if (classFirst == 0) classFirst = p.Number;
                        if (p.Number > classLast) classLast = p.Number;
                    }
				}
                ClassifiedFrom = classFirst;
                ClassifiedTo = classLast;
            }

            RaisePropertyChangedEvent("Year");
            RaisePropertyChangedEvent("Number");
            RaisePropertyChangedEvent("PageCount");
            RaisePropertyChangedEvent("ClassifiedFrom");
            RaisePropertyChangedEvent("ClassifiedTo");

        }


        public int Year { get; set; }
        public int Number { get; set; }
        
        public int PageCount { get; set; }
        public int ClassifiedFrom { get; set; }
        public int ClassifiedTo { get; set; }

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
                        Issue.CreateIssue(Year, Number, (byte)PageCount, (byte)ClassifiedFrom, (byte)ClassifiedTo);
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
