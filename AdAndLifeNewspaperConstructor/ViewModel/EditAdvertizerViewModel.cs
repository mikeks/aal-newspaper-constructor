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
	class EditAdvertizerViewModel
    {

		private Advertizer _ad;

		public Advertizer Ad
		{
			get
			{
				return _ad;
			}
		}

		public EditAdvertizerViewModel(Advertizer ad)
		{
			_ad = ad;
		}
        

		public ICommand SaveCommand
		{
			get
			{
				return new DelegateCommand<Window>((w) => 
                {
					Ad.Save();
					w.Close();
                    MainViewModel.Instance.RefreshAdvertizers();
				});
			}
		}

		public ICommand CancelCommand
		{
			get
			{
				return new DelegateCommand<Window>((w) => 
                {
                    Ad.Revert();
					w.Close();
                    MainViewModel.Instance.RefreshAdvertizers();
				});
			}
		}

	}
}
