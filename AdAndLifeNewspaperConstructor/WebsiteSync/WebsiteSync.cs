using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VitalConnection.AAL.Builder.Model;
using VitalConnection.AAL.Builder.ViewModel;

namespace VitalConnection.AAL.Builder.WebsiteSync
{
    class WebsiteSync
    {


        public static void Init()
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
			dispatcherTimer.Tick += (sender, e) => DoSync(true);
            dispatcherTimer.Interval = new TimeSpan(4, 0, 0);
            dispatcherTimer.Start();
        }



        private static void DoSync(bool silent = false)
        {
            Task.Run(() =>
            {
                try
                {
                    DbObject.ExecStoredProc("ExportClassifiedTask", null);
                    if (!silent)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show("Синхронизация с сайтом успешно выполнена.");
                        });
                    }
                }
                catch (Exception e)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Ошибка синхронизации объявлений с сайтом AdAndLife. " + e.Message, "Проблемка тута...", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            });
        }

        public static void SyncClassified()
        {
            DoSync();
        }

		public static void SyncArticles(Action onFinished)
		{
			Task.Run(() =>
			{
				try
				{
					DbObject.ExecStoredProc("ExportArticlesTask", null);
					Application.Current.Dispatcher.Invoke(() =>
					{
						onFinished();
						MessageBox.Show("Синхронизация статей с сайтом успешно выполнена.", "Удача", MessageBoxButton.OK, MessageBoxImage.Information);
					});
				}
				catch (Exception e)
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						onFinished();
						MessageBox.Show("Ошибка синхронизации статей с сайтом AdAndLife. " + e.Message, "Проблемка тута...", MessageBoxButton.OK, MessageBoxImage.Error);
					});
				}
			});
			
		}

    }
}
