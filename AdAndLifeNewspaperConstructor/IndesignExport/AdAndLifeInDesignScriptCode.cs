using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalConnection.AAL.Builder.Model;
using static VitalConnection.AAL.Builder.Model.ClassifiedAd;

namespace VitalConnection.AAL.Builder.IndesignExport
{
    public partial class AdAndLifeInDesignScript
    {

        Issue _issue;


        

        public AdAndLifeInDesignScript(Issue issue)
        {
            _issue = issue;
            
        }

        private string TemplateLocation
        {
            get
            {
                return Path.Combine(Environment.CurrentDirectory, "IndesignExport", "Template", NewspaperDatabase.Current.TemplateFilename);
            }
        }

        private int PageCount
        {
            get
            {
                return _issue.Pages.Length;
            }
        }

        private string GetClassifiedText(ClassifiedAd cl)
        {
            

            var tx = cl.Text.Replace("\n", " ").Replace("\r", " ");
            while (tx.Contains("  "))
                tx = tx.Replace("  ", " ");
            tx = tx.Replace("\"", "\"\""); 
            return tx;
        }

        private bool IsBold(ClassifiedAd cl)
        {
            return cl.Options.HasFlag(OptionsFlags.IsBold);
        }

        private bool IsFilled(ClassifiedAd cl)
        {
            return cl.Options.HasFlag(OptionsFlags.IsBackground);
        }

        private bool IsBorder(ClassifiedAd cl)
        {
            return cl.Options.HasFlag(OptionsFlags.IsBorder);
        }

        private bool IsCentered(ClassifiedAd cl)
        {
            return cl.Options.HasFlag(OptionsFlags.IsCentered);
        }

        private IEnumerable<ClassifiedAd> Classified
        {
            get
            {
                return ClassifiedAd.GetAdsForIssue(_issue);
            }
        }

        private Issue Issue
        {
            get
            {
                return _issue;
            }
        }


    }
}
