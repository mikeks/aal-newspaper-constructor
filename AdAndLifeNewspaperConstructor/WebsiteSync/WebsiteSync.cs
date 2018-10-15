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
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(4, 0, 0);
            dispatcherTimer.Start();
        }

        private static void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            DoSync(true);
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

    }
}
