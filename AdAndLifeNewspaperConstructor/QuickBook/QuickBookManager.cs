using QBFC13Lib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VitalConnection.AAL.Builder.Model;

namespace VitalConnection.AAL.Builder.QuickBook
{
	class QuickBookManager
	{

		public static async Task<int> CreateInvoices(IEnumerable<QuickBookInvoice> invoices)
		{
			var cnt = 0;
			foreach (var inv in invoices)
			{
				var cr = new QuickBookCreateInvoice(inv);
				await Task.Run(() => cr.Start());
				if (cr.StatusCode != 0)
				{
					if (MessageBox.Show($"Возникла проблема с выставлением счета для {inv.CustomerName} (код {cr.StatusCode}). Продолжить генерировать остальные счета?", "Опа!", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.No)
						break;
				}
				else cnt++;
			}
			return cnt;
		}

		public static async Task<int> ImportCustomers()
		{
			var imp = new QuickBookImportCustomers();
			await Task.Run(() => imp.Start());
			var list = imp.Customers;

			if (list.Count == 0) return 0;

			var dt = new DataTable();
			dt.Columns.Add("Name", typeof(string));
			foreach (var name in list)
			{
				dt.Rows.Add(name);
			}

			DbObject.ExecStoredProc("ImportAdvertisersFromQb", (cmd) =>
			{
				cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@advertisers", SqlDbType.Structured) { Value = dt });
			});

			Advertizer.ReloadAllFromDb();

			return list.Count;
		}

	}
}
