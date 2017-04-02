using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalConnection.AAL.Builder.Model
{
    public interface IDbObject
    {
        void ReadFromDb(SqlDataReader rdr);
    }
}
