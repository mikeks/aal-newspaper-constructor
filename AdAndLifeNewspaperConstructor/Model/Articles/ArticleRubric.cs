using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalConnection.AAL.Builder.Model.Articles
{
	public class ArticleRubric : DbObject, IDbObject
	{

		public int Id { get; set; }
		public string Name { get; set; }

		public void ReadFromDb(SqlDataReader rdr)
		{
			Id = (int)rdr["Id"];
			Name = (string)rdr["Name"];
		}

		private static ArticleRubric[] _all;

		public static ArticleRubric[] All {
			get
			{
				if (_all == null) _all = ReadCollectionFromDb<ArticleRubric>("select * from ArticleRubric order by Name");
				return _all;
			}
		}

		public override string ToString()
		{
			return Name;
		}

	}



	
}
