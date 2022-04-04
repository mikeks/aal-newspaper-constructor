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

        public static object ConvertDbNull(object x, object defalutValue = null)
        {
            return x is DBNull ? defalutValue : x;
        }

		public static string ConvertFilePath(string ph)
		{
            if (!Properties.Settings.Default.IsConvertPath) return ph;
			ph = ph.Replace(@"\\Server02\designer4\_TIFF images\", @"D:\_TIFF images\");
			ph = ph.Replace(@"Z:\_TIFF images\", @"D:\_TIFF images\");
			ph = ph.Replace(@"Z:\_Articles\", @"D:\_Articles\");
			ph = ph.Replace(@"\\Server02\designer4\_Classified\All Images\kartinki dlya rubrik\", @"D:\_TIFF images\ClassifiedImagesForRubrics\");

            if (ph.ToLower().StartsWith("z:") || ph.ToLower().StartsWith("\\\\server02")) return null;
            //if (!ph.StartsWith("D:")) Debugger.Break();
            return ph;
		}

    }

	public static class DateTimeExtensions
	{
		///<summary>Gets the first week day following a date.</summary>
		///<param name="date">The date.</param>
		///<param name="dayOfWeek">The day of week to return.</param>
		///<returns>The first dayOfWeek day following date, or date if it is on dayOfWeek.</returns>
		public static DateTime Next(this DateTime date, DayOfWeek dayOfWeek)
		{
			return date.AddDays((dayOfWeek < date.DayOfWeek ? 7 : 0) + dayOfWeek - date.DayOfWeek);
		}
	}

}
