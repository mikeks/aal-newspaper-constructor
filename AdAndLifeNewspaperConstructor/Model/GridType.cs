using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalConnection.AAL.Builder.Model
{

    public enum GridIdEnum
    {
        Main = 1,
        First = 2,
        Last = 3,
        Classified = 4
    }

    public class GridType : DbObject, IDbObject
    {

        public GridIdEnum Id { get; set; }
        public string Name { get; set; }
        public int ColumnsCount { get; set; }
        public int RowCount { get; set; }
        public bool IsForClassified { get; set; }

        // for InDesign
        public float MarginInside { get; set; }
        public float MarginOutside { get; set; }
        public float MarginTop { get; set; }
        public float MarginBottom { get; set; }
        public float Gap { get; set; }

        public void ReadFromDb(SqlDataReader rdr)
        {
            Id = (GridIdEnum)(int)rdr["Id"];
            Name = (string)rdr["Name"];
            ColumnsCount = (short)rdr["Columns"];
            RowCount = (short)rdr["Rows"];
            IsForClassified = (bool)rdr["IsForClassified"];
            MarginInside = (float)rdr["MarginInside"];
            MarginOutside = (float)rdr["MarginOutside"];
            MarginTop = (float)rdr["MarginTop"];
            MarginBottom = (float)rdr["MarginBottom"];
            Gap = (float)rdr["Gap"];
        }

        private static GridType[] ReadAllFromDb()
        {
            List<GridType> ss = new List<GridType>();
            ReadSql("select * from Grid", (rdr) => {
                var gt = new GridType();
                gt.ReadFromDb(rdr);
                ss.Add(gt);
            });
            return ss.ToArray();
        }

        private static GridType[] _allTypes = ReadAllFromDb();

        public static GridType[] AllTypes
        {
            get
            {
                return _allTypes;
            }
        }

        public static void ReloadAllFromDb()
        {
            _allTypes = ReadAllFromDb();
        }


        public static GridType GetGridType(GridIdEnum id)
        {
            return _allTypes.FirstOrDefault((x) => x.Id == id);
        }

        public override string ToString()
        {
            return Name;
        }


    }
}
