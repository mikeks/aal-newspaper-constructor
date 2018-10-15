using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VitalConnection.AAL.Builder.Model;

namespace VitalConnection.AAL.Builder.WebsiteAdmin
{
	class WebsiteAdminHelper
	{

		private static void CopyFile(string filename)
		{
			using (WebClient client = new WebClient())
			{
				client.Credentials = new NetworkCredential("mikeks", "e409MB178RUS");
				var fn = Path.GetFileName(filename);
				client.UploadFile($"ftp://chtogdekogda.org/{fn}", "STOR", filename);
			}
		}

		public static void CopyAdsToFTP(Issue issue)
		{


			foreach (var p in issue.Pages)
			{

				foreach (var ad in p.AdModules)
				{
					var ph = Utility.ConvertFilePath(ad.AdModule.FullPath);
					if (ph == null) continue;
					if (!File.Exists(ph)) continue;

					CopyFile(ph);

				}

			}
		}

	}
}
