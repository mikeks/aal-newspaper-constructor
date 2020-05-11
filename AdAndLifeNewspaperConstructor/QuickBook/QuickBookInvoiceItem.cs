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
		public string ItemName => "Ad & Life #" + (NewspaperNumber > 9 ? "" : " ") + NewspaperNumber.ToString();
		public string Description => ItemName;
		public decimal Price { get; set; }
	}
}
