using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalConnection.AAL.Builder.Model
{
    public class NewspaperDatabase
    {

        public static bool IsConnectionProblems { get; set; }

        public string DatabaseName { get; set; }
        public string NewspaperName { get; set; }

        public string TemplateFilename
        {
            get
            {
                return DatabaseName.ToLower() + ".indt";
            }
        }

        private static NewspaperDatabase[] _all;

        public static NewspaperDatabase[] All
        {
            get
            {
                if (_all == null)
                {
                    var dbs = new List<NewspaperDatabase>();
                    foreach (var s in Properties.Settings.Default.Databases)
                    {
                        var ss = s.Split('|');
                        dbs.Add(
                            new NewspaperDatabase()
                            {
                                NewspaperName = ss[0],
                                DatabaseName = ss[1]
                            });
                    }
                    _all = dbs.ToArray();
                }
                return _all;
            }
        }

        private static NewspaperDatabase _current;
        public static NewspaperDatabase Current
        {
            get
            {
                if (_current == null) _current = All[0];
                return _current;
            }
            set
            {
                if (_current != value)
                {
                    _current = value;
                    AdModule.ReloadAllFromDb();
                    Advertizer.ReloadAllFromDb();
                    ClassifiedAd.ReloadAllFromDb();
                    ClassifiedRubric.ReloadAllFromDb();
                    GridType.ReloadAllFromDb();
                }
            }
        }

        public override string ToString()
        {
            return NewspaperName;
        }

    }
}
