using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VitalConnection.AAL.Builder
{
	static class ImageLoader
	{

		private static Task<BitmapImage> LoadImage(Uri imageUri)
		{
			return Task.Run(() =>
			{
				try
				{
					var bitmapImage = new BitmapImage
					{
						CacheOption = BitmapCacheOption.OnLoad
					};
					bitmapImage.BeginInit();
					bitmapImage.UriSource = imageUri;
					bitmapImage.EndInit();
					bitmapImage.Freeze();
					return bitmapImage;
				}
				catch (Exception)
				{
					return null;
				}
			});
		}

		public async static Task<BitmapImage> LoadImage(string path, bool useNotFoundImage = true)
		{
            BitmapImage imgSrc = null;
            if (!string.IsNullOrWhiteSpace(path))
			    imgSrc = await LoadImage(new Uri(path));
            
			if (imgSrc == null && useNotFoundImage)
			{
				imgSrc = await LoadImage(new Uri("NotFound.png", UriKind.Relative));
			}

			return imgSrc;
		}

	}
}
