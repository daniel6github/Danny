using System;
using System.Text;
using Danny.Core.Data;
using Danny.Core.Models;

namespace Danny.Core.Analysis
{
	public class GenerateCsvReport
	{
		public GenerateCsvReport(string outputfolder)
		{
			OutputFolder = outputfolder;
		}

        private readonly string OutputFolder;


        public void ListStockFromPriceView()
		{
			string header = $"code,name,2020y3m,2016y2m,current";

			DateTime comparePoint202003 = new DateTime(2020, 3, 1).FirstDayOfMonth();
			DateTime comparePoint201602 = new DateTime(2016, 2, 1).FirstDayOfMonth();

			using MyDbContext myDbContext = new();

			StringBuilder sb = new StringBuilder();
			sb.AppendLine(header);

			List<Stock> stocks = myDbContext.Stocks.Where(x => x.IsValid && x.MarketCode == MarketCode.HK).Include(x => x.Klines.Where(x => x.PeriodType == PeriodType.Month)).ToList();

			foreach (var s in stocks)
			{
				//和2020年3月比下跌了多少
				var orderedMK = s.Klines.OrderBy(x => x.Timestamp);
                var kl202003 = orderedMK.FirstOrDefault(x => x.Timestamp >= comparePoint202003.ToUnixTimestamp() && x.Timestamp < comparePoint202003.AddMonths(1).ToUnixTimestamp());
                //和2016年2月比下跌了多少
                var kl201602 = orderedMK.FirstOrDefault(x => x.Timestamp >= comparePoint201602.ToUnixTimestamp() && x.Timestamp < comparePoint201602.AddMonths(1).ToUnixTimestamp());
				var current = orderedMK.LastOrDefault();

				if (kl202003 != null)
				{
					sb.AppendLine($"{s.Code},{s.Name},{kl202003.Low},{kl201602?.Low},{current.Close}");
				}
			}

            File.WriteAllText(Path.Combine(OutputFolder, $"priceview_{DateTime.Now.ToString("yyyyMMddhhmmss")}.csv"), sb.ToString());
        }

		public void Test()
		{
			using MyDbContext myDbContext = new MyDbContext();
			var provider = new TimeSeriesDataProvider(myDbContext, PeriodType.Month);

			var stock = new Stock() { Code = "02800" };
			List<string> symbols = new()
			{
				stock.SymbolClose,
				TradingEconomicsSymbol.Cngg10yrSymbol,
				TradingEconomicsSymbol.USDCNYSymbol,
				TradingEconomicsSymbol.Usgg10yrSymbol
			};
			var result = provider.GetTimeSeriesData(new DateTime(2023, 8, 1), symbols);

            File.WriteAllText(Path.Combine(OutputFolder, $"test.csv"), result.ToCsvString());
        }
    }
}

