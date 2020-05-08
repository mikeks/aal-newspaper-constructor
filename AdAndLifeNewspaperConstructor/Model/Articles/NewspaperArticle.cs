using HtmlAgilityPack;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VitalConnection.AAL.Builder.Model.Articles
{
	public class NewspaperArticle : DbObject, IDbObject
	{

		public int Id { get; private set; }
		public int IssueYear { get; set; }
		public int IssueNumber { get; set; }
		public bool UsePreviewImage { get; private set; }
		public bool IsSynced { get; private set; }


		public NewspaperArticle()
		{

		}

		public NewspaperArticle(int issueYear, int issueNumber)
		{
			IssueYear = issueYear;
			IssueNumber = issueNumber;
		}


		//public string NewspaperDescr => $"{Newspaper.NewspaperName} №{IssueNumber} ({IssueYear})";

		public string Name { get; set; }
		public string Author { get; set; }
		public ArticleRubric Rubric { get; set; }

		public DateTime Created { get; set; }

		public string Html { get; private set; }

		public int ImageCount { get; private set; } = 0;
		//public bool SyncedWithSite { get; private set; } = false;


		public string ParsingErrorMessage { get; private set; }

		public void ReadFromDb(SqlDataReader rdr)
		{
			Id = (int)rdr["Id"];
			Name = (string)rdr["Name"];
			Author = (string)ResolveDbNull(rdr["Author"]);
			IssueYear = (int)rdr["IssueYear"];
			IssueNumber = (short)rdr["IssueNumber"];

			Created = (DateTime)rdr["Created"];
			UsePreviewImage = (bool)rdr["UsePreviewImage"];
			IsSynced = (bool)rdr["IsSynced"];

			var rubricId = (int)rdr["RubricId"];
			Rubric = ArticleRubric.All.FirstOrDefault((x) => x.Id == rubricId);

			Html = (string)rdr["Txt"];

			//SyncedWithSite = (bool)rdr["SyncedWithSite"];

		}


		public void Save()
		{
			var articleId = ExecStoredProcWithReturnValue("SaveArticle", (cmd) =>
			{
				if (Id > 0) cmd.Parameters.AddWithValue("@id", Id);
				cmd.Parameters.AddWithValue("@name", Name);
				cmd.Parameters.AddWithValue("@author", Author);
				cmd.Parameters.AddWithValue("@issueNumber", IssueNumber);
				cmd.Parameters.AddWithValue("@issueYear", IssueYear);
				cmd.Parameters.AddWithValue("@rubricId", Rubric.Id);
				cmd.Parameters.AddWithValue("@usePreviewImage", UsePreviewImage);
				cmd.Parameters.AddWithValue("@txt", Html);
			});

			if (articleId == 0) throw new Exception("Не удалось сохранить статью.");

			ExecSQL($"delete from WebsiteArticleImage where ArticleId = {articleId}");


			for (int i = 1; i <= ImageCount; i++)
			{
				var data = File.ReadAllBytes(GetImageFilename(i));

				ExecSQL($"insert WebsiteArticleImage (ArticleId, Num, Data) values ({articleId}, {i}, @data)", (cmd) =>
				{
					cmd.Parameters.AddWithValue("@data", data);
				});
			}

		}

		public void LoadImagesFromDb()
		{
			if (Id == 0) return; // not saved

			foreach (var file in Directory.GetFiles($@"ArticlePreview\ArticleImages", "*.jpg"))
			{
				File.Delete(file);
			}

			ReadSql($"select Num, Data from WebsiteArticleImage where ArticleId = {Id}", (rdr) =>
			{
				var num = (byte)rdr["num"];
				var data = (byte[])rdr["data"];
				File.WriteAllBytes(GetImageFilename(num), data);
			});

		}


		public void Delete()
		{
			ExecStoredProc($"DeleteArticle", (cmd) => cmd.Parameters.AddWithValue("@id", Id));
		}


		public static NewspaperArticle[] GetArticles(int issueYear, int issueNumber)
		{
			return ReadCollectionFromDb<NewspaperArticle>($"select * from WebsiteArticle where IssueNumber = {issueNumber} and IssueYear = {issueYear} order by Name");
		}


		public Bitmap ResizeImage(Bitmap image, int width, int height)
		{
			Bitmap img = new Bitmap(width, height);
			Graphics g = Graphics.FromImage(img);
			g.InterpolationMode = InterpolationMode.High;
			g.DrawImage(image, 0, 0, width, height);
			return img;
		}

		private Bitmap ResizeImage(Bitmap bitmap)
		{
			const int maxW = 400;
			const int maxH = 400;
			if (bitmap.Width > bitmap.Height)
			{
				if (bitmap.Width > maxW)
				{
					return ResizeImage(bitmap, maxW, bitmap.Height * maxW / bitmap.Width);
				}
			}
			else
			{
				if (bitmap.Height > maxH)
				{
					return ResizeImage(bitmap, bitmap.Width * maxH / bitmap.Height, maxH);
				}
			}
			return ResizeImage(bitmap, bitmap.Width, bitmap.Height);
		}

		private static ImageCodecInfo GetEncoderInfo(string mimeType)
		{
			int j;
			ImageCodecInfo[] encoders;
			encoders = ImageCodecInfo.GetImageEncoders();
			for (j = 0; j < encoders.Length; ++j)
			{
				if (encoders[j].MimeType == mimeType)
					return encoders[j];
			}
			return null;
		}

		private string GetImageFilename(int imgNum) => $@"ArticlePreview\ArticleImages\{imgNum}.jpg";

		private void SaveErrorReport(string s)
		{
			Directory.CreateDirectory(@"c:\NewspaperBuilderErrors");
			var dt = DateTime.Now.ToString("dd-MM-yyyy hh-mm-ss");
			File.WriteAllText($@"c:\NewspaperBuilderErrors\{dt}.log", s);
		}

		private string DocFilename;

		private bool CheckForErrors(string rawHtml, HtmlDocument doc)
		{
			if (doc.ParseErrors.Count() == 0) return true;

			var sb = new StringBuilder();
			sb.AppendLine("Failure processing article.");
			sb.AppendLine($"File: {DocFilename}");
			foreach (var err in doc.ParseErrors)
			{
				sb.AppendLine($"Position: {err.StreamPosition}. {err.Reason}");
			}
			sb.AppendLine();
			sb.AppendLine(rawHtml);
			SaveErrorReport(sb.ToString());

			// + doc.ParseErrors.First().Reason
			ParsingErrorMessage = $"Ошибка обработки документа! Обратитесь за помощью.";
			return false;
		}


		public void UploadDocFile(string filename)
		{
			Html = null;
			ImageCount = 0;
			DocFilename = filename;
			UsePreviewImage = false;
			HTMLConverter converter = new HTMLConverter();
			string htmlContent;



			try
			{
				var imgNum = 1;
				htmlContent = converter.ConvertToHtml(filename, (imageInfo) =>
				{
					Directory.CreateDirectory(@"ArticlePreview\ArticleImages");
					var bitmap = ResizeImage(imageInfo.Bitmap);
					var pars = new EncoderParameters(1);
					pars.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 75L);
					var codecInfo = GetEncoderInfo("image/jpeg");
					var imgFilename = GetImageFilename(imgNum);
					bitmap.Save(imgFilename, codecInfo, pars);
					if (imgNum == 1 && bitmap.Width > bitmap.Height) UsePreviewImage = true;
					ImageCount++;
					var imgTag = new XElement("p", $"ArticleImage{imgNum}");
					imgNum++;
					return imgTag;
				});

			}
			catch (Exception ex)
			{
				ParsingErrorMessage = ex.Message;
				return;
			}

			string clearHtml;
			try
			{
				clearHtml = ClearHtml(htmlContent);
				//clearHtml = ProcessHeaders(clearHtml);
				//if (clearHtml == null) return; // error already set
				clearHtml = ProcessImages(clearHtml);
			}
			catch (Exception ex)
			{
				ParsingErrorMessage = ex.Message;
				return;
			}

			// Validate HTML
			var doc = new HtmlDocument();
			doc.LoadHtml(clearHtml);
			
			if (!CheckForErrors(clearHtml, doc)) return;

			//clearHtml = doc.DocumentNode.OuterHtml;


			var m = Regex.Matches(clearHtml, "<h1>(.*?)</h1>");
			if (m.Count > 1)
			{
				ParsingErrorMessage = $"Ошибка! Допускается только один заголовок первого уровня. А их тут целых {m.Count}. Уберите лишние, пожалуйста.";
				return;
			}
			if (m.Count == 0)
			{
				ParsingErrorMessage = "Ошибка! В статье отсутствует заголовок первого уровня. Добавьте его, пожалуйста и загрузите заново.";
				return;
			}

			Html = clearHtml;
			var nm = m[0].Groups[1].Value.Trim();
			Name = nm.Length > 200 ? nm.Substring(0, 200).Trim() : nm;

		}

		private string ProcessImages(string text)
		{
			var imgNum = 1;
			int p;

			string ArtPlaceholder(int i) => $"<p>ArticleImage{i}</p>";

			while ((p = text.IndexOf(ArtPlaceholder(imgNum))) != -1)
			{
				var bitmap = Image.FromFile($@"ArticlePreview\ArticleImages\{imgNum}.jpg");

				var posNextImg = text.IndexOf(ArtPlaceholder(imgNum + 1));
				if (posNextImg == -1) posNextImg = text.Length - 1;

				string caption = "";
				foreach (Match capM in Regex.Matches(text, "&gt;&gt;([^<]+)"))
				{
					if (capM.Index > p && capM.Index < Math.Min(posNextImg, p + 100))
					{
						caption = capM.Groups[1].Value;
						break;
					}
				}
				// style='width:{bitmap.Width}px'
				var imageHtml = $"<div class='article-image-box'><img src=\"ArticleImages/{imgNum}.jpg\" width=\"{bitmap.Width}\" height=\"{bitmap.Height}\" title=\"{caption}\"><div style='max-width:{bitmap.Width}px'>{caption}</div></div><table class='art-img-fix'></table>";

				text = text.Replace(ArtPlaceholder(imgNum), imageHtml);
				imgNum++;
			}

			text = Regex.Replace(text, "&gt;&gt;([^<]+)", "");

			// Remove <p> tag around image (if any)
			text = Regex.Replace(text, "<p> *<div class='article-image-box'>", "<div class='article-image-box'>");
			text = Regex.Replace(text, "<table class='art-img-fix'></table> *</p>", "<table class='art-img-fix'></table>");

			return text;
		}

		//private string ProcessHeaders(string html)
		//{
		//	var doc = new HtmlDocument();
		//	doc.LoadHtml(html);

		//	if (!CheckForErrors(html, doc)) return null;

		//	var headers = doc.DocumentNode.SelectNodes("//span[@class='header']");
		//	foreach (var header in headers)
		//	{



		//	}


		//	return html;
		//}

		private string ClearHtml(string html)
		{
			var body = Regex.Match(html, "<body>(.*)<\\/body>", RegexOptions.IgnoreCase).Groups[1].Value;
			var authors = "";


			body = body.Replace("[", "&opsq;").Replace("]", "&clsq;");
			body = body.Replace("", " "); // remove broken symbols
			body = body.Replace("<br />", "</p><p>");

			//body = Regex.Replace(body, "<h([1-4])[^>]*>", "[[h$1]]", RegexOptions.IgnoreCase);
			//body = Regex.Replace(body, "</h([1-4])>", "[[/h$1]]", RegexOptions.IgnoreCase);
			body = Regex.Replace(body, "<p[^>]*>", "[[p]]", RegexOptions.IgnoreCase).Replace("</p>", "[[/p]]");

			body = Regex.Replace(body, "<p[^>]*>", "[[p]]", RegexOptions.IgnoreCase).Replace("</p>", "[[/p]]");

			// underline and bold
			body = Regex.Replace(body, "<span[^>]*text-decoration: *underline[^>]*>(.*?)<\\/span>", "[[u]]$1[[/u]]", RegexOptions.IgnoreCase);
			body = Regex.Replace(body, "<span[^>]*font-weight: *bold[^>]*>(.*?)<\\/span>", "[[b]]$1[[/b]]", RegexOptions.IgnoreCase);
			body = Regex.Replace(body, "<span[^>]*font-style: *italic[^>]*>(.*?)<\\/span>", "[[i]]$1[[/i]]", RegexOptions.IgnoreCase);

			// remove all tags, except for ones processed above
			body = Regex.Replace(body, "<.*?>", "");
			body = Regex.Replace(body, "\\[\\[(.*?)\\]\\]", "<$1>");
			body = body.Replace("&opsq;", "[").Replace("&clsq;", "]");

			body = Regex.Replace(body, "\\s+", " ");

			body = body
				.Replace("&#x200e;", "")
				.Replace("<b></b>", "")
				.Replace("<b> </b>", " ")
				.Replace("<u></u>", "")
				.Replace("<u> </u>", " ")
				.Replace("<i></i>", "")
				.Replace("<i> </i>", " ")
				
				.Replace("</i><i>", "")
				.Replace("</u><u>", "")
				.Replace("</b><b>", "")
				.Replace("</i> <i>", " ")
				.Replace("</u> <u>", " ")
				.Replace("</b> <b>", " ")
				
				.Replace("<p> </p>", " ")
				.Replace("<p></p>", "");


			// Poetry markup: [[[Poetry]]]
			body = Regex.Replace(body, $"\\[\\[(.*?)\\]\\]", (m) =>
			{
				var inner = m.Groups[1].Value.Replace("</p><p>", "<br>");
				return inner;
			});

			// [R] - for right aligned paragraph
			if (body.Replace("<p>[R]</p>", "<p>[R]</p> ").Length - body.Length != body.Replace("<p>[/R]</p>", "<p>[/R]</p> ").Length - body.Length)
			{
				throw new Exception("Несовпадение тэгов [R]. Учтите, что каждый такой тег должен быть на отдельной строке.");
			}

			body = body.Replace("<p>[R]</p>", "<div class='right-par'>").Replace("<p>[/R]</p>", "</div>");

			body = Regex.Replace(body, "<p>A&gt;(.*)</p>", (m) =>
			{
				var a = m.Groups[1].Value;
				if (authors.Length > 0) authors += ", ";
				authors += a;
				return $"<p class='right-par author'>{a}</p>";
			});

			//body = body.Replace("<p>1&gt;[R]</p>", "<div class='right-par'>").Replace("<p>[/R]</p>", "</div>");

			body = body
				//.Replace("<p><p>", "<p>") // for article images
				//.Replace("<p> <p>", "<p>")
				//.Replace("</p> </p>", "</p>")
				//.Replace("</p></p>", "</p>")
				.Replace("<p></p>", "")
				.Replace("<p> </p>", "");

			body = Regex.Replace(body, "\\s+", " ");


			// Header format: 1>, 2>, 3>
			for (int i = 3; i > 0; i--)
			{
				//body = body.Replace($"{i}&gt;", $"<span class='header'>{i}</span>");
				body = Regex.Replace(body, $"{i}&gt;([^<]*)", $"<h{i}>$1</h{i}>");
				body = Regex.Replace(body, $"<b> *(<h{i}>.*?</h{i}>) *</b>", "$1");
				body = Regex.Replace(body, $"<i> *(<h{i}>.*?</h{i}>) *</i>", "$1");
				body = Regex.Replace(body, $"<p> *(<h{i}>.*?</h{i}>) *</p>", "$1");
				body = body.Replace($"<h{i}>", $"<div style='clear:both'></div><h{i}>");
			}

			if (authors.Length > 0) Author = authors;


			return body;
		}

		/*
private void ImageHandler()
{
	++imageCounter;


	string extension = imageInfo.ContentType.Split('/')[1].ToLower();
	ImageFormat imageFormat = null;
	if (extension == "png") imageFormat = ImageFormat.Png;
	else if (extension == "gif") imageFormat = ImageFormat.Gif;
	else if (extension == "bmp") imageFormat = ImageFormat.Bmp;
	else if (extension == "jpeg") imageFormat = ImageFormat.Jpeg;
	else if (extension == "tiff")
	{
		extension = "gif";
		imageFormat = ImageFormat.Gif;
	}
	else if (extension == "x-wmf")
	{
		extension = "wmf";
		imageFormat = ImageFormat.Wmf;
	}

	if (imageFormat == null)
		return null;

	string base64 = null;
	try
	{
		using (MemoryStream ms = new MemoryStream())
		{
			imageInfo.Bitmap.Save(ms, imageFormat);
			var ba = ms.ToArray();
			base64 = System.Convert.ToBase64String(ba);
		}
	}
	catch (System.Runtime.InteropServices.ExternalException)
	{ return null; }


	ImageFormat format = imageInfo.Bitmap.RawFormat;
	ImageCodecInfo codec = ImageCodecInfo.GetImageDecoders().First(c => c.FormatID == format.Guid);
	string mimeType = codec.MimeType;

	string imageSource = string.Format("data:{0};base64,{1}", mimeType, base64);

	XElement img = new XElement(Xhtml.img,
		new XAttribute(NoNamespace.src, imageSource),
		imageInfo.ImgStyleAttribute,
		imageInfo.AltText != null ?
			new XAttribute(NoNamespace.alt, imageInfo.AltText) : null);
	return img;
}
*/


	}
}
