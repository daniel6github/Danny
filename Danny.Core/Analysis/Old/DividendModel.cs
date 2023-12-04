using Danny.Core.Common;
using Danny.Core.Data;
using Danny.Core.Models;
using Danny.Core.DataExtracting;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Danny.Core.Analysis
{
	//
	public class DividendModel
	{
		private readonly List<BondYield> Usgg10yrMonth;
		private readonly List<BondYield> Cngg10yrMonth;

		private readonly string OutputFolder;

		public DividendModel(string outputfolder)
		{
			Usgg10yrMonth = GetMonthBondYields(TradingEconomicsSymbol.USDCNYSymbol);
			Cngg10yrMonth = GetMonthBondYields(TradingEconomicsSymbol.Cngg10yrSymbol);
			OutputFolder = outputfolder;
		}

		private List<BondYield> GetMonthBondYields(string bondSymbol)
		{
			List<BondYield> bondYields = new();
            using MyDbContext dbContext = new MyDbContext();
			var dailyBondYields = dbContext.BondYields.Where(x => x.Symbol == bondSymbol).ToList();

			int month = 0;
			int year = 0;
			foreach (var dby in dailyBondYields)
			{
			    var dt = dby.Timestamp.UnixTimestampToDateTime();
				if (dt.Month != month || dt.Year != year)
				{
					var monthTimestamp = new DateTime(dt.Year, dt.Month, 1);
					bondYields.Add(new BondYield()
					{
						Symbol = bondSymbol,
						Timestamp = monthTimestamp.ToUnixTimestamp(),
						Yield = dby.Yield
					});
					month = dt.Month;
					year = dt.Year;
				}
				else
				{
					//取最高
					bondYields.Last().Yield = Math.Max(bondYields.Last().Yield, dby.Yield);
				}
			}

            return bondYields;
		}

		

		//改进
		public void CreateDividendReport(string code, int dividendtimesPerYears)
		{
			using MyDbContext dbContext = new MyDbContext();
			//盈富基金的分红和10年期美债收益做比较
			var stock = dbContext.Stocks.Include(x => x.Klines).
				Include(x => x.Dividends).
				Where(x => x.Code == code).First();

			StringBuilder sb = new StringBuilder();
			foreach (var kline in stock.Klines)
			{
				var span = kline.Timestamp - new TimeSpan(365, 0, 0, 0).TotalMilliseconds;
				var futureDividents = stock.Dividends.
					Where(x => x.ExcludeRightDate <= kline.Timestamp && x.ExcludeRightDate > span).TakeLast(dividendtimesPerYears).ToList();

				if (futureDividents.Count == dividendtimesPerYears)
				{
					var datetime = kline.Timestamp.UnixTimestampToDateTime();
					var total = futureDividents.Sum(x => x.Amount);
					var rate = total / kline.Low;

					var usg10yr = Usgg10yrMonth.LastOrDefault(x => x.Timestamp <= kline.Timestamp);
					var cng10yr = Cngg10yrMonth.LastOrDefault(x => x.Timestamp <= kline.Timestamp);

					if (usg10yr != null && cng10yr!=null)
					sb.AppendLine($"{datetime.ToString("yyyyMMdd")},{Math.Round(rate * 100,3)}," +
                        $"{usg10yr?.Yield},{cng10yr?.Yield},{kline.Low}");
				}
			}
			File.WriteAllText(Path.Combine(OutputFolder, $"{code}.csv"), sb.ToString());
		}
	}
}

