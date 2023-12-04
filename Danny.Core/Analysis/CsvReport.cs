using System;
using Danny.Core.Data;
using Danny.Core.Models;

namespace Danny.Core.Analysis
{
	public class CsvReport
	{
        private string OutputFolder;

        public CsvReport(string outputfolder)
        {
            OutputFolder = outputfolder;
        }

        private List<string> GetSymbols(string[] code)
        {
            List<string> symbols = new List<string>(code.Length);
            foreach (var c in code)
            {
                var s = new Stock() { Code = c };
                symbols.Add(s.SymbolPd);
            }

            return symbols;
        }

        //比较当前股市和
        public void MacroFactors(params string[] etfcodes)
		{
			using MyDbContext myDbContext = new();

			var tsd =  new TimeSeriesDataProvider(myDbContext, PeriodType.Month);
            DateTime startOfMonth = DateTime.Now.FirstDayOfMonth().AddYears(-11);
            var symbols = GetSymbols(etfcodes);
            symbols.Add(TradingEconomicsSymbol.Cngg10yrSymbol);
            symbols.Add(TradingEconomicsSymbol.USDCNYSymbol);
            symbols.Add(TradingEconomicsSymbol.Usgg10yrSymbol);

            var result = tsd.GetTimeSeriesData(startOfMonth, symbols);
            File.WriteAllText(Path.Combine(OutputFolder, $"MacroFactors.csv"), result.ToCsvString());
        }

        //股票代码
        //产生两个report 长期和短期的pb比较
        public void StockPbFactors(params string[] codes)
        {
            using MyDbContext myDbContext = new();
            DateTime startOfMonth = DateTime.Now.FirstDayOfMonth().AddYears(-11);
            DateTime startOfDay = DateTime.Now.AddMonths(-4);

            var tsd = new TimeSeriesDataProvider(myDbContext, PeriodType.Day);
            var symbols = GetSymbols(codes);

            var result = tsd.GetTimeSeriesData(startOfDay, symbols);
            File.WriteAllText(Path.Combine(OutputFolder, $"StockPb3Months.csv"), result.ToCsvString());

            tsd = new TimeSeriesDataProvider(myDbContext, PeriodType.Month);
            result = tsd.GetTimeSeriesData(startOfMonth, symbols);
            File.WriteAllText(Path.Combine(OutputFolder, $"StockPb10Years.csv"), result.ToCsvString());
        }
    }
}

