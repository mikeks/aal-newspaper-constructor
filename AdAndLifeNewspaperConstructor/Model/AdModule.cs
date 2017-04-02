using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalConnection.AAL.Builder.Model
{
    public class AdModule : DbObject, IDbObject
    {

        public int Id { get; private set; }
        public string FullPath { get; set; }
        public Advertizer Advertizer { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public GridType Grid { get; set; }

        public string Name {
            get
            {
                return Path.GetFileNameWithoutExtension(FullPath);
            }
        }

        public AdModule()
        {
            Advertizer = Advertizer.All.FirstOrDefault();
        }

        public void ReadFromDb(SqlDataReader rdr)
        {
            Id = (int)rdr["Id"];
            FullPath = (string)rdr["FileName"];
            var advertId = (int)rdr["AdvertizerId"];
            Advertizer = Advertizer.GetById(advertId);
            Width = (short)rdr["Width"];
            Height = (short)rdr["Height"];
            Grid = GridType.GetGridType((GridIdEnum)(int)rdr["GridId"]);
        }



		private static AdModule[] _all;
		private static Dictionary<int, AdModule> _adModuleDictionary;

        public static void ReloadAllFromDb()
        {
            _all = null;
        }

		private static void Load()
		{
			_all = ReadCollectionFromDb<AdModule>("select * from AdModule");
			_adModuleDictionary = new Dictionary<int, AdModule>();
			foreach (var m in _all)
			{
				_adModuleDictionary.Add(m.Id, m);
			}
		}



        public void Save()
        {
            ExecStoredProc("SaveAdModule", (cmd) =>
            {
                if (Id > 0) cmd.Parameters.AddWithValue("@id", Id);
                cmd.Parameters.AddWithValue("@filename", FullPath);
                cmd.Parameters.AddWithValue("@width", Width);
                cmd.Parameters.AddWithValue("@height", Height);
                cmd.Parameters.AddWithValue("@gridId", Grid.Id);
                cmd.Parameters.AddWithValue("@AdvertizerId", Advertizer.Id);
            });
        }

        public void Delete()
        {
            ExecStoredProc("DeleteAdModule", (cmd) =>
            {
                cmd.Parameters.AddWithValue("@id", Id);
            });
        }


        public static AdModule GetAdModule(int id)
		{
			if (_all == null) Load();
			AdModule m;
			if (!_adModuleDictionary.TryGetValue(id, out m)) return null;
			return m;
		}


		public static AdModule[] AllModules
        {
            get
			{
				if (_all == null) Load();
				return _all;
			}
        }


        public override string ToString()
        {
            return Name;
        }


		

    }
}
