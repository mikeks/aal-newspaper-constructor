using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VitalConnection.AAL.Builder.Model;
using VitalConnection.AAL.Builder.ViewModel;

namespace VitalConnection.AAL.Builder.AdModulesManagement
{
	class AdsOnPageManager
	{

		private readonly MainViewModel _mvm;
		private Grid Grid { get; set; }

		public AdsOnPageManager(MainViewModel mvm, Grid grid)
		{
			_mvm = mvm;
			this.Grid = grid;
		}

		private readonly List<Border> _blocks = new List<Border>();
		private readonly List<AdModuleOnPage> _adModules = new List<AdModuleOnPage>();
		private MoveAdModuleMode _moveMode;
		private AdModuleOnPage _selectedAdModule;

        public AdModuleOnPage SelectedAdModule
        {
            get
            {
                return _selectedAdModule;
            }
            set
            {
                _selectedAdModule = value;



            }
        }


        private void SetStatus(string status)
		{
			_mvm.StatusBarText = status;
		}

		private NewspaperPage CurrentPage
		{
			get
			{
				return _mvm.CurrentPage;
			}
		}

		public async Task RefreshAdModulesOnPage()
		{
			SetStatus("Загружаем картинки...");

			foreach (var m in CurrentPage.AdModules)
			{
				if (_adModules.Contains(m)) continue;

				var ph = Utility.ConvertFilePath(m.AdModule.FullPath);


				var b = new Border();
				Grid.SetColumn(b, m.X - 1);
				Grid.SetRow(b, m.Y - 1);
				Grid.SetColumnSpan(b, m.AdModule.Width);
				Grid.SetRowSpan(b, m.AdModule.Height);


				if (System.IO.Path.GetExtension(ph)?.ToLower() == ".pdf")
				{
					b.Background = new SolidColorBrush(Color.FromRgb(170, 239, 228));

					var lb = new Label
					{
						Content = System.IO.Path.GetFileName(ph),
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Center,
						FontSize = 16
					};
					b.Child = lb;



				}
				else
				{
					SetStatus("Загружаем: " + ph);
					var imgSrc = await ImageLoader.LoadImage(ph);
					b.Child = new Image
					{
						Source = imgSrc,
						Stretch = Stretch.Fill
					};
				}


				b.MouseUp += Img_MouseUp;
				b.MouseDown += Img_MouseDown;
				b.Tag = m;

				_blocks.Add(b);
				Grid.Children.Add(b);
				_adModules.Add(m);

			}

			// check for removed
			foreach (var img in _blocks)
			{
				var amp = img.Tag as AdModuleOnPage;
				if (CurrentPage.AdModules.FirstOrDefault((x) => x.AdModuleId == amp.AdModuleId) == null)
				{
					// was removed
					Grid.Children.Remove(img);
					_adModules.Remove(amp);
				}
			}

			SetStatus("Готово");

		}

		public async void RecreateGrid()
		{
			Grid.ColumnDefinitions.Clear();
			for (int x = 0; x < CurrentPage.Grid.ColumnsCount; x++)
			{
				Grid.ColumnDefinitions.Add(new ColumnDefinition());
			}

			Grid.RowDefinitions.Clear();
			for (int y = 0; y < CurrentPage.Grid.RowCount; y++)
			{
				Grid.RowDefinitions.Add(new RowDefinition());
			}

			Grid.Children.Clear();

			for (int y = 0; y < CurrentPage.Grid.RowCount; y++)
			{
				for (int x = 0; x < CurrentPage.Grid.ColumnsCount; x++)
				{
					var r = new Rectangle
					{
						Fill = new SolidColorBrush(Colors.Gray),
						Margin = new Thickness(2)
					}; 
					Grid.SetColumn(r, x);
					Grid.SetRow(r, y);
					Grid.Children.Add(r);
				}
			}



			_blocks.Clear();
			_adModules.Clear();
			await RefreshAdModulesOnPage();

		}

		private void StartMoveMode(Border brd, Point mousePos)
		{
			if (_moveMode == null)
			{

				_moveMode = new MoveAdModuleMode(new MoveAdModuleMode.SelectionContext()
				{
					Grid = Grid,
					//StatusBarText = _win.tbStatusBarText,
					CurrentBlock = brd,
					StartMousePosition = mousePos,
					Blocks = _blocks.ToArray(),
                    Page = CurrentPage
				});
				_moveMode.OnDone += ImageSelectionMode_OnDone;

                

				//_imageSelectionMode.OnSelected += _imageSelectionMode_OnSelected;
			}
		}

        private void SelectAdModule(AdModuleOnPage adModule)
        {
            SelectedAdModule = adModule;
            _mvm.SelectedAdModule = adModule.AdModule;
            _mvm.SelectedAdModuleOnPage = adModule;
        }

        private void Img_MouseUp(object sender, MouseButtonEventArgs e)
		{
            if (e.ChangedButton != MouseButton.Left) return;
            var brd = (Border)sender;
            var adModule = brd.Tag as AdModuleOnPage;
            SelectAdModule(adModule);
        }

        private void Img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Right) return;
            var brd = (Border)sender;
            var adModule = brd.Tag as AdModuleOnPage;
            SelectAdModule(adModule);
            var p = e.GetPosition(Grid);
            StartMoveMode(brd, p);
        }

        private void ImageSelectionMode_OnDone(MoveAdModuleMode sender)
		{
			_moveMode = null;
		}

	}
}
