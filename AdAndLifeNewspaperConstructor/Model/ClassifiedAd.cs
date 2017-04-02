using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalConnection.AAL.Builder.Model
{

	public class IssueNumber
	{
		public int Year { get; set; }
		public int Number { get; set; }

		public IssueNumber(int year, int number)
		{
			Year = year;
			Number = number;
		}

		public override string ToString()
		{
			return $"{Year}.{Number}";
		}
	}

	public class ClassifiedAd : DbObject, IDbObject
	{
		[Flags]
		public enum OptionsFlags
		{
			IsBorder = 1,
			IsBold = 2,
			IsBackground = 4,
			IsCentered = 8
		}

		public enum PaymentMethodEnum
		{
			[Description("Не задан")]
			NotSet = -1,
			[Description("Наличные")]
			Cash = 0,
			[Description("Кредитная карта")]
			CreditCard = 1
		}

		public int Id { get; private set; }
		public string Advertiser { get; set; }
		public ClassifiedRubric Rubric { get; set; }
		public string Text { get; set; }
		public OptionsFlags Options { get; set; }
		public string Info { get; set; }
		public bool IsPaid { get; set; }
		public PaymentMethodEnum PaymentMethod { get; set; }
		public decimal Price { get; set; }
		public IssueNumber StartIssue { get; set; }
		public IssueNumber EndIssue { get; set; }
		public string SM { get; set; }
		public DateTime Created { get; private set; }

		public ClassifiedAd()
		{
			StartIssue = new IssueNumber(DateTime.Now.Year, 1);
			EndIssue = new IssueNumber(DateTime.Now.Year, 52);
		}

		public bool IsInNumber(int year, int number)
		{
			if (year < StartIssue.Year || year > EndIssue.Year) return false;
			if (year == StartIssue.Year)
			{
				if (number < StartIssue.Number) return false;
			}
			if (year == EndIssue.Year)
			{
				if (number > EndIssue.Number) return false;
			}
			return true;
		}

		public bool IsEndsInNumber(int year, int number)
		{
			return year == EndIssue.Year && number == EndIssue.Number;
		}


		public void ReadFromDb(SqlDataReader rdr)
		{
			Id = (int)rdr["Id"];
			Advertiser = (string)rdr["Advertizer"];
            var rubId = (int)rdr["RubricId"];
			Rubric = ClassifiedRubric.GetById(rubId);
			Text = (string)rdr["AdText"];

			Options = 0;
			if ((bool)rdr["IsBorder"]) Options |= OptionsFlags.IsBorder;
			if ((bool)rdr["IsBold"]) Options |= OptionsFlags.IsBold;
			if ((bool)rdr["IsBackground"]) Options |= OptionsFlags.IsBackground;
			if ((bool)rdr["IsCentered"]) Options |= OptionsFlags.IsCentered;

			Info = (string)ResolveDbNull(rdr["Info"]);
			IsPaid = (bool)rdr["IsPaid"];
			PaymentMethod = rdr["PaymentMethod"] is DBNull ? PaymentMethodEnum.NotSet : (PaymentMethodEnum)(byte)rdr["PaymentMethod"];
			Price = (decimal)ResolveDbNull(rdr["Price"], 0m);

			StartIssue = new IssueNumber((int)rdr["StartIssueYear"], (short)rdr["StartIssueNumber"]);
			EndIssue = new IssueNumber((int)rdr["EndIssueYear"], (short)rdr["EndIssueNumber"]);

			SM = (string)ResolveDbNull(rdr["clasSM"]);
			Created = (DateTime)rdr["clasTimestamp"];
		}

        public void Revert()
        {
            ReadSql($"select * from [ClassifiedAd] where Id = {Id}", ReadFromDb);
        }

		public void Delete()
		{
			ExecSQL($"delete from [ClassifiedAd] where Id = {Id}");
		}

		public void Save()
		{
            ExecStoredProc("SaveClassifiedAd", (cmd) =>
            {
                if (Id > 0) cmd.Parameters.AddWithValue("@id", Id);
                cmd.Parameters.AddWithValue("@advertiser", Advertiser);
                cmd.Parameters.AddWithValue("@rubricId", Rubric.Id);
                cmd.Parameters.AddWithValue("@text", Text);
                cmd.Parameters.AddWithValue("@isBorder", Options.HasFlag(OptionsFlags.IsBorder));
                cmd.Parameters.AddWithValue("@isBold", Options.HasFlag(OptionsFlags.IsBold));
                cmd.Parameters.AddWithValue("@isBackground", Options.HasFlag(OptionsFlags.IsBackground));
                cmd.Parameters.AddWithValue("@isCentered", Options.HasFlag(OptionsFlags.IsCentered));
                cmd.Parameters.AddWithValue("@info", Info);
                cmd.Parameters.AddWithValue("@isPaid", IsPaid);
                if (PaymentMethod != PaymentMethodEnum.NotSet) cmd.Parameters.AddWithValue("@paymentMethod", PaymentMethod);
                cmd.Parameters.AddWithValue("@price", Price);
                cmd.Parameters.AddWithValue("@startIssueNumber", StartIssue.Number);
                cmd.Parameters.AddWithValue("@startIssueYear", StartIssue.Year);
                cmd.Parameters.AddWithValue("@endIssueNumber", EndIssue.Number);
                cmd.Parameters.AddWithValue("@endIssueYear", EndIssue.Year);
                cmd.Parameters.AddWithValue("@sm", SM);
            });
		}

        public static void ReloadAllFromDb()
        {
            _all = null;
        }


        private static ClassifiedAd[] _all;

		public static ClassifiedAd[] All
		{
			get
			{
				if (_all == null)
				{
					_all = ReadCollectionFromDb<ClassifiedAd>("select * from [ClassifiedAd]");
				}
				return _all;
			}
		}

		public static IEnumerable<ClassifiedAd> GetAdsForIssue(Issue issue)
		{
			return All.Where((x) => x.IsInNumber(issue.Year, issue.Number)).OrderBy((x) => x.Rubric.SortOrder);
		}

	}
}
