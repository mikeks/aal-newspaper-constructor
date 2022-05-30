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

		public bool CanSelect => AdModule?.Advertiser?.Name != null && AdModule?.Price > 0;

		public string AdSizeDescription
		{
			get
			{
				switch (Page.Grid.Id)
				{
					case GridIdEnum.Main:
					case GridIdEnum.Last:
						if (AdModule.Width == 2 && AdModule.Height == 2) return "1/4";
						if (AdModule.Width == 4 && AdModule.Height == 2) return "1/2";
						if (AdModule.Width == 2 && AdModule.Height == 4) return "1/2";
						if (AdModule.Width == 4 && AdModule.Height == 4) return "Full page";
						if (AdModule.Width == 2 && AdModule.Height == 1) return "1/8";
						if (AdModule.Width == 1 && AdModule.Height == 2) return "1/8";
						if (AdModule.Width == 1 && AdModule.Height == 1) return "1/16";
						return "";
					case GridIdEnum.First:
						return (AdModule.Width * AdModule.Height).ToString() + "BC";
					case GridIdEnum.Classified:
						return "Classified";
					default:
						return "";
				}
			}
		}

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

			var a = new AdModuleOnPage
			{
				AdModule = adModule,
				AdModuleId = adModule.Id,
				PageId = page.Id,
				Page = page,
				X = 1,
				Y = 1
			};
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
