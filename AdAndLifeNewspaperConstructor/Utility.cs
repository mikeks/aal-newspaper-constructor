using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VitalConnection.AAL.Builder
{
	class Utility
	{

        public static object ConvertDbNull(object x)
        {
            return x is DBNull ? null : x;
        }

		public static string ConvertFilePath(string ph)
		{
            if (!Properties.Settings.Default.IsConvertPath) return ph;
			ph = ph.Replace(@"\\Server02\designer4\_TIFF images\", @"D:\_TIFF images\");
			ph = ph.Replace(@"Z:\_TIFF images\", @"D:\_TIFF images\");
			ph = ph.Replace(@"\\Server02\designer4\_Classified\All Images\kartinki dlya rubrik\", @"D:\_TIFF images\ClassifiedImagesForRubrics\");

            if (ph.ToLower().StartsWith("z:") || ph.ToLower().StartsWith("\\\\server02")) return null;
            //if (!ph.StartsWith("D:")) Debugger.Break();
            return ph;
		}

    }

}
