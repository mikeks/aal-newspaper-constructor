using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalConnection.AAL.Builder.Model
{
	public class AdModuleOnPage : DbObject, IDbObject
	{
		public NewspaperPage Page { get; set; }
		public AdModule AdModule { get; private set; }
		public int AdModuleId { get; private set; }
		public int PageId { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		private bool _isSelected = true;

		public bool IsSelected
		{
			get
			{
				return CanSelect && _isSelected;
			}
			set
			{
				_isSelected = CanSelect && value;
			}
		}

		public bool CanSelect => AdModule?.Advertizer?.Name != null && AdModule?.Price > 0;

		public void ReadFromDb(SqlDataReader rdr)
		{
			PageId = (int)rdr["PageId"];
			AdModuleId = (int)rdr["AdModuleId"];
			AdModule = AdModule.GetAdModule(AdModuleId);
			X = (byte)rdr["X"];
			Y = (byte)rdr["Y"];
		}

		public static AdModuleOnPage PlaceOnPage(AdModule adModule, NewspaperPage page)
		{
			var am = page.AdModules.FirstOrDefault((x) => x.AdModuleId == adModule.Id);
			if (am != null) return am; // already added

			var a = new AdModuleOnPage();
			a.AdModule = adModule;
			a.AdModuleId = adModule.Id;
			a.PageId = page.Id;
			a.Page = page;
			a.X = 1;
			a.Y = 1;
			page.AdModules.Add(a);
			a.SaveNew();
			return a;
		}

		public void Delete()
		{
			Page.AdModules.Remove(this);
			ExecSQL("delete PageAdModule where PageId = @pageId and AdModuleId = @adModuleId", (cmd) =>
			{
				cmd.Parameters.AddWithValue("@pageId", PageId);
				cmd.Parameters.AddWithValue("@adModuleId", AdModuleId);
			});
		}

		public void SaveNew()
		{
			ExecSQL("insert PageAdModule (PageId, AdModuleId, X, Y) values (@pageId, @adModuleId, @x, @y)", (cmd) =>
			{
				cmd.Parameters.AddWithValue("@x", X);
				cmd.Parameters.AddWithValue("@y", Y);
				cmd.Parameters.AddWithValue("@pageId", PageId);
				cmd.Parameters.AddWithValue("@adModuleId", AdModuleId);
			});
		}

		public void SavePostionToDb(int newX, int newY)
		{
			X = newX;
			Y = newY;

			ExecSQL("update PageAdModule set X = @x, Y = @y where PageId = @pageId and AdModuleId = @adModuleId", (cmd) =>
			{
				cmd.Parameters.AddWithValue("@x", X);
				cmd.Parameters.AddWithValue("@y", Y);
				cmd.Parameters.AddWithValue("@pageId", PageId);
				cmd.Parameters.AddWithValue("@adModuleId", AdModuleId);
			});

		}
	}
}
