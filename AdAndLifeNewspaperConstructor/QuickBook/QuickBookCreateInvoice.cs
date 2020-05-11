using QBFC13Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalConnection.AAL.Builder.QuickBook
{
	class QuickBookCreateInvoice : QuickBookRequest
	{

		private QuickBookInvoice Invoice { get; }
		public int StatusCode { get; private set; }

		public QuickBookCreateInvoice(QuickBookInvoice invoice)
		{
			Invoice = invoice;
		}

		protected override void Action(QBSessionManager sessionManager, IMsgSetRequest request)
		{

			// Add the request to the message set request object
			IInvoiceAdd invoiceAdd = request.AppendInvoiceAddRq();
			
			invoiceAdd.CustomerRef.FullName.SetValue(Invoice.CustomerName);

			IInvoiceLineAdd invoiceLineAdd = invoiceAdd.ORInvoiceLineAddList.Append().InvoiceLineAdd;
			invoiceLineAdd.ItemRef.FullName.SetValue(Invoice.ItemName);
			invoiceLineAdd.Desc.SetValue(Invoice.Description);
			var price = (double)Invoice.Price;
			invoiceLineAdd.ORRatePriceLevel.Rate.SetValue(price);
			invoiceLineAdd.Quantity.SetValue(Convert.ToDouble(1));
			invoiceLineAdd.Amount.SetValue(price);

			var responseMsgSet = sessionManager.DoRequests(request);
			IResponse response = responseMsgSet.ResponseList.GetAt(0);
			StatusCode = response.StatusCode;
		}
	}
}
