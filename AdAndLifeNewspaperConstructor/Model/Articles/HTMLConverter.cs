using System.IO;
using System.Linq;
using OpenXmlPowerTools;
using DocumentFormat.OpenXml.Packaging;
using System.Drawing.Imaging;
using System.Xml.Linq;
using System;

namespace VitalConnection.AAL.Builder.Model.Articles
{

	public class HTMLConverter
	{


		public string ConvertToHtml(string fullFilePath, Func<ImageInfo, XElement> imageHandler)
		{
			if (string.IsNullOrEmpty(fullFilePath) || Path.GetExtension(fullFilePath) != ".docx")
				throw new Exception("Неверный формат файла.");

			FileInfo fileInfo = new FileInfo(fullFilePath);

			string htmlText = string.Empty;
			try
			{
				htmlText = ParseDOCX(fileInfo, imageHandler);
			}
			catch (OpenXmlPackageException e)
			{

				if (e.ToString().Contains("Invalid Hyperlink"))
				{
					using (FileStream fs = new FileStream(fullFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
					{
						UriFixer.FixInvalidUri(fs, brokenUri => FixUri(brokenUri));
					}
					htmlText = ParseDOCX(fileInfo, imageHandler);
				}
			}


			return htmlText;


		}

		private static string FixUri(string brokenUri)
		{
			string newURI = string.Empty;

			if (brokenUri.Contains("mailto:"))
			{
				int mailToCount = "mailto:".Length;
				brokenUri = brokenUri.Remove(0, mailToCount);
				newURI = brokenUri;
			}
			else
			{
				newURI = " ";
			}
			return newURI;
		}


		private string ParseDOCX(FileInfo fileInfo, Func<ImageInfo, XElement> imageHandler)
		{

			byte[] byteArray;

			try
			{
				byteArray = File.ReadAllBytes(fileInfo.FullName);

			}
			catch
			{
				throw new Exception("Файл недоступен. Возможно, он открыт в другой программе.");
			}


			using (MemoryStream memoryStream = new MemoryStream())
			{
				memoryStream.Write(byteArray, 0, byteArray.Length);

				using (WordprocessingDocument wDoc = WordprocessingDocument.Open(memoryStream, true))
				{

					//int imageCounter = 0;

					var pageTitle = fileInfo.FullName;
					var part = wDoc.CoreFilePropertiesPart;
					if (part != null)
						pageTitle = (string)part.GetXDocument().Descendants(DC.title).FirstOrDefault() ?? fileInfo.FullName;

					WmlToHtmlConverterSettings settings = new WmlToHtmlConverterSettings()
					{
						//AdditionalCss = "body { margin: 1cm auto; max-width: 20cm; padding: 0; }",
						//PageTitle = pageTitle,
						FabricateCssClasses = false,
						//CssClassPrefix = "pt-",
						//RestrictToSupportedLanguages = false,
						//RestrictToSupportedNumberingFormats = false,
						ImageHandler = imageHandler
					};

					XElement htmlElement = WmlToHtmlConverter.ConvertToHtml(wDoc, settings);

					var html = new XDocument(new XDocumentType("html", null, null, null), htmlElement);
					var htmlString = html.ToString(SaveOptions.DisableFormatting);
					return htmlString;
				}
			}

		}


	}
}
