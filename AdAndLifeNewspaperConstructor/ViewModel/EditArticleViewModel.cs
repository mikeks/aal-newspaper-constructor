using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VitalConnection.AAL.Builder.Model;
using VitalConnection.AAL.Builder.Model.Articles;

namespace VitalConnection.AAL.Builder.ViewModel
{
	class EditArticleViewModel : ObservableObject
	{

		public NewspaperArticle Article { get; private set; }

		public EditArticleViewModel(NewspaperArticle article)
		{
			Article = article;
		}



		public void Init()
		{
			if (Article.Id == 0)
			{
				RefreshPreview();
				return;
			}

			// Load images
			RefreshPreview(true);
			var worker = new BackgroundWorker();
			worker.DoWork += (sender, e) =>
			{
				Article.LoadImagesFromDb();
			};
			worker.RunWorkerCompleted += (sender, e) =>
			{
				RefreshPreview();
			};
			worker.RunWorkerAsync();

		}

		public IEnumerable<ArticleRubric> AllRubrics => ArticleRubric.All;

		private Issue[] _allIssues;

		public IEnumerable<Issue> AllIssues
		{
			get
			{
				if (_allIssues == null) _allIssues = Issue.GetAllIssues();
				return _allIssues;
			}
		}

		public Issue CurrentIssue
		{
			get
			{
				return _allIssues.FirstOrDefault((x) => x.Number == Article.IssueNumber && x.Year == Article.IssueYear);
			}
			set
			{
				Article.IssueYear = value.Year;
				Article.IssueNumber = value.Number;
			}
		}

		public int CurrentIssueId
		{
			get
			{
				return CurrentIssue?.Id ?? 0;
			}

			set
			{
				CurrentIssue = AllIssues.First((x) => x.Id == value);
			}
		}


		public string ArticleName
		{
			get
			{
				return Article.Name;
			}
			set
			{
				Article.Name = value;
			}
		}

		public string Author
		{
			get
			{
				return Article.Author;
			}
			set
			{
				Article.Author = value;
			}
		}



		public ICommand UploadArticleCommand => new DelegateCommand<Window>((w) =>
		{
			var dialog = new Microsoft.Win32.OpenFileDialog
			{
				DefaultExt = ".docx", 
				Filter = "Word Document (.docx)|*.docx" 
			};

			var result = dialog.ShowDialog();
			if (result == true)
			{
				RefreshPreview(true);

				var worker = new BackgroundWorker();
				worker.DoWork += (sender, e) =>
				{
					Article.UploadDocFile(dialog.FileName);
				};
				worker.RunWorkerCompleted += (sender, e) =>
				{
					RefreshPreview();
					RaisePropertyChangedEvent("ArticleName");
					RaisePropertyChangedEvent("Author");
				};
				worker.RunWorkerAsync();
			}

		});

		public Action ShowHtml;



		
		private void RefreshPreview(bool isProcessingNow = false)
		{
			string html;
			if (isProcessingNow)
			{
				html = $"<div style='font-size:24px;text-align:center;margin-top:50px'><img src='wait100.gif'><br>Статья загружается...</div>";
			}
			else if (Article.Html != null)
			{
				html = Article.Html;
			}
			else if (Article.ParsingErrorMessage != null)
			{
				var err = Article.ParsingErrorMessage.Replace("<", "&lt;").Replace(">", "&gt;");
				html = $"<div style='color:red;font-size:24px;text-align:center;margin-top:50px'><img width=100 src='error.png'><br>{err}</div>";
			} else
			{
				html = File.ReadAllText(@"ArticlePreview\FormattingRules.htm");
			}

			var template = File.ReadAllText("ArticlePreview/ArticleTemplate.htm");
			var fullHtml = template.Replace("[Content]", html);
			File.WriteAllText("ArticlePreview/Article.htm", fullHtml);
			ShowHtml();

		}


		public ICommand SaveCommand => new DelegateCommand<Window>((w) =>
		{
			if (Article.Html == null)
			{
				MessageBox.Show("Нельзя сохранить статью, потому что текст отсуствует (совсем).", "Низзя!", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			if (string.IsNullOrWhiteSpace(Article.Name))
			{
				MessageBox.Show("Нельзя сохранить статью, потому что названия нет.", "Низзя!", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			if (Article.Rubric == null)
			{
				MessageBox.Show("Нельзя сохранить статью, потому что не указана рубрика.", "Низзя!", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			try
			{
				Article.Save();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Произошла неожиданная ошибка при сохранении статьи. " + ex.Message, "Случилось страшное", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			MainViewModel.Instance.RefreshArticles();
			w.Close();
		});

		public ICommand CancelCommand => new DelegateCommand<Window>((w) =>
		{
			w.Close();
		});



	}
}
