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
	class EditClassifiedAdViewModel : ObservableObject
	{

		private ClassifiedAd _ad;

		public ClassifiedAd Ad
		{
			get
			{
				return _ad;
			}
		}

		public EditClassifiedAdViewModel(ClassifiedAd ad)
		{
			_ad = ad;

			var lst = new List<int>();
			for (int i = 1; i <= 52; i++)
			{
				lst.Add(i);
			}
			_allIssues = lst;

			lst = new List<int>();
			lst.Add(DateTime.Now.Year - 1);
			lst.Add(DateTime.Now.Year);
			lst.Add(DateTime.Now.Year + 1);
			_allYears = lst;
		}

		private void SetOption(ClassifiedAd.OptionsFlags opt, bool? isSet)
		{
			if (isSet.Value) _ad.Options |= opt; else _ad.Options &= ~opt;
		}

		public bool? IsBorder
		{
			get
			{
				return _ad.Options.HasFlag(ClassifiedAd.OptionsFlags.IsBorder);
			}
			set
			{
				SetOption(ClassifiedAd.OptionsFlags.IsBorder, value);
			}
		}

		public bool? IsBackground
		{
			get
			{
				return _ad.Options.HasFlag(ClassifiedAd.OptionsFlags.IsBackground);
			}
			set
			{
				SetOption(ClassifiedAd.OptionsFlags.IsBackground, value);
			}
		}

		public bool? IsBold
		{
			get
			{
				return _ad.Options.HasFlag(ClassifiedAd.OptionsFlags.IsBold);
			}
			set
			{
				SetOption(ClassifiedAd.OptionsFlags.IsBold, value);
			}
		}

		public bool? IsCentered
		{
			get
			{
				return _ad.Options.HasFlag(ClassifiedAd.OptionsFlags.IsCentered);
			}
			set
			{
				SetOption(ClassifiedAd.OptionsFlags.IsCentered, value);
			}
		}


		public IEnumerable<ClassifiedRubric> AllRubrics
		{
			get
			{
				return ClassifiedRubric.All;
			}
		}


		public IEnumerable<string> AllSM
		{
			get
			{
				//var all = new List<string>(ClassifiedSM.All);
				//if (!all.Contains(_ad.SM)) all.Add(_ad.SM);
				return ClassifiedSM.All;
			}
		}

		public ICommand AddSMCommand => new DelegateCommand<Window>((w) => {
			w.Close();
			new NewSMWindow() { Owner = Application.Current.MainWindow }.ShowDialog();
		});

		public ICommand RemoveSMCommand => new DelegateCommand<Window>((w) => {

			if (!AllSM.Contains(_ad.SM)) return;
			if (MessageBox.Show($"Удалить значение \"{_ad.SM}\"?", "Удалим-ка", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
				return;
			ClassifiedSM.Remove(_ad.SM);
			w.Close();
			//RaisePropertyChangedEvent("AllSM");
		});

		private IEnumerable<int> _allIssues;
		public IEnumerable<int> AllIssues
		{
			get
			{
				return _allIssues;
			}
		}

		private IEnumerable<int> _allYears;
		public IEnumerable<int> AllYears
		{
			get
			{
				return _allYears;
			}
		}


		public IEnumerable<ClassifiedAd.PaymentMethodEnum> AllPaymentMethods
		{
			get
			{
				return Enum.GetValues(typeof(ClassifiedAd.PaymentMethodEnum)).Cast<ClassifiedAd.PaymentMethodEnum>();
			}
		}

		private bool ValidateAd()
		{
			if (Ad.Advertiser == null)
			{
				MessageBox.Show("Пожалуйста, укажите рекламодателя.", "Сюда нельзя!", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			if (Ad.Rubric == null)
			{
				MessageBox.Show("Пожалуйста, укажите рубрику.", "Сюда нельзя!", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			if (string.IsNullOrWhiteSpace(Ad.Text))
			{
				MessageBox.Show("Пожалуйста, укажите текст объявления.", "Сюда нельзя!", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			return true;
		}

		public ICommand SaveCommand
		{
			get
			{
				return new DelegateCommand<Window>((w) => {

					if (ValidateAd())
					{
						Ad.Save();
						w.Close();
                        MainViewModel.Instance.RefreshClassified(true);
                    }
				});
			}
		}

		public ICommand CancelCommand
		{
			get
			{
				return new DelegateCommand<Window>((w) => {
                    Ad.Revert();
					w.Close();
                    MainViewModel.Instance.RefreshClassified(true);
				});
			}
		}

	}
}
