using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using VitalConnection.AAL.Builder.Model.Articles;
using VitalConnection.AAL.Builder.ViewModel;

namespace VitalConnection.AAL.Builder
{
	/// <summary>
	/// Interaction logic for EditArticleWindow.xaml
	/// </summary>
	public partial class EditArticleWindow : Window
	{
		public EditArticleWindow(NewspaperArticle article)
		{
			InitializeComponent();
			DataContext = new EditArticleViewModel(article);
			//PreviewBrowser.NavigateToString("<html><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8'></head><body style='border: solid 1px gray'><div style='text-align:center;margin-top:100px;'>Тут появится статья</div></body></html>");


			ViewModel.ShowHtml = () =>
			{
				var ph = Path.Combine(Environment.CurrentDirectory, "ArticlePreview/Article.htm");
				PreviewBrowser.Navigate("file:///" + ph);
			};

			ViewModel.Init();
		}

		private EditArticleViewModel ViewModel => DataContext as EditArticleViewModel;




	}
}
