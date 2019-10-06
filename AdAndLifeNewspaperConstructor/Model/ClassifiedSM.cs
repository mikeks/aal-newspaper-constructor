using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalConnection.AAL.Builder.Model
{
	public class ClassifiedSM 
	{

		private static List<string> _all;

		public static List<string> All
		{
			get
			{
				if (_all == null)
				{
					try
					{
						_all = File.ReadAllLines("sm.txt").ToList();
					}
					catch (Exception)
					{
						_all = new List<string>();
					}
				}
				return _all;
			}
		}

		private static void Save()
		{
			File.WriteAllLines("sm.txt", _all);
		}

		public static void Add(string name)
		{
			_all.Add(name);
			Save();
		}

		public static void Remove(string name)
		{
			_all.Remove(name);
			Save();
		}


	}
}
