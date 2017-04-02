using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalConnection.AAL.Builder.Model
{
	public class ClassifiedRubric : DbObject, IDbObject
	{

		public int Id { get; private set; }
		public string Name { get; private set; }
		public string ImageFilename { get; private set; }
		public int SortOrder { get; private set; }

		public ClassifiedRubric()
		{

		}

		public ClassifiedRubric(string name)
		{
			Name = name;
		}

		public void ReadFromDb(SqlDataReader rdr)
		{
			Id = (int)rdr["Id"];
			Name = (string)rdr["Name"];
			SortOrder = (byte)rdr["Sort"];
            ImageFilename = (string)rdr["ImageFilename"];
        }

		private static Dictionary<int, ClassifiedRubric> _all;

		public static ClassifiedRubric GetById(int id)
		{
			if (_all == null)
			{
				var all = ReadCollectionFromDb<ClassifiedRubric>("select * from ClassifiedRubric");
				_all = new Dictionary<int, ClassifiedRubric>();
				foreach (var a in all)
				{
					_all.Add(a.Id, a);
				}
			}

			if (!_all.ContainsKey(id)) return null;
			return _all[id];
		}

        public static void ReloadAllFromDb()
        {
            _all = null;
        }


        public static IEnumerable<ClassifiedRubric> All
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
