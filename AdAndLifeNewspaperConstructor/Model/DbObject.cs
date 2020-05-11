using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VitalConnection.AAL.Builder.Model
{

    public class ConnectionProblemException : Exception { }


    public class DbObject : ObservableObject
    {



        protected static SqlConnection GetConnection()
        {
            var conn = new SqlConnection(Properties.Settings.Default.CS.Replace("{DB}", NewspaperDatabase.Current.DatabaseName));
            try
            {
                conn.Open();
                NewspaperDatabase.IsConnectionProblems = false;
                return conn;
            }
            catch
            {
                conn.Dispose();
                NewspaperDatabase.IsConnectionProblems = true;
                throw new ConnectionProblemException();
            }
        }

		protected static void ReadSql(string sql, Action<SqlDataReader> f, CommandType cmdType = CommandType.Text)
		{
			ReadSql(sql, null, f, cmdType);
		}


		protected static void ReadSql(string sql, Action<SqlCommand> addParAction, Action<SqlDataReader> f, CommandType cmdType = CommandType.Text)
        {
            try
            {
                using (var conn = GetConnection())
                {
					var cmd = new SqlCommand(sql, conn)
					{
						CommandType = cmdType
					};
					addParAction?.Invoke(cmd);
					using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            f(rdr);
                        }
                    }
                }
            }
            catch (ConnectionProblemException)
            {
            }
        }

        private static void _exec(string storedProcName, Action<SqlCommand> addParAction, CommandType cmdType)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    var cmd = new SqlCommand(storedProcName, conn) { CommandType = cmdType };
                    addParAction?.Invoke(cmd);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (ConnectionProblemException)
            {
            }
            catch (Exception e)
            {
                Directory.CreateDirectory(@"c:\NewspaperBuilderErrors");
                var dt = DateTime.Now.ToString("dd-MM-yyyy hh-mm-ss");
                var s = 
					$@"Stored proc: {storedProcName}
					Error: {e.Message}
					Inner: {e.InnerException?.Message}
					Stack: {e.StackTrace}
					";
                File.WriteAllText($@"c:\NewspaperBuilderErrors\{dt}.log", s);
                MessageBox.Show("Произошла ошибка.", "Ой...", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }


		protected int ExecStoredProcWithReturnValue(string sql, Action<SqlCommand> addParAction)
		{
			using (var conn = GetConnection())
			{
				var cmd = new SqlCommand(sql, conn)
				{
					CommandType = CommandType.StoredProcedure
				};
				addParAction?.Invoke(cmd);
				var rv = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
				rv.Direction = ParameterDirection.ReturnValue;
				cmd.ExecuteNonQuery();
				return (int)rv.Value;
			}
		}



		public static void ExecStoredProc(string storedProcName, Action<SqlCommand> addParAction)
        {
            _exec(storedProcName, addParAction, CommandType.StoredProcedure);
        }

        protected static void ExecSQL(string storedProcName, Action<SqlCommand> addParAction = null)
        {
            _exec(storedProcName, addParAction, CommandType.Text);
        }

        protected static T[] ReadCollectionFromDb<T>(string sql) where T : IDbObject, new()
        {
            List<T> ss = new List<T>();
            ReadSql(sql, (rdr) =>
            {
                var itm = new T();
                itm.ReadFromDb(rdr);
                ss.Add(itm);
            });
            return ss.ToArray();
        }

        protected static T[] ReadCollectionFromDb<T>(string sql, Func<T> factory) where T : IDbObject
        {
            List<T> ss = new List<T>();
            ReadSql(sql, (rdr) =>
            {
                var itm = factory();
                itm.ReadFromDb(rdr);
                ss.Add(itm);
            });
            return ss.ToArray();
        }

        protected object ResolveDbNull(object o, object def = null)
        {
            return o is DBNull ? def : o;
        }

    }
}
