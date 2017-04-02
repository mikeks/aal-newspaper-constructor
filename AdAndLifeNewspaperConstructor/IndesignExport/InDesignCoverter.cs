using InDesign;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalConnection.AAL.Builder.Model;
using VitalConnection.AAL.Builder.Properties;

namespace VitalConnection.AAL.Builder.IndesignExport
{
    public class InDesignCoverter
    {

        

        public void CreateIssue(Issue issue)
        {

            if (!issue.Validate()) return;

            StringBuilder sbLog = new StringBuilder();

            Action<string> Log = (s) =>
            {
                sbLog.AppendLine($"{DateTime.Now}. {s}");
            };

            Log("Start converting...");

            var d = new AdAndLifeInDesignScript(issue);
            var scr = d.TransformText();

            Log("Script generated.");

            var scrFn = Settings.Default.InDesignScriptFilename;
            if (!string.IsNullOrEmpty(scrFn))
            {
                try
                {
                    Log($"Start saving script in file: \"{scrFn}\"...");
                    File.WriteAllText(scrFn, scr);
                    Log("Script saved.");
                }
                catch (Exception e)
                {
                    Log($"Script not saved. Error: {e.Message}");
                }
            }


            Type oType;
            try
            {
                Log("Connecting to InDesign...");
                oType = Type.GetTypeFromProgID("InDesign.Application");
                Log("Connected to InDesign.");
            }
            catch (Exception e)
            {
                Log($"Connection failed. Script won't be executed. {e.Message}");
                return;
            }

			dynamic instance = null;

			try
			{
				instance = Activator.CreateInstance(oType); // or any other way you can get it
			}
			catch (Exception e)
			{
				Log($"Execution when creating InDesign instance. {e.Message}");
				System.Windows.MessageBox.Show("Кажется, что-то пошло не так...", "Ой", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk);
			}

			if (instance != null)
			{
				try
				{
					Log("Executing InDesign script...");
					instance.DoScript(scr, idScriptLanguage.idVisualBasic, new object[] { "" });
					Log("Script executed successfully.");
				}
				catch (Exception e)
				{
					Log($"Script execution exception received (it may be still OK). {e.Message}");
				}
			} else
				Log($"Instance is null. Can't process.");

			File.WriteAllText("inDesignConvertLog.txt", sbLog.ToString());

        }

    }
}
