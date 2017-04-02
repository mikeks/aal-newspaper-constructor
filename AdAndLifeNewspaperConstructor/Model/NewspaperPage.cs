using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalConnection.AAL.Builder.Model
{
    public class NewspaperPage : DbObject, IDbObject
    {

		public int Id { get; private set; }
		public int Number { get; private set; }
		public int IsssueId { get; private set; }

        public NewspaperPage(int issueId)
        {
            IsssueId = issueId;
        }

        private GridType _grid;
        public GridType Grid { get
            {
                return _grid;
            }
            set
            {
                _grid = value;
                UpdateGridType();
            }
        }

        public void ReadFromDb(SqlDataReader rdr)
        {
			Id = (int)rdr["Id"];
            Number = (int)rdr["Number"];
            _grid = GridType.GetGridType((GridIdEnum)(int)rdr["GridId"]);
        }

		List<AdModuleOnPage> _adModules = null;

		private void ReadAdModules()
		{
			_adModules = ReadCollectionFromDb<AdModuleOnPage>($"select * from PageAdModule where PageId = {Id}").ToList();
			_adModules.ForEach((am) => { am.Page = this; });
		}

		public List<AdModuleOnPage> AdModules
		{
			get
			{
				if (_adModules == null) ReadAdModules();
				return _adModules;
			}
		}


        public override string ToString()
        {
            return Number.ToString();
        }

        private void UpdateGridType()
        {
            ExecStoredProc("UpdatePageType", (cmd) =>
            {
                cmd.Parameters.AddWithValue("@issueId", IsssueId);
                cmd.Parameters.AddWithValue("@pageNumber", Number);
                cmd.Parameters.AddWithValue("@gridId", _grid.Id);
            });
        }

    }
}
