using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalConnection.AAL.Builder.QuickBook
{
	class QuickBookInvoice
	{
		public string CustomerName { get; set; }
		public List<QuickBookInvoiceItem> Items { get; } = new List<QuickBookInvoiceItem>();
	}
}
