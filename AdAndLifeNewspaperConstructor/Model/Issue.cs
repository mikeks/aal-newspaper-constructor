using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VitalConnection.AAL.Builder.Model
{
    public class Issue: DbObject, IDbObject
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public int Number { get; set; }

        private NewspaperPage[] _pages;

        internal bool Validate()
        {


            for (int i = 0; i < Pages.Length - 1; i++)
            {
                var p = Pages[i];
                bool f = false;
                switch (p.Grid.Id)
                {
                    case GridIdEnum.Main:
                    case GridIdEnum.Classified:
                        f = (i == 0 || i == Pages.Length - 1);
                        break;
                    case GridIdEnum.First:
                        f = (i != 0);
                        break;
                    case GridIdEnum.Last:
                        f = (i != Pages.Length - 1);
                        break;
                }

                if (f)
                {
                    MessageBox.Show($"Ошибка проверки выпуска. Тип страницы {i + 1} неверный.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            return true;
        }

        public NewspaperPage[] Pages
        {
            get
            {
                if (_pages == null) _pages = ReadPages();
                return _pages;
            }
        }

        public void ReadFromDb(SqlDataReader rdr)
        {
            Id = (int)rdr["Id"];
            Year = (int)rdr["Year"];
            Number = (short)rdr["Number"];
        }

        public override string ToString()
        {
            return $"{Year} - №{Number}";
        }

        NewspaperPage[] ReadPages()
        {
            return ReadCollectionFromDb($"select * from [Page] where [IssueId] = {Id} order by Number", () => new NewspaperPage(Id));
        }

        public static Issue[] GetAllIssues()
        {
            return ReadCollectionFromDb<Issue>("select * from [Issue] order by Year, Number");
        }

        public void CreatePage(int newPageNumber)
        {
            ExecStoredProc("CreatePage", (cmd) =>
            {
                cmd.Parameters.AddWithValue("@issueId", Id);
                cmd.Parameters.AddWithValue("@pageNumber", newPageNumber);
            });
            _pages = null;
        }

        public void DeletePage(int newPageNumber)
        {
            ExecStoredProc("DeletePage", (cmd) =>
            {
                cmd.Parameters.AddWithValue("@issueId", Id);
                cmd.Parameters.AddWithValue("@pageNumber", newPageNumber);
            });
            _pages = null;
        }

        public static void CreateIssue(int year, int number)
        {
            ExecStoredProc("CreateIssue", (cmd) =>
            {
                cmd.Parameters.AddWithValue("@year", year);
                cmd.Parameters.AddWithValue("@number", number);
                //cmd.Parameters.AddWithValue("@pageCount", pageCount);
            });
        }

    }
}
