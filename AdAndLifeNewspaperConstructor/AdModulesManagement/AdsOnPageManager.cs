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

		private MainViewModel _mvm;
		private Grid _grid { get; set; }

		public AdsOnPageManager(MainViewModel mvm, Grid grid)
		{
			_mvm = mvm;
			_grid = grid;
		}

		private List<Border> _blocks = new List<Border>();
		private List<AdModuleOnPage> _adModules = new List<AdModuleOnPage>();
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
			_mvm.Status = status;
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
			SetStatus("Loading images...");

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

					var lb = new Label();
					lb.Content = System.IO.Path.GetFileName(ph);
					lb.HorizontalAlignment = HorizontalAlignment.Center;
					lb.VerticalAlignment = VerticalAlignment.Center;
					lb.FontSize = 16;
					b.Child = lb;



				}
				else
				{
					var il = new ImageLoader();
					var imgSrc = await il.LoadImage(ph);

					var img = new Image();
					img.Source = imgSrc;
					img.Stretch = Stretch.Fill;

					b.Child = img;
				}


				b.MouseUp += Img_MouseUp;
				b.MouseDown += Img_MouseDown;
				b.Tag = m;

				_blocks.Add(b);
				_grid.Children.Add(b);
				_adModules.Add(m);

			}

			// check for removed
			foreach (var img in _blocks)
			{
				var amp = img.Tag as AdModuleOnPage;
				if (CurrentPage.AdModules.FirstOrDefault((x) => x.AdModuleId == amp.AdModuleId) == null)
				{
					// was removed
					_grid.Children.Remove(img);
					_adModules.Remove(amp);
				}
			}

			SetStatus("");

		}

		public async void RecreateGrid()
		{
			_grid.ColumnDefinitions.Clear();
			for (int x = 0; x < CurrentPage.Grid.ColumnsCount; x++)
			{
				_grid.ColumnDefinitions.Add(new ColumnDefinition());
			}

			_grid.RowDefinitions.Clear();
			for (int y = 0; y < CurrentPage.Grid.RowCount; y++)
			{
				_grid.RowDefinitions.Add(new RowDefinition());
			}

			_grid.Children.Clear();

			for (int y = 0; y < CurrentPage.Grid.RowCount; y++)
			{
				for (int x = 0; x < CurrentPage.Grid.ColumnsCount; x++)
				{
					var r = new Rectangle(); //Border();
											 //r.Fill = new RadialGradientBrush(Colors.Red, Colors.Blue);
					r.Fill = new SolidColorBrush(Colors.Gray);
					r.Margin = new Thickness(2);
					//r.BorderThickness = new Thickness(1);
					Grid.SetColumn(r, x);
					Grid.SetRow(r, y);
					_grid.Children.Add(r);
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
					Grid = _grid,
					//StatusBarText = _win.tbStatusBarText,
					CurrentBlock = brd,
					StartMousePosition = mousePos,
					Blocks = _blocks.ToArray(),
                    Page = CurrentPage
				});
				_moveMode.OnDone += _imageSelectionMode_OnDone;

                

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
            var p = e.GetPosition(_grid);
            StartMoveMode(brd, p);
        }

        private void _imageSelectionMode_OnDone(MoveAdModuleMode sender)
		{
			_moveMode = null;
		}

	}
}
