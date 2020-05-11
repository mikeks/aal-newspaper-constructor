using QBFC13Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalConnection.AAL.Builder.QuickBook
{
	class QuickBookImportCustomers : QuickBookRequest
	{

		public List<string> Customers { get; } = new List<string>();

		protected override void Action(QBSessionManager sessionManager, IMsgSetRequest request)
		{


			// Add the request to the message set request object
			//ICustomerQuery CustQ = request.AppendCustomerQueryRq();
			request.AppendCustomerQueryRq();

			// Optionally, you can put filter on it.
			//CustQ.ORCustomerListQuery.CustomerListFilter.MaxReturned.SetValue(50);

			// Do the request and get the response message set object
			IMsgSetResponse responseSet = sessionManager.DoRequests(request);

			// Uncomment the following to view and save the request and response XML
			// string requestXML = requestSet.ToXMLString();
			// MessageBox.Show(requestXML);
			// SaveXML(requestXML);
			// string responseXML = responseSet.ToXMLString();
			// MessageBox.Show(responseXML);
			// SaveXML(responseXML);

			IResponse response = responseSet.ResponseList.GetAt(0);
			// int statusCode = response.StatusCode;
			// string statusMessage = response.StatusMessage;
			// string statusSeverity = response.StatusSeverity;
			// MessageBox.Show("Status:\nCode = " + statusCode + "\nMessage = " + statusMessage + "\nSeverity = " + statusSeverity);


			ICustomerRetList customerRetList = response.Detail as ICustomerRetList;
			if (!(customerRetList.Count == 0))
			{
				for (int ndx = 0; ndx <= (customerRetList.Count - 1); ndx++)
				{
					ICustomerRet customerRet = customerRetList.GetAt(ndx);
					Customers.Add(customerRet.FullName.GetValue());
				} // for
			} // if


		}
	}
}
