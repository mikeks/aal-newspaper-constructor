using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using VitalConnection.AAL.Builder.Model;

namespace VitalConnection.AAL.Builder.ViewModel
{
	class FindAdvertiserViewModel
	{

		string _filterString;
		public string FilterString
		{
			get
			{
				return _filterString;
			}
			set
			{
				_filterString = value;
				AdvertisersView.Refresh();
			}
		}

		public ICollectionView AdvertisersView { get; private set; }

		public FindAdvertiserViewModel()
		{
			AdvertisersView = CollectionViewSource.GetDefaultView(Advertizer.All); // .OrderBy((m) => m.Name)
			AdvertisersView.Filter = AdvertiserFilter;
			AdvertisersView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
		}

		private bool AdvertiserFilter(object item)
		{
			var l = item as Advertizer;
			if (string.IsNullOrWhiteSpace(FilterString)) return true;
			return l.Name.ToLower().Contains(FilterString.ToLower().Trim());
		}

		public Advertizer SelectedAdvertiser
		{
			get; set;
		}

	}
}
