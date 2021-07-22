using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalConnection.AAL.Builder.Model
{
    public class Advertizer : DbObject, IDbObject
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }


        private static Dictionary<int, Advertizer> _all;

        public static IEnumerable<Advertizer> All
        {
            get
            {
                if (_all == null) Load();
                return _all?.Values;
            }
        }

        public static void ReloadAllFromDb()
        {
            Load();
        }

        public static Advertizer GetById(int id)
        {
            if (_all == null) Load();
			_all.TryGetValue(id, out Advertizer a);
			return a;
        }

        private static void Load()
        {
            var mm = ReadCollectionFromDb<Advertizer>("select * from Advertizer order by Name");
            _all = new Dictionary<int, Advertizer>();
            foreach (var m in mm)
            {
                _all.Add(m.Id, m);
            }
        }

        public void ReadFromDb(SqlDataReader rdr)
        {
            Id = (int)rdr["Id"];
            Name = (string)rdr["Name"];
            ContactName = (string)Utility.ConvertDbNull(rdr["ContactName"]);
            Phone = (string)Utility.ConvertDbNull(rdr["Phone"]);
            Fax = (string)Utility.ConvertDbNull(rdr["Fax"]);
            Email = (string)Utility.ConvertDbNull(rdr["Email"]);
        }

        public void Save()
        {
            ExecStoredProc("SaveAdvertizer", (cmd) =>
            {
                if (Id > 0) cmd.Parameters.AddWithValue("@id", Id);
                cmd.Parameters.AddWithValue("@name", Name);
                cmd.Parameters.AddWithValue("@contactName", ContactName);
                cmd.Parameters.AddWithValue("@phone", Phone);
                cmd.Parameters.AddWithValue("@fax", Fax);
                cmd.Parameters.AddWithValue("@email", Email);
            });
        }

        public void Revert()
        {
            ReadSql($"select * from [Advertizer] where Id = {Id}", ReadFromDb);
        }

        public override string ToString()
        {
            return Name; 
        }

    }
}
