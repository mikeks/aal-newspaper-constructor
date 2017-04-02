using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VitalConnection.AAL.Builder.Model;

namespace VitalConnection.AAL.Builder.ViewModel
{
    class EditAdModuleWindowViewModel : ObservableObject
    {

        private string ShowSelectImageFileDialog()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "Файлы картинок или pdf|*.jpeg;*.jpg;*.png;*.gif;*.pdf|Все файлы|*.*";

            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();

            if (result != true || dlg.FileName == null) return null;

            return dlg.FileName;
        }

        public EditAdModuleWindowViewModel(AdModule ad)
        {
            Ad = ad;
            InitImage();
        }

        public string Title
        {
            get
            {
                return string.IsNullOrEmpty(Ad.FullPath) ? "Новый рекламный блок" : Path.GetFileNameWithoutExtension(Ad.FullPath);
            }
        }

        private void GuessSizeByFilename()
        {
            var fn = Path.GetFileName(Ad.FullPath);
            
            

            if (fn.StartsWith("1-2"))
            {
                Ad.Width = Ad.Grid.ColumnsCount; // 6
                Ad.Height = Ad.Grid.RowCount / 2; // 4
                RaisePropertyChangedEvent("Ad");
                //MessageBox.Show("Предполагаю, что это реклама на пол странички.");
                return;
            }

            if (fn.StartsWith("1-4"))
            {
                Ad.Width = Ad.Grid.ColumnsCount; // 3
                Ad.Height = Ad.Grid.RowCount / 2; // 4
                RaisePropertyChangedEvent("Ad");
                //MessageBox.Show("Предполагаю, что это реклама на четвертушку.");
                return;
            }

            if (fn.StartsWith("1-8"))
            {
                Ad.Width = Ad.Grid.ColumnsCount; // 3
                Ad.Height = Ad.Grid.RowCount / 4; // 2
                RaisePropertyChangedEvent("Ad");
                //MessageBox.Show("Предполагаю, что это реклама на восьмушку.");
                return;
            }

            if (fn.StartsWith("1-0"))
            {
                Ad.Width = Ad.Grid.ColumnsCount;
                Ad.Height = Ad.Grid.RowCount;
                RaisePropertyChangedEvent("Ad");
                //MessageBox.Show("Предполагаю, что это реклама на всю страницу.");
                return;
            }
        }

        private async void InitImage()
        {

            if (Ad.FullPath != null)
            {
                Image = await new ImageLoader().LoadImage(Utility.ConvertFilePath(Ad.FullPath));
            } else
            {
                await SelectNewFile();
                if (Ad.FullPath == null)
                    throw new Exception("User cancel the operation");
            }

        }

		public string Filename => Path.GetFileName(Ad.FullPath);

        public AdModule Ad
        {
            get; private set;
        }

        private ImageSource _image;
        public ImageSource Image
        {
            get
            {
                return _image;
            }
            private set
            {
                _image = value;
                RaisePropertyChangedEvent("Image");
            }
        }

        private async Task SelectNewFile()
        {
            var imgFn = ShowSelectImageFileDialog();
            
            if (imgFn == null) return;

			if (Path.HasExtension(".pdf"))
			{
				Image = null;
				Ad.FullPath = imgFn;
				Ad.Width = 2;
				Ad.Height = 2;
				RaisePropertyChangedEvent("Ad");
			}
			else
			{
				var img = await new ImageLoader().LoadImage(imgFn, false);
				if (img == null)
				{
					MessageBox.Show("Не удалось загрузить изображение.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}


				//Image = img;
				Ad.FullPath = imgFn;
				Image = img;

				GuessSizeByFilename();
			}

		}

        public IEnumerable<GridType> AllGridTypes
        {
            get
            {
                return GridType.AllTypes;
            }
        }

        public IEnumerable<Advertizer> AllAdvertizers
        {
            get
            {
                return Advertizer.All;
            }
        }
        


        public ICommand SelectNewFileCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    #pragma warning disable CS4014 
                    SelectNewFile();
                });
            }
        }

        public ICommand LocateOnDiscCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    Process.Start("explorer.exe", Utility.ConvertFilePath(Ad.FullPath));
                });
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    Ad.Save();
                    MainViewModel.Instance.RefreshAdModules();
                });
            }
        }

        //public ICommand CancelCommand
        //{
        //    get
        //    {
        //        return new DelegateCommand(() =>
        //        {
        //        });
        //    }
        //}


    }
}
