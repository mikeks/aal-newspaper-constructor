using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using VitalConnection.AAL.Builder.Model;

namespace VitalConnection.AAL.Builder.AdModulesManagement
{
	class MoveAdModuleMode
	{
		public event Action<MoveAdModuleMode> OnDone;
		//public event Action<ImageSelectionMode> OnSelected;

		public struct SelectionContext
		{
			public Grid Grid { get; set; }
			public Border CurrentBlock { get; set; }
			public Point StartMousePosition { get; set; }
			public Border[] Blocks { get; set; }
            public NewspaperPage Page { get; set; }
			//public Canvas SelectionCanvas { get; set; }
			//public TextBlock StatusBarText { get; set; }
		}

		SelectionContext _context;
		//private Image _selImg;

		public SelectionContext Context
		{
			get
			{
				return _context;
			}
		}

		private int _currentImageDefaultRow;
		private int _currentImageDefaultColumn;

		public MoveAdModuleMode(SelectionContext context)
		{
			_context = context;

			foreach (var img in _context.Blocks)
			{
				if (img != _context.CurrentBlock)
				{
					img.Opacity = 0.2;
				}
				else
				{
					img.Opacity = 1;
				}
			}

			

			//_selImg = new Image();
			//_selImg.Stretch = System.Windows.Media.Stretch.Fill;
			//_selImg.Source = _context.CurrentImage.Source;
			//_selImg.Width = _context.CurrentImage.ActualWidth;
			//_selImg.Height = _context.CurrentImage.ActualHeight;
			//_context.SelectionCanvas.Children.Add(_selImg);

			_currentImageDefaultColumn = Grid.GetColumn(_context.CurrentBlock);
			_currentImageDefaultRow = Grid.GetRow(_context.CurrentBlock);

			_context.Grid.MouseMove += Grid_MouseMove;
			_context.Grid.MouseUp += Grid_MouseUp;

		}

		private void Done()
		{

			_context.Grid.MouseMove -= Grid_MouseMove;
			_context.Grid.MouseUp -= Grid_MouseUp;

			foreach (var img in _context.Blocks)
			{
				img.Opacity = 1;
			}

			OnDone(this);
		}

		private void Selected(int newColumn, int newRow)
		{
			var m = (AdModuleOnPage)_context.CurrentBlock.Tag;
			m.SavePostionToDb(newColumn + 1, newRow + 1);
		//	OnSelected(this);
		}

		private void Grid_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var newColumn = Grid.GetColumn(_context.CurrentBlock);
			var newRow = Grid.GetRow(_context.CurrentBlock);

			if (_currentImageDefaultColumn != newColumn || _currentImageDefaultRow != newRow)
			{
				Selected(newColumn, newRow);
			}

			Done();
		}

		private void Grid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var p = e.GetPosition(_context.Grid);

			var x = p.X;// - _context.StartMousePosition.X;
			var y = p.Y;// - _context.StartMousePosition.Y;

			var dw = _context.Grid.ColumnDefinitions[0].ActualWidth;
			var dh = _context.Grid.RowDefinitions[0].ActualHeight;

			var dx = (int)(x / dw);
			var dy = (int)(y / dh);

            //_context.StatusBarText.Text = $"x = {x} / y = {y} || dw = {dw} / dh = {dh} || dx = {dx} / dy = {dy}";
            //Debug.WriteLine();

            var m = (AdModuleOnPage)_context.CurrentBlock.Tag;

            if (dx > _context.Page.Grid.ColumnsCount - m.AdModule.Width) return; // can't move there
            if (dy > _context.Page.Grid.RowCount - m.AdModule.Height) return; // can't move there



            try
            {
				Grid.SetColumn(_context.CurrentBlock, dx); // _currentImageDefaultColumn
				Grid.SetRow(_context.CurrentBlock, dy); // _currentImageDefaultRow
			}
			catch 
			{
			}


		}


	}
}
