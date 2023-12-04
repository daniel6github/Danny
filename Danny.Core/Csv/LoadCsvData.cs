using System.Globalization;
using CsvHelper;
using Danny.Core.Data;
using Danny.Core.Models;
using Microsoft.EntityFrameworkCore;
using Danny.Core.Common;
using CsvHelper.Configuration;

namespace Danny.Core.Csv
{
    public class LoadCsvData
    {
        public static void UpdateETFDividends(string folder)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };
            foreach (string filename in Directory.GetFiles(folder, "*.csv"))
            {
                string code = new FileInfo(filename).Name.Replace(".csv", string.Empty);
                var eTFDividendCsvs = CsvMethod.ReadCsv<ETFDividendCsv>(filename, config);
                List<Dividend> dividends = new List<Dividend>(eTFDividendCsvs.Count());
                foreach (var c in eTFDividendCsvs)
                {
                    dividends.Add(new Dividend()
                    {
                        CurrencyCode = CurrencyCode.HKD,
                        StockCode = code,
                        Amount = c.Amount,
                        ExcludeRightDate = GetDatetimeFromHsiFormatDateString(c.ExcludeRightDate).ToUnixTimestamp(),
                        PayDate = GetDatetimeFromHsiFormatDateString(c.PayDate).ToUnixTimestamp()
                    });
                }

                using MyDbContext myDbContext = new MyDbContext();
                if (dividends.Count() > 0)
                {
                    var queryResult= myDbContext.Dividends.Where(x => x.StockCode == code);
                    myDbContext.Dividends.RemoveRange(queryResult);
                    myDbContext.Dividends.AddRange(dividends);
                    myDbContext.SaveChanges();
                }
            }
        }

        private static DateTime GetDatetimeFromHsiFormatDateString(string s)
        {
            string dateString = s.Replace("年", string.Empty).
                Replace("月", string.Empty).Replace("日", string.Empty);

            return DateTime.Parse(s);
        }

        public static void LoadStockListFromCsvToDb(string folder)
        {
            List<CsvStockId> csvStocks = new();

            foreach (string filename in Directory.GetFiles(folder, "*.csv"))
            {
                if (filename.Contains("HK.csv"))
                {
                    var stocks = ReadSingleFile<HKStockCSV>(filename, MarketCode.HK);
                    //暂未纳入港股通 但是值得跟踪的一个指数etf 用于整体估值位置的判断
                    stocks.Add(new HKStockCSV()
                    {
                        Code = "03136",
                        MarketCode = MarketCode.HK,
                        Name = "恒指ESGETF"
                    });

                    csvStocks.AddRange(stocks);
                }
                else if (filename.Contains("SH68.csv") || filename.Contains("SH60.csv"))
                {
                    var mc = filename.Contains("SH68.csv") ? MarketCode.SH68 : MarketCode.SH60;
                    var stocks = ReadSingleFile<SHStockCSV>(filename, mc);
                    csvStocks.AddRange(stocks);
                }
                else if (filename.Contains("SZ00.csv") || filename.Contains("SZ30.csv"))
                {
                    var mc = filename.Contains("SZ00.csv") ? MarketCode.SZ00 : MarketCode.SZ30;
                    var stocks = ReadSingleFile<SZStockCSV>(filename, mc);
                    csvStocks.AddRange(stocks);
                }
            }

            using (var dbcontext = new MyDbContext())
            {
                //将不再港股通 和退市的股票设置成is_valid为false
                var queryResult = dbcontext.Stocks.ToList().Where(s => !csvStocks.Exists(s1 => s1.Code == s.Code)).ToList();
                queryResult?.ForEach(x => x.IsValid = false);
                dbcontext.SaveChanges();

                csvStocks.ForEach(x =>
                {
                    var stock = dbcontext.Stocks.Find(x.Code);
                    //不存在 添加新的
                    if (stock == null)
                    {
                        dbcontext.Stocks.Add(new Stock() { Code = x.Code, IsValid = true, MarketCode = x.MarketCode, Name = x.Name });
                    }
                    //存在更新名字 又变成ST的可能
                    else
                    {
                        stock.Name = x.Name;
                        stock.MarketCode = x.MarketCode;
                    }
                });
                dbcontext.SaveChanges();
            }
        }

        private static List<T> ReadSingleFile<T>(string filename, MarketCode mc) where T : CsvStockId, new()
        {
            var ret = new List<T>();
            using var reader = new StreamReader(filename);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            foreach (var s in csvReader.GetRecords<T>())
            {
                ret.Add(new T() { Code = s.Code, Name = s.Name, MarketCode = mc });
            }
            return ret;
        }
    }
}


