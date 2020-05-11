using System.Collections.Generic;
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


		AdsOnPageManager _pageLayout;

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

			if (IsAdModuleAdvertizerFilterOn && AdModuleAdvertizerFilter != null && l.Advertizer != AdModuleAdvertizerFilter) return false;

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
				var il = new ImageLoader();
				_selectedImage = await il.LoadImage(ph);
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

		public string AdModuleAdvertiserName => $"Рекламодатель: {_selectedAdModule?.Advertizer?.Name ?? "не указан"}";

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
					var w = new EditAdModuleWindow(SelectedAdModule);
					w.ShowDialog();
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


		string _status;
		public string Status
		{
			get
			{
				return _status;
			}

			set
			{
				_status = value;
				RaisePropertyChangedEvent("Status");
			}
		}




		public void FillPageSelectionStackPanel()
		{
			StackPanel sp = PageSelectionStackPanel;

			sp.Children.Clear();

			Button firstBtn = null;

			foreach (var p in CurrentIssue.Pages)
			{
				var b = new Button();
				b.Content = p;
				_defaultButtonBorderBrush = b.BorderBrush;
				//b.BorderThickness = new Thickness(2);
				b.Padding = new Thickness(10, 0, 10, 0);
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
			}
			else
			{
				ClassifiedView.Refresh();
			}
			RaisePropertyChangedEvent("ClassifiedCount");

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
				var l = new List<ClassifiedRubric>();
				l.Add(AllRubricsEntity);
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
			var w = new EditClassifiedAdWindow();
			w.DataContext = new EditClassifiedAdViewModel(ad);
			w.ShowDialog();
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


		private void ShowEditAdvertizerWindow(Advertizer a)
		{
			var w = new EditAdvertizerWindow();
			w.DataContext = new EditAdvertizerViewModel(a);
			w.ShowDialog();
		}

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
						var w = new EditAdModuleWindow(new AdModule() { Grid = CurrentPage?.Grid });
						w.ShowDialog();
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
					new NewIssueWindow().ShowDialog();
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
			QuickBookWarning();
			VisibilityQuickBookConnect = Visibility.Visible;
			var cnt = await QuickBookManager.ImportCustomers();
			VisibilityQuickBookConnect = Visibility.Collapsed;
			MessageBox.Show("Успешно загружено " + cnt + " рекламодателей.", "Ура, заработало!", MessageBoxButton.OK, MessageBoxImage.Information);
		});

		public ICommand QuickbookExportInvoices => new DelegateCommand(async () =>
		{
			var invoices = new List<QuickBookInvoice>();
			foreach (var ad in IssueAdModules)
			{
				if (string.IsNullOrWhiteSpace(ad.AdModule.Advertizer?.Name) || ad.AdModule.Price <= 0 || !ad.IsSelected) continue;
				var inv = new QuickBookInvoice()
				{
					CustomerName = ad.AdModule.Advertizer.Name,
					NewspaperNumber = CurrentIssue.Number,
					Price = ad.AdModule.Price
				};
				invoices.Add(inv);
			}

			if (invoices.Count == 0)
			{
				MessageBox.Show("Ничего не было выделено.", "Тут же пусто!", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			QuickBookWarning();

			if (MessageBox.Show($"Будет выставлено счетов: {invoices.Count}. Продолжить?", "Еще один вопрос", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK) return;

			VisibilityQuickBookConnect = Visibility.Visible;
			var cnt = await QuickBookManager.CreateInvoices(invoices);
			if (cnt > 0) MessageBox.Show($"Сгенерировано {cnt} новых счетов в QuickBooks.", "Закончили", MessageBoxButton.OK, MessageBoxImage.Information);
			VisibilityQuickBookConnect = Visibility.Collapsed;
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
					new AboutWindow().ShowDialog();
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
			new EditArticleWindow(new NewspaperArticle(CurrentIssue.Year, CurrentIssue.Number)).ShowDialog();
		});

		public ICommand OpenArticleCommand => new DelegateCommand(() =>
		{
			if (SelectedArticle == null) return;
			new EditArticleWindow(SelectedArticle).ShowDialog();
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
