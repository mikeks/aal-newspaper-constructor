using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalConnection.AAL.Builder.QuickBook
{




	class QuickBookInvoiceItem
	{
		public int NewspaperNumber { get; set; }
		public int NewspaperYear { get; set; }
		public DateTime NewspaperDate => new DateTime(NewspaperYear, 1, 1).Next(DayOfWeek.Wednesday).AddDays(7 * (NewspaperNumber - 1));
		public string AdDescription { get; set; }
		public string ItemName => $"Ad & Life #{NewspaperNumber}   {NewspaperDate:MMMM dd, yyyy}   {AdDescription}".Trim();
		public string Description => ItemName;
		public decimal Price { get; set; }
	}
}
