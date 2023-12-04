using System;
using System.Net.Http.Headers;
using System.Text.Json;
using Danny.Core.Data;
using Danny.Core.Models;

namespace Danny.Core.DataExtracting
{
    public record TradingEconomicsToken(string usBondToken, string cnBondToken, string usdcnyCurToken);

    public class TradingEconomics
	{
		private readonly HttpClient TradingEconomicsHttpClient = new();
        //private PeriodType _periodType;
        //private string _span;
        //private string _interval;
        private TradingEconomicsToken _tokens;
        
        public TradingEconomics(TradingEconomicsToken tokens)
		{
			var productValue = new ProductInfoHeaderValue("Safari", "605.1.15");
            
            TradingEconomicsHttpClient.DefaultRequestHeaders.UserAgent.Add(productValue);
            _tokens = tokens;
		}

        //25 year 月数据
        public void GetTradingDataFromTrendingEconomics(PeriodType periodType)
        {
            var span = "25y";
            var interval = "1month";
            if (periodType == PeriodType.Day)
            {
                span = "10y";
                interval = "1day";
            }

            string url = $"https://markets.tradingeconomics.com/chart?s={TradingEconomicsSymbol.Usgg10yrSymbol}" +
                $"&interval={interval}&span={span}&securify=new" +
                $"&url=/united-states/government-bond-yield&AUTH={_tokens.usBondToken}";

            GetSingleChartData(TradingEconomicsSymbol.Usgg10yrSymbol, url, periodType);

             url = $"https://markets.tradingeconomics.com/chart?s={TradingEconomicsSymbol.Cngg10yrSymbol}" +
                $"&interval={interval}&span={span}&securify=new" +
                $"&url=/china/government-bond-yield&AUTH={_tokens.cnBondToken}";

            GetSingleChartData(TradingEconomicsSymbol.Cngg10yrSymbol, url, periodType);

            url = $"https://markets.tradingeconomics.com/chart/{TradingEconomicsSymbol.USDCNYSymbol}?" +
                $"interval={interval}&span={span}" +
                $"&securify=new&url=/china/currency" +
                $"&AUTH={_tokens.usdcnyCurToken}";

            GetSingleChartData(TradingEconomicsSymbol.USDCNYSymbol, url, periodType);
        }

        //默认取最近10年的数据
        private void GetSingleChartData(string symbol, string url, PeriodType periodType)
		{     
            string jsonString = TradingEconomicsHttpClient.HttpGetString(url);
            using JsonDocument document = JsonDocument.Parse(jsonString);
            JsonElement items = document.RootElement.GetProperty("series").EnumerateArray().First().GetProperty("data"); ;
            List<TradingData> tradingData  = new(items.GetArrayLength());
            foreach (var item in items.EnumerateArray())
            {
                var td = new TradingData();
                td.Timestamp = item.GetProperty("x").GetInt64();
                td.Symbol = symbol;
                td.Value = item.GetProperty("y").GetDouble();
                td.PeriodType = periodType;
                tradingData.Add(td);
            }

            if (tradingData.Count() > 0)
            { 
                using var dbcontext = new MyDbContext();
                if (dbcontext.TradingData.Where(x => x.Symbol == symbol && x.PeriodType == periodType).Count() == 0)
                {
                    dbcontext.TradingData.AddRange(tradingData);
                }
                else
                {
                    //update lastone
                    var lastOne = dbcontext.TradingData.Where(x => x.Symbol == symbol && x.PeriodType == periodType).ToList().MaxBy(x => x.Timestamp);
                    lastOne.Value = tradingData.First(x => x.Timestamp == lastOne.Timestamp
                    //&& x.Symbol == symbol && x.PeriodType == _periodType
                    ).Value;

                    foreach (var a in tradingData.Where(x => x.Timestamp > lastOne.Timestamp))
                    {
                        dbcontext.TradingData.Add(a);
                    }
                }
                dbcontext.SaveChanges();
            }
        }
    }
}

