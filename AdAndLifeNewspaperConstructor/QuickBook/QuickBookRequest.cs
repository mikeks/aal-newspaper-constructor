using QBFC13Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VitalConnection.AAL.Builder.QuickBook
{
	abstract class QuickBookRequest
	{

		protected abstract void Action(QBSessionManager sessionManager, IMsgSetRequest request);

		public bool IsFailed { get; private set; } = false;

		public void Start()
		{

			// We want to know if we begun a session so we can end it if an
			// error happens
			bool isSessionBegun = false;

			// Create the session manager object using QBFC
			QBSessionManager sessionManager = new QBSessionManager();

			try
			{
				// Open the connection and begin a session to QuickBooks
				sessionManager.OpenConnection("", "Newspaper Constructor");
				//sessionManager.OpenConnection2("", "IDN InvoiceAdd C# sample",ENConnectionType.ctLocalQBDLaunchUI);
				sessionManager.BeginSession("", ENOpenMode.omDontCare);
				isSessionBegun = true;

				// Get the RequestMsgSet based on the correct QB Version
				IMsgSetRequest requestSet = sessionManager.CreateMsgSetRequest("US", 6, 0);

				// Initialize the message set request object
				requestSet.Attributes.OnError = ENRqOnError.roeStop;

				Action(sessionManager, requestSet);

				// Close the session and connection with QuickBooks
				sessionManager.EndSession();
				isSessionBegun = false;
				sessionManager.CloseConnection();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Возникла проблема при попытке подключится к QuickBooks. Убедитесь что QuickBooks запущен на этом компьютере и в нем открыта компания.\n\n" + ex.Message.ToString(), "Ой ой ой", MessageBoxButton.OK, MessageBoxImage.Error);
				if (isSessionBegun)
				{
					sessionManager.EndSession();
					sessionManager.CloseConnection();
				}
				IsFailed = true;
			}
		}


	}
}
