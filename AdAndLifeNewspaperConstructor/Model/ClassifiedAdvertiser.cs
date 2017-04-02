using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalConnection.AAL.Builder.Model
{
	public class ClassifiedAdvertiser : DbObject, IDbObject
	{

		public int Id { get; private set; }
		public string Name { get; private set; }
		public string Info { get; private set; }

		public void ReadFromDb(SqlDataReader rdr)
		{
			Id = (int)rdr["AdvertizerId"];
			Name = (string)rdr["Name"];
			Info = (string)ResolveDbNull(rdr["Info"]);
		}


		private static Dictionary<int, ClassifiedAdvertiser> _all;

		public static ClassifiedAdvertiser GetById(int id)
		{
			if (_all == null)
			{
				var all = ReadCollectionFromDb<ClassifiedAdvertiser>("select * from ClassifiedAdvertizer");
				_all = new Dictionary<int, ClassifiedAdvertiser>();
				foreach (var a in all)
				{
					_all.Add(a.Id, a);
				}
			}

			if (!_all.ContainsKey(id)) return null;
			return _all[id];
		}

		public static IEnumerable<ClassifiedAdvertiser> All
		{
			get
			{
				return _all.Values;
			}
		}

		public override string ToString()
		{
			return Name;
		}

	}
}
