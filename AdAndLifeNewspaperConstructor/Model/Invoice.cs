using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalConnection.AAL.Builder.Model
{
	public class Invoice : DbObject, IDbObject
	{
		public int Id { get; private set; }
		public DateTime Created { get; private set; }
		//public DateTime Exported { get; private set; }
		public string CustomerName { get; set; }
		public int NewspaperNumber { get; set; }
		public int PageNumber { get; set; }
		public decimal Price { get; set; }

		public bool IsSelected { get; set; }

		public void ReadFromDb(SqlDataReader rdr)
		{
			Id = (int)rdr["Id"];
			CustomerName = (string)rdr["CustomerName"];
			NewspaperNumber = (short)rdr["NewspaperNumber"];
			Price = (decimal)rdr["Price"];
			PageNumber = (int)rdr["PageNumber"];
			Created = (DateTime)rdr["Created"];
			//Exported = (DateTime)rdr["Exported"];
		}

		public void Save()
		{
			ExecStoredProc("AddInvoice", (cmd) =>
			{
				cmd.Parameters.AddWithValue("@customerName", CustomerName);
				cmd.Parameters.AddWithValue("@newspaperNumber", NewspaperNumber);
				cmd.Parameters.AddWithValue("@price", Price);
				cmd.Parameters.AddWithValue("@pageNumber", PageNumber);
			});
		}

        static IEnumerable<Invoice> _all;

		public static void ReloadAllFromDb()
		{
			Load();
		}

        public static IEnumerable<Invoice> All
        {
            get
            {
                if (_all == null) Load();
                return _all;
            }
        }

        private static void Load()
        {
            _all = ReadCollectionFromDb<Invoice>("select * from Invoice order by CustomerName");
        }

		internal void Delete()
		{
			ExecSQL("delete from Invoice where Id = @id", (cmd) => cmd.Parameters.AddWithValue("@id", Id));
		}
	}


}
