﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VitalConnection.AAL.Builder.AdModulesManagement;
using VitalConnection.AAL.Builder.IndesignExport;
using VitalConnection.AAL.Builder.Model;
using VitalConnection.AAL.Builder.Model.Articles;
using VitalConnection.AAL.Builder.QuickBook;

namespace VitalConnection.AAL.Builder.ViewModel
{
	public class MainViewModel : ObservableObject
	{

		public NewspaperPage CurrentPage { get; set; }

		private StackPanel PageSelectionStackPanel { get; set; }

		readonly AdsOnPageManager _pageLayout;

		private static MainViewModel _instance;
		public static MainViewModel Instance
		{
			get
			{
				return _instance;
			}
		}


		public MainViewModel(StackPanel pageSelectionStackPanel, Grid pageLayoutGrid)
		{
			CreateAdModuleView();

			ClassifiedView = CollectionViewSource.GetDefaultView(ClassifiedAd.All);
			ClassifiedView.Filter = ClassifiedFilter;

			_pageLayout = new AdsOnPageManager(this, pageLayoutGrid);
			PageSelectionStackPanel = pageSelectionStackPanel;
			if (Issues.Count() > 0) CurrentIssue = Issues.Last();
			_instance = this;
		}

		private void CreateAdModuleView()
		{
			AdModuleView = CollectionViewSource.GetDefaultView(AdModule.AllModules); // .OrderBy((m) => m.Name)
			AdModuleView.Filter = AdModuleFilter;
			AdModuleView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
		}

		public void RefreshAdModules()
		{
			AdModule.ReloadAllFromDb();
			CreateAdModuleView();
			RaisePropertyChangedEvent("AdModuleView");
			RaisePropertyChangedEvent("IssueAdModules");
			SelectedAdModules = null;
			SelectedAdModule = null;
		}

		public void RefreshAdvertizers()
		{
			Advertizer.ReloadAllFromDb();
			RaisePropertyChangedEvent("AllAdvertisers");
		}

		public void RefreshIssues()
		{
			RaisePropertyChangedEvent("Issues");
			if (Issues != null && Issues.Count() > 0) CurrentIssue = Issues.Last();
		}


		Advertizer _adModuleAdvertizerFilter;
		public Advertizer AdModuleAdvertizerFilter
		{
			get
			{
				return _adModuleAdvertizerFilter;
			}
			set
			{
				if (_adModuleAdvertizerFilter == value) return;
				_adModuleAdvertizerFilter = value;
				if (IsAdModuleAdvertizerFilterOn)
					AdModuleView.Refresh();
				else
					IsAdModuleAdvertizerFilterOn = true;
			}
		}

		private bool AdModuleFilter(object item)
		{
			var l = item as AdModule;
			if (IsAdModuleMustFitThePage && CurrentPage != null && l.Grid.Id != CurrentPage.Grid.Id) return false;

			if (IsAdModuleAdvertizerFilterOn && AdModuleAdvertizerFilter != null && l.Advertiser != AdModuleAdvertizerFilter) return false;

			if (string.IsNullOrWhiteSpace(FilterString)) return true;
			return l.Name.ToLower().Contains(FilterString.ToLower().Trim());
		}

		bool _isAdModuleMustFitThePage = true;
		public bool IsAdModuleMustFitThePage
		{
			get
			{
				return _isAdModuleMustFitThePage;
			}
			set
			{
				if (_isAdModuleMustFitThePage == value) return;
				_isAdModuleMustFitThePage = value;
				AdModuleView.Refresh();
			}
		}

		bool _isAdModuleAdvertizerFilterOn;
		public bool IsAdModuleAdvertizerFilterOn
		{
			get
			{
				return _isAdModuleAdvertizerFilterOn;
			}
			set
			{
				if (_isAdModuleAdvertizerFilterOn == value) return;
				_isAdModuleAdvertizerFilterOn = value;
				AdModuleView.Refresh();
				RaisePropertyChangedEvent("IsAdModuleAdvertizerFilterOn");
			}
		}



		public ICollectionView AdModuleView { get; private set; }

		bool _isAdModuleTextFilterOn;
		public bool IsAdModuleTextFilterOn
		{
			get
			{
				return _isAdModuleTextFilterOn;
			}
			set
			{
				if (_isAdModuleTextFilterOn == value) return;
				_isAdModuleTextFilterOn = value;
				if (!_isAdModuleTextFilterOn) FilterString = null;
				RaisePropertyChangedEvent("IsAdModuleTextFilterOn");
			}
		}

		private string _filterString;

		public string FilterString
		{
			get { return _filterString; }
			set
			{
				if (_filterString == value) return;
				_filterString = value;

				IsAdModuleTextFilterOn = true;

				RaisePropertyChangedEvent("FilterString");
				AdModuleView.Refresh();
			}
		}

		public string GridTypeName
		{
			get
			{
				return "Тип страницы: " + CurrentPage?.Grid.Name;
			}
		}


		private AdModuleOnPage _selectedAdModuleOnPage;

		public AdModuleOnPage SelectedAdModuleOnPage
		{
			get
			{
				return _selectedAdModuleOnPage;
			}
			set
			{
				_selectedAdModuleOnPage = value;
				if (value != null) SelectedAdModule = value.AdModule;
			}
		}


		public string AdModulesMultipleSelectionInfo => $"Выделено {_selectedAdModules?.Count()} рекламных объявлений";
		public Visibility AdModuleMultipleSelectionVisibility => _selectedAdModules != null ? Visibility.Visible : Visibility.Collapsed;

		IEnumerable<AdModule> _selectedAdModules;
		public IEnumerable<AdModule> SelectedAdModules
		{
			get
			{
				return _selectedAdModules;
			}
			set
			{
				if (_selectedAdModules == value) return;
				_selectedAdModules = value;
				RaisePropertyChangedEvent("AdModulesMultipleSelectionInfo");
				RaisePropertyChangedEvent("AdModuleMultipleSelectionVisibility");
			}
		}

		public void OnAdModulesSelected(IEnumerable<AdModule> selectedAdModules)
		{
			if (selectedAdModules.Count() == 1)
			{
				SelectedAdModule = selectedAdModules.First();
				SelectedAdModules = null;
			}
			else
			{
				SelectedAdModule = null;
				SelectedAdModules = selectedAdModules;
			}
		}


		AdModule _selectedAdModule;

		private void RefreshAdModuleInformation()
		{
			RaisePropertyChangedEvent("AdModuleImage");
			RaisePropertyChangedEvent("AdModuleName");
			RaisePropertyChangedEvent("AdModuleSize");
			RaisePropertyChangedEvent("AddToPageCommandVisibility");
			RaisePropertyChangedEvent("DeleteCommandVisibility");
			RaisePropertyChangedEvent("DeleteAdModuleCommandVisibility");
			RaisePropertyChangedEvent("SelectedAdModule");
			RaisePropertyChangedEvent("VisibleIfAdModuleSelected");
			RaisePropertyChangedEvent("AdModuleAdvertiserName");
			RaisePropertyChangedEvent("AdModulePrice");
			RaisePropertyChangedEvent("AdModuleInfoVisibility");
		}

		public AdModule SelectedAdModule
		{
			get
			{
				return _selectedAdModule;
			}
			set
			{
				if (_selectedAdModule == value) return;
				_selectedAdModule = value;
				SelectedAdModules = null;
				RefreshAdModuleInformation();
			}
		}






		BitmapImage _selectedImage;
		string _selectedImagePath;

		private void LoadCurrentImage()
		{
			var ph = Utility.ConvertFilePath(_selectedAdModule.FullPath);
			if (_selectedImagePath == ph) return; // already loaded

			Task.Run(async () =>
			{
				_selectedImage = await ImageLoader.LoadImage(ph);
				_selectedImagePath = ph;
				Application.Current.Dispatcher.Invoke(() =>
				{
					RaisePropertyChangedEvent("AdModuleImage");
				});
			});

		}

		public string AdModuleName
		{
			get
			{
				return _selectedAdModule?.Name;
			}
		}

		public string AdModuleSize
		{
			get
			{
				if (_selectedAdModule == null) return null;
				return $"Размер: {_selectedAdModule.Width}x{_selectedAdModule.Height}";
			}
		}

		public string AdModuleAdvertiserName => $"Рекламодатель: {_selectedAdModule?.Advertiser?.Name ?? "не указан"}";

		public string AdModulePrice => $"Цена: {_selectedAdModule?.Price}";

		public IEnumerable<Advertizer> AllAdvertisers => Advertizer.All;

		//public Advertizer SelectedAdModuleOnPage
		//{
		//	get; set;
		//}


		public bool SelectAllAdModules
		{
			get
			{
				return IssueAdModules.All((x) => !x.CanSelect || x.IsSelected);
			}
			set
			{
				foreach (var m in IssueAdModules)
				{
					m.IsSelected = value;
				}
				RaisePropertyChangedEvent("IssueAdModules");
			}
		}

		public bool SelectAllInvoices
		{
			get
			{
				return Invoices.OfType<Invoice>().All((x) => x.IsSelected);
			}
			set
			{
				foreach (Invoice m in Invoices)
				{
					m.IsSelected = value;
				}
				RefreshInvoices(false);
			}
		}

		bool _showAllInvoices;
		public bool ShowAllInvoices
		{
			get
			{
				return _showAllInvoices;
			}
			set
			{
				_showAllInvoices = value;
				RefreshInvoices(false);
			}
		}


		//private void ResetIssueAdModules()
		//{
		//	_issueAdModules = null;
		//	RaisePropertyChangedEvent("IssueAdModules");
		//}

		//List<AdModuleOnPage> _issueAdModules;
		public IEnumerable<AdModuleOnPage> IssueAdModules
		{
			get
			{
				//if (_issueAdModules != null) return _issueAdModules;
				var _issueAdModules = new List<AdModuleOnPage>();
				foreach (var page in CurrentIssue.Pages)
				{
					foreach (var adModule in page.AdModules)
					{
						_issueAdModules.Add(adModule);
					}
				}
				return _issueAdModules;
			}
		}


		public ImageSource AdModuleImage
		{
			get
			{
				if (_selectedAdModule == null)
				{
					return null;
				}

				LoadCurrentImage();
				return _selectedImage;

			}
		}

		private bool IsCurrentAdModuleOnPage
		{
			get
			{
				if (CurrentPage == null || SelectedAdModule == null) return false;
				return CurrentPage.AdModules.FirstOrDefault((am) => am.AdModuleId == SelectedAdModule.Id) != null;
			}
		}





		public Visibility AdModuleInfoVisibility => _selectedAdModule != null ? Visibility.Visible : Visibility.Collapsed;

		public Visibility AddToPageCommandVisibility
		{
			get
			{
				return _selectedAdModule == null || IsCurrentAdModuleOnPage ? Visibility.Collapsed : Visibility.Visible;
			}
		}

		public Visibility DeleteCommandVisibility
		{
			get
			{
				return _selectedAdModule == null || !IsCurrentAdModuleOnPage ? Visibility.Collapsed : Visibility.Visible;
			}
		}

		public Visibility DeleteAdModuleCommandVisibility
		{
			get
			{
				return _selectedAdModule == null || IsCurrentAdModuleOnPage ? Visibility.Collapsed : Visibility.Visible;
			}
		}

		public Visibility VisibleIfAdModuleSelected
		{
			get
			{
				return _selectedAdModule == null ? Visibility.Collapsed : Visibility.Visible;
			}
		}


		public Visibility MoveCommandVisibility
		{
			get
			{
				return _selectedAdModule == null || !IsCurrentAdModuleOnPage ? Visibility.Collapsed : Visibility.Visible;
			}
		}


		public ICommand AddToPageCommand
		{
			get
			{
				return new DelegateCommand(async () =>
				{
					var amp = AdModuleOnPage.PlaceOnPage(SelectedAdModule, CurrentPage);
					await _pageLayout.RefreshAdModulesOnPage();
					SelectedAdModuleOnPage = amp;
					RefreshAdModuleInformation();
				});
			}
		}


		public ICommand DeleteCommand
		{
			get
			{
				return new DelegateCommand(async () =>
				{
					if (SelectedAdModuleOnPage == null) return;
					SelectedAdModuleOnPage.Delete();
					await _pageLayout.RefreshAdModulesOnPage();
					SelectedAdModuleOnPage = null;
					RefreshAdModuleInformation();
					//SelectedAdModule = null;
				});
			}
		}

		public ICommand DeleteAdModuleCommand => new DelegateCommand(() =>
		{
			if (SelectedAdModule == null) return;
			if (MessageBox.Show("Удалить рекламное объявление?", "Подтвердите", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) return;
			SelectedAdModule.Delete();
			RefreshAdModules();
		});

		public ICommand DeleteAdModulesCommand => new DelegateCommand(() =>
		{
			if (SelectedAdModules == null) return;
			if (MessageBox.Show($"Удалить рекламные объявления в количестве {SelectedAdModules.Count()} штук?", "Уверены?", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) return;
			foreach (var adm in SelectedAdModules)
			{
				adm.Delete();
			}
			RefreshAdModules();
		});


		public ICommand EditAdModuleCommand
		{
			get
			{
				return new DelegateCommand(() =>
				{
					if (SelectedAdModule == null) return;
					new EditAdModuleWindow(SelectedAdModule)
					{
						Owner = Application.Current.MainWindow
					}.ShowDialog();
				});
			}
		}



		//public ICommand MoveCommand
		//{
		//	get
		//	{
		//		return new DelegateCommand(async () => {
		//			AdModuleOnPage.PlaceOnPage(SelectedAdModule, CurrentPage);
		//			await _pageLayout.RefreshAdModulesOnPage();
		//		});
		//	}
		//}


		//string _status;
		//public string Status
		//{
		//	get
		//	{
		//		return _status;
		//	}

		//	set
		//	{
		//		_status = value;
		//		RaisePropertyChangedEvent("Status");
		//	}
		//}




		public void FillPageSelectionStackPanel()
		{
			StackPanel sp = PageSelectionStackPanel;

			sp.Children.Clear();

			Button firstBtn = null;

			foreach (var p in CurrentIssue.Pages)
			{
				var b = new Button
				{
					Content = p,
					Padding = new Thickness(10, 0, 10, 0)
				};
				_defaultButtonBorderBrush = b.BorderBrush;
				//b.BorderThickness = new Thickness(2);
				b.Click += OnPageSelected;

				var m = new ContextMenu();

				var mAdd = new MenuItem() { Header = "Добавить страницу" };

				var mAddBefore = new MenuItem() { Header = "Перед этой страницей" };
				mAddBefore.Click += (sender, e) =>
				{
					CurrentIssue.CreatePage(p.Number);
					FillPageSelectionStackPanel();
				};
				mAdd.Items.Add(mAddBefore);

				var mAddAfter = new MenuItem() { Header = "После этой страницы" };
				mAddAfter.Click += (sender, e) =>
				{
					CurrentIssue.CreatePage(p.Number + 1);
					FillPageSelectionStackPanel();
				};
				mAdd.Items.Add(mAddAfter);


				m.Items.Add(mAdd);
				//var mMove = new MenuItem() { Header = "Переместить..." };
				//mMove.Click += (sender, e) =>
				//{

				//    MessageBox.Show("Переместить на номер", "", MessageBoxButton.OK,)
				//};
				//m.Items.Add(mMove);


				var mTypes = new MenuItem() { Header = "Тип страницы" };

				foreach (var t in GridType.AllTypes)
				{
					var mType = new MenuItem() { Header = t.Name, IsCheckable = true, IsChecked = p.Grid.Id == t.Id };
					mType.Click += (sender, e) =>
					{
						p.Grid = t;
						FillPageSelectionStackPanel();
					};
					mTypes.Items.Add(mType);
				}

				m.Items.Add(mTypes);
				m.Items.Add(new Separator());

				var mDelete = new MenuItem() { Header = "Удалить" };
				mDelete.Click += (sender, e) =>
				{
					if (MessageBox.Show("Удалить страницу и все рекламы на ней? Отмена действия невозможна.", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
					{
						CurrentIssue.DeletePage(p.Number);
						FillPageSelectionStackPanel();
					}
				};
				m.Items.Add(mDelete);


				b.ContextMenu = m;

				switch (p.Grid.Id)
				{
					case GridIdEnum.First:
					case GridIdEnum.Last:
						b.Background = new SolidColorBrush(Colors.LightSkyBlue);
						break;
					case GridIdEnum.Classified:
						b.Background = new SolidColorBrush(Colors.Gold);
						break;
				}

				sp.Children.Add(b);
				if (firstBtn == null) firstBtn = b;

			}

			OnPageSelected(firstBtn, null);

		}


		Issue _currentIssue;
		public Issue CurrentIssue
		{
			get
			{
				return _currentIssue;
			}

			set
			{
				_currentIssue = value;
				FillPageSelectionStackPanel();
				RaisePropertyChangedEvent("CurrentIssue");
				RaisePropertyChangedEvent("CurrentIssueId");
				RefreshClassified(false);
				RefreshArticles();
				RaisePropertyChangedEvent("IssueAdModules");
				RefreshInvoices(false);

			}
		}

		public int CurrentIssueId
		{
			get
			{
				return _currentIssue?.Id ?? 0;
			}

			set
			{
				CurrentIssue = Issues.First((x) => x.Id == value);
			}
		}

		public NewspaperDatabase CurrentDatabase
		{
			get 
			{
				return NewspaperDatabase.Current;
			}

			set
			{
				NewspaperDatabase.Current = value;
				RefreshIssues();
				RefreshClassified(true);
				RefreshArticles();
				RefreshInvoices(true);
			}
		}




		public IEnumerable<Issue> Issues
		{
			get
			{
				return Issue.GetAllIssues();
			}
		}

		public IEnumerable<NewspaperDatabase> Databases
		{
			get
			{
				return NewspaperDatabase.All;
			}
		}

		private Button _selectedPageButton;
		private Brush _defaultButtonBorderBrush;

		private void OnPageSelected(object sender, RoutedEventArgs e)
		{
			if (_selectedPageButton != null)
			{
				_selectedPageButton.BorderThickness = new Thickness(1);
				_selectedPageButton.BorderBrush = _defaultButtonBorderBrush;
			}
			var b = (Button)sender;
			b.BorderThickness = new Thickness(2);
			b.BorderBrush = new SolidColorBrush(Colors.Blue);
			CurrentPage = (NewspaperPage)((Button)sender).Content;
			_pageLayout.RecreateGrid();
			_selectedPageButton = b;
			AdModuleView.Refresh();
			SelectedAdModules = null;
			SelectedAdModule = null;
			RaisePropertyChangedEvent("GridType");
		}

		bool _isClassifiedEndsNowFilter;
		public bool IsClassifiedEndsNowFilter
		{
			get
			{
				return _isClassifiedEndsNowFilter;
			}
			set
			{
				_isClassifiedEndsNowFilter = value;
				RefreshClassified(false);
			}
		}

		bool _isClassifiedMustBeInThisIssue;
		public bool IsClassifiedMustBeInThisIssue
		{
			get
			{
				return _isClassifiedMustBeInThisIssue;
			}
			set
			{
				_isClassifiedMustBeInThisIssue = value;
				RefreshClassified(false);
			}
		}

		public void RefreshClassified(bool reloadDb)
		{
			if (reloadDb)
			{
				ClassifiedAd.ReloadAllFromDb();
				ClassifiedView = CollectionViewSource.GetDefaultView(ClassifiedAd.All);
				ClassifiedView.Filter = ClassifiedFilter;
				RaisePropertyChangedEvent("ClassifiedView");
				RaisePropertyChangedEvent("AllRubrics");
			}
			else
			{
				ClassifiedView.Refresh();
			}
			RaisePropertyChangedEvent("ClassifiedCount");
			RaisePropertyChangedEvent("ClassifiedPrice");

		}

		private string _classifiedFilterString;
		public string ClassifiedFilterString
		{
			get { return _classifiedFilterString; }
			set
			{
				_classifiedFilterString = value;
				RefreshClassified(false);
			}
		}

		ClassifiedRubric _classifiedFilterRubric = _allRubricsEntity;
		public ClassifiedRubric ClassifiedFilterRubric
		{
			get
			{
				return _classifiedFilterRubric;
			}
			set
			{
				_classifiedFilterRubric = value;
				RefreshClassified(false);
			}
		}


		const string AllSmId = "<Все продавцы>";

		string _classifiedFilterSm = AllSmId;
		public string ClassifiedFilterSm
		{
			get
			{
				return _classifiedFilterSm;
			}
			set
			{
				_classifiedFilterSm = value;
				RefreshClassified(false);
			}
		}

		public IEnumerable<string> AllSalesmans
		{
			get
			{
				var lst = new List<string>();
				lst.Add(AllSmId);
				lst.AddRange(ClassifiedSM.All);
				return lst;
			}
		}

		private bool ClassifiedFilter(object item)
		{
			if (CurrentIssue == null) return false;
			var ad = item as ClassifiedAd;
			if (ClassifiedFilterRubric != null && ClassifiedFilterRubric != AllRubricsEntity && ad.Rubric.Id != ClassifiedFilterRubric.Id) return false;
			if (IsClassifiedEndsNowFilter)
			{
				if (!ad.IsEndsInNumber(CurrentIssue.Year, CurrentIssue.Number)) return false;
			}
			else if (IsClassifiedMustBeInThisIssue)
			{
				if (!ad.IsInNumber(CurrentIssue.Year, CurrentIssue.Number)) return false;
			}
			if (ClassifiedFilterSm != AllSmId && ClassifiedFilterSm != ad.SM) return false;
			if (string.IsNullOrWhiteSpace(ClassifiedFilterString)) return true;
			return ad.Advertiser.ToLower().Contains(ClassifiedFilterString.ToLower().Trim());
		}

		public ICollectionView ClassifiedView { get; private set; }

		private static readonly ClassifiedRubric _allRubricsEntity = new ClassifiedRubric("<все рубрики>");
		private ClassifiedRubric AllRubricsEntity => _allRubricsEntity;

		public IEnumerable<ClassifiedRubric> AllRubrics
		{
			get
			{
				var l = new List<ClassifiedRubric>
				{
					AllRubricsEntity
				};
				l.AddRange(ClassifiedRubric.All);
				return l;
			}
		}



		//public IEnumerable<ClassifiedAd> Classified
		//{
		//	get
		//	{
		//              return ClassifiedAd.GetAdsForIssue(CurrentIssue);
		//	}
		//}

		public int ClassifiedCount => ClassifiedView.OfType<object>().Count();
		public decimal ClassifiedPrice => ClassifiedView.OfType<ClassifiedAd>().Sum((x) => x.IssuePrice);

		private ClassifiedAd _selectedClassifiedAd;
		public ClassifiedAd SelectedClassifiedAd
		{
			get
			{
				return _selectedClassifiedAd;
			}
			set
			{
				_selectedClassifiedAd = value;
				RaisePropertyChangedEvent("DeleteClassifiedAdVisibility");
			}
		}


		private void ShowEditClassifiedAdWindow(ClassifiedAd ad)
		{
			new EditClassifiedAdWindow
			{
				DataContext = new EditClassifiedAdViewModel(ad),
				Owner = Application.Current.MainWindow
			}.ShowDialog();
		}


		public ICommand NewClassifiedAdCommand
		{
			get
			{
				return new DelegateCommand(() =>
				{
					ShowEditClassifiedAdWindow(new ClassifiedAd());
				});
			}
		}

		public ICommand OpenClassifiedAdCommand
		{
			get
			{
				return new DelegateCommand(() =>
				{
					ShowEditClassifiedAdWindow(SelectedClassifiedAd);
				});
			}
		}

		public ICommand DeleteClassifiedAdCommand => new DelegateCommand(() =>
		{
			if (SelectedClassifiedAd == null) return;
			if (MessageBox.Show($"Удалить рекламу от {SelectedClassifiedAd.Advertiser}?", "Подтвердите удаление", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) return;

			SelectedClassifiedAd.Delete();
			RefreshClassified(true);
		});

		public Visibility DeleteClassifiedAdVisibility => SelectedClassifiedAd == null ? Visibility.Collapsed : Visibility.Visible;


		//private void ShowEditAdvertizerWindow(Advertizer a)
		//{
		//	var w = new EditAdvertizerWindow
		//	{
		//		DataContext = new EditAdvertizerViewModel(a),
		//		Owner = Application.Current.MainWindow
		//	};
		//	w.ShowDialog();
		//}

		//public ICommand NewAdvertizerCommand
		//{
		//	get
		//	{
		//		return new DelegateCommand(() =>
		//		{
		//			ShowEditAdvertizerWindow(new Advertizer());
		//		});
		//	}
		//}

		public ICommand NewAdModuleCommand
		{
			get
			{
				return new DelegateCommand(() =>
				{
					try
					{
						new EditAdModuleWindow(new AdModule() { Grid = CurrentPage?.Grid })
						{
							Owner = Application.Current.MainWindow
						}.ShowDialog();
					}
					catch
					{
					}
				});
			}
		}

		public ICommand NewIssueCommand
		{
			get
			{
				return new DelegateCommand(() =>
				{
					new NewIssueWindow() { Owner = Application.Current.MainWindow }.ShowDialog();
				});
			}
		}

		public ICommand ValidateIssueCommand
		{
			get
			{
				return new DelegateCommand(() =>
				{
					if (CurrentIssue.Validate())
					{
						MessageBox.Show("Ошибок не выявлено.", "Все ок", MessageBoxButton.OK, MessageBoxImage.Information);
					}
				});
			}
		}

		public ICommand BuildIssueCommand => new DelegateCommand(() =>
			{
				if (MessageBox.Show($"Начать экспорт данных в InDesign для выпуска газеты номер {CurrentIssue.Number} за {CurrentIssue.Year} год?", "Экспорт в InDesign", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
					return;

				var c = new InDesignCoverter();
				c.CreateIssue(CurrentIssue);
			});

		private bool QuickBookWarning()
		{
			return MessageBox.Show($"QuickBook должен быть открыт на этом компьютере и в нем должна быть открыта компания. Продолжить?", "Необходим QuickBooks", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
		}

		public ICommand QuickBookImportCustomers => new DelegateCommand(async () =>
		{
			if (!QuickBookWarning()) return;
			VisibilityQuickBookConnect = Visibility.Visible;
			var cnt = await QuickBookManager.ImportCustomers();
			VisibilityQuickBookConnect = Visibility.Collapsed;
			MessageBox.Show("Успешно загружено " + cnt + " рекламодателей.", "Ура, заработало!", MessageBoxButton.OK, MessageBoxImage.Information);
		});



		public ICommand CreateInvoicesClassified => new DelegateCommand(() =>
		{
			var cadDict = new Dictionary<string, decimal>();
			foreach (var cad in ClassifiedAd.All)
			{
				if (cad.IssuePrice > 0 && cad.IsInNumber(CurrentIssue.Year, CurrentIssue.Number))
				{
					var sm = string.IsNullOrWhiteSpace(cad.SM) ? "No SM" : cad.SM;
					if (cadDict.ContainsKey(sm)) cadDict[sm] += cad.IssuePrice; else cadDict.Add(sm, cad.IssuePrice);
				}
			}

			var s = "";
			foreach (var kvp in cadDict)
			{
				s += kvp.Key + ": $" + kvp.Value.ToString("0.##") + "\r\n";
			}

			if (s == "")
			{
				MessageBox.Show("Объявлений с указанной ценой за номер в данном выпуске газеты не обнаружено.");
				return;
			}

			s += "Total amount: $" + cadDict.Values.Sum().ToString("0.##");

			if (MessageBox.Show($"Будут созданы счета для Объявлений в газете №{CurrentIssue.Year}.{CurrentIssue.Number}: \r\n\n" + s, "Объявления", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel) return;

			foreach (var kvp in cadDict)
			{
				var inv = new Invoice()
				{
					CustomerName = "CLASSIFIED. " + kvp.Key,
					NewspaperNumber = CurrentIssue.Number,
					NewspaperYear = CurrentIssue.Year,
					Price = kvp.Value,
					PageNumber = 0,
					AdDescription = "Classified ad"
				};
				inv.Save();
			}

			RefreshInvoices(true);
			MessageBox.Show("Счета за объявления выставлены. Результат можно посмотреть на вкладке Счета.", "Всё уже сделано", MessageBoxButton.OK, MessageBoxImage.Information);

		});

		public ICommand CreateInvoices => new DelegateCommand(() =>
		{
			var invoices = new List<Invoice>();
			foreach (var ad in IssueAdModules)
			{
				if (string.IsNullOrWhiteSpace(ad.AdModule.Advertiser?.Name) || ad.AdModule.Price <= 0 || !ad.IsSelected) continue;
				var inv = new Invoice()
				{
					CustomerName = ad.AdModule.Advertiser.Name,
					NewspaperNumber = CurrentIssue.Number,
					NewspaperYear = CurrentIssue.Year,
					Price = ad.AdModule.Price,
					PageNumber = ad.Page.Number,
					AdDescription = ad.AdSizeDescription
				};
				invoices.Add(inv);
			}

			if (invoices.Count == 0)
			{
				MessageBox.Show("Ничего не было выделено.", "Тут же пусто!", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			if (MessageBox.Show($"Будет создано счетов: {invoices.Count}. Продолжить?", "Вопросец", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK) return;

			foreach (var invoice in invoices)
			{
				invoice.Save();
			}


			RefreshInvoices(true);
			MessageBox.Show("Счета за рекламу выставлены. Результат можно посмотреть на вкладке Счета.", "Всё уже сделано", MessageBoxButton.OK, MessageBoxImage.Information);
		});


		public ICommand CalculateSums => new DelegateCommand(() =>
		{
			var prices = new Dictionary<string, decimal>();

			foreach (var invoice in Invoice.All)
			{
				
				var k = $"{invoice.NewspaperNumber}.{invoice.NewspaperYear}" + (invoice.CustomerName.StartsWith("CLASSIFIED") ? " CLASSIFIED" : " ADS");
				if (prices.ContainsKey(k))
				{
					prices[k] += invoice.Price;
				}
				else
				{
					prices.Add(k, invoice.Price);
				}
			}

			string s = "";
			foreach (var price in prices.OrderBy((x) => x.Key))
			{
				s += $"Сумма за №{price.Key}: ${price.Value:0.00}. \r\n";
			}

			MessageBox.Show(s, "Суммы", MessageBoxButton.OK, MessageBoxImage.Information);

		});

		//public ObservableCollection<Invoice> Invoices { get; private set; } = new ObservableCollection<Invoice>(Invoice.All);
		public ICollectionView Invoices { get; private set; }



		private string statusBarText;
		public string StatusBarText 
		{
			get
			{
				return statusBarText;
			}
			set
			{
				statusBarText = value;
				RaisePropertyChangedEvent("StatusBarText");
			}
		}

		private bool InvoicesFilter(object item)
		{
			if (ShowAllInvoices) return true;
			// show invoices for this issue only
			var inv = item as Invoice;
			return CurrentIssue?.Year == inv?.NewspaperYear && CurrentIssue?.Number == inv?.NewspaperNumber;
		}

		private void RefreshInvoices(bool fromDb)
		{
			if (fromDb || Invoices == null)
			{
				Invoice.ReloadAllFromDb();
				Invoices = CollectionViewSource.GetDefaultView(Invoice.All);
				Invoices.Filter = InvoicesFilter;
				RaisePropertyChangedEvent("ClassifiedView");
			}
			else
			{
				Invoices.Refresh();
			}
			RaisePropertyChangedEvent("Invoices");
			RaisePropertyChangedEvent("SelectAllInvoices");
		}

		private IEnumerable<Invoice> GetSelectedInvoices(bool excludeClassified)
		{
			var lst = new List<Invoice>();
			foreach (Invoice inv in Invoices)
			{
				if (excludeClassified && inv.CustomerName.StartsWith("CLASSIFIED")) continue;
				if (inv.IsSelected) lst.Add(inv);
			}
			lst.Sort((i1, i2) =>
			{
				var n = i1.CustomerName.CompareTo(i2.CustomerName);
				if (n != 0) return n;
				n = i1.NewspaperYear.CompareTo(i2.NewspaperYear);
				if (n != 0) return n;
				return i1.NewspaperNumber.CompareTo(i2.NewspaperNumber);
			});
			if (lst.Count == 0) MessageBox.Show("Счетов не выделено.", "Тут же пусто!", MessageBoxButton.OK, MessageBoxImage.Information);
			return lst;
		}

		public ICommand DeleteInvoices => new DelegateCommand(() =>
		{
			var invs = GetSelectedInvoices(false);
			if (invs.Count() == 0) return;
			if (MessageBox.Show($"Вы уверены что хотите удалить счетов в количестве {invs.Count()} шт.?", "Что, правда?", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) return;
			foreach (var inv in invs)
			{
				inv.Delete();
			}
			RefreshInvoices(true);
		});

		public ICommand QuickbookExportInvoices => new DelegateCommand(async () =>
		{
			var invs = GetSelectedInvoices(true);
			if (invs.Count() == 0) return;

			if (!QuickBookWarning()) return;

			var qbInvoices = QuickBookManager.MergeInvoices(invs);

			if (MessageBox.Show($"Будет выставлено счетов: {qbInvoices.Count()} (всего реклам: {invs.Count()}). Продолжить?", "Еще один вопрос", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK) return;

			VisibilityQuickBookConnect = Visibility.Visible;
			var cnt = await QuickBookManager.CreateInvoices(qbInvoices);
			VisibilityQuickBookConnect = Visibility.Collapsed;
			if (cnt > 0)
			{
				MessageBox.Show($"Сгенерировано {cnt} новых счетов в QuickBooks.", "Закончили", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		});




		public ICommand SyncClassifiedCommand => new DelegateCommand(() =>
		{
			WebsiteSync.WebsiteSync.SyncClassified();
		});



		public ICommand AboutCommand
		{
			get
			{
				return new DelegateCommand(() =>
				{
					new AboutWindow() { Owner = Application.Current.MainWindow }.ShowDialog();
				});
			}
		}

		public Visibility VisibilityServerProblems => NewspaperDatabase.IsConnectionProblems ? Visibility.Visible : Visibility.Collapsed;

		private Visibility _visibilityQuickBookConnect = Visibility.Collapsed;

		public Visibility VisibilityQuickBookConnect
		{
			get
			{
				return _visibilityQuickBookConnect;
			}
			set
			{
				_visibilityQuickBookConnect = value;
				RaisePropertyChangedEvent("VisibilityQuickBookConnect");
			}
		}


		public ICommand NewArticleCommand => new DelegateCommand(() =>
		{
			new EditArticleWindow(new NewspaperArticle(CurrentIssue.Year, CurrentIssue.Number)) { Owner = Application.Current.MainWindow }.ShowDialog();
		});

		public ICommand OpenArticleCommand => new DelegateCommand(() =>
		{
			if (SelectedArticle == null) return;
			new EditArticleWindow(SelectedArticle) { Owner = Application.Current.MainWindow }.ShowDialog();
		});

		public ICommand DeleteArticleCommand => new DelegateCommand(() =>
		{
			if (SelectedArticle == null) return;
			if (MessageBox.Show($"Удалить статью \"{SelectedArticle.Name}\"?", "Подтвердите хладнокровное убийство", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) return;
			SelectedArticle.Delete();
			RefreshArticles();
		});



		public Visibility DeleteArticleVisibility => SelectedArticle == null ? Visibility.Collapsed : Visibility.Visible;

		private NewspaperArticle _selectedArticle;
		public NewspaperArticle SelectedArticle
		{
			get
			{
				return _selectedArticle;
			}
			set
			{
				_selectedArticle = value;
				RaisePropertyChangedEvent("DeleteArticleVisibility");
			}
		}

		private IEnumerable<NewspaperArticle> _articles;

		public IEnumerable<NewspaperArticle> Articles
		{
			get
			{
				if (_articles == null) _articles = NewspaperArticle.GetArticles(CurrentIssue.Year, CurrentIssue.Number);
				return _articles;
			}
		}

		public void RefreshArticles()
		{
			_articles = null;
			RaisePropertyChangedEvent("Articles");
		}


		//public Visibility SyncArticlesVisibility
		//{
		//	get
		//	{
		//		return Articles.Any((x) => !x.SyncedWithSite) ? Visibility.Visible : Visibility.Collapsed;
		//	}
		//}

		public bool SyncArticlesEnabled { get; private set; } = true;

		private Visibility _articleSyncProgressBarVisibility = Visibility.Collapsed;

		public Visibility ArticleSyncProgressBarVisibility
		{
			get
			{
				return _articleSyncProgressBarVisibility;
			}
			set
			{
				_articleSyncProgressBarVisibility = value;
				RaisePropertyChangedEvent("ArticleSyncProgressBarVisibility");
			}
		}

		public ICommand SyncArticlesCommand => new DelegateCommand(() =>
		{
			ArticleSyncProgressBarVisibility = Visibility.Visible;
			SyncArticlesEnabled = false;
			RaisePropertyChangedEvent("SyncArticlesEnabled");
			WebsiteSync.WebsiteSync.SyncArticles(() =>
			{
				ArticleSyncProgressBarVisibility = Visibility.Collapsed;
				SyncArticlesEnabled = true;
				RaisePropertyChangedEvent("SyncArticlesEnabled");
				RefreshArticles();
			});

		});


	}
}
