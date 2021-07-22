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
        private Advertizer _advertiser;
        public Advertizer Advertiser { 
            get
            {
                return _advertiser;
            }
            set
            {
                _advertiser = value;
                RaisePropertyChangedEvent("AdvertizerName");
            }
        }
        public int Width { get; set; }
        public int Height { get; set; }
        public decimal Price { get; set; }

        public GridType Grid { get; set; }

        public string AdvertiserName => Advertiser?.Name ?? "не указан";

        public string Name {
            get
            {
                return Path.GetFileNameWithoutExtension(FullPath);
            }
        }

        public AdModule()
        {
            //Advertizer = Advertizer.All.FirstOrDefault();
        }

        public void ReadFromDb(SqlDataReader rdr)
        {
            Id = (int)rdr["Id"];
            FullPath = (string)rdr["FileName"];
            var advertId = (int?)Utility.ConvertDbNull(rdr["AdvertizerId"]);
            if (advertId.HasValue) Advertiser = Advertizer.GetById(advertId.Value);
            Width = (short)rdr["Width"];
            Height = (short)rdr["Height"];
            Price = (decimal)Utility.ConvertDbNull(rdr["Price"], (decimal)0);
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
                if (Advertiser != null) cmd.Parameters.AddWithValue("@advertizerId", Advertiser.Id);
                cmd.Parameters.AddWithValue("@price", Price);
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
			if (!_adModuleDictionary.TryGetValue(id, out AdModule m)) return null;
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
