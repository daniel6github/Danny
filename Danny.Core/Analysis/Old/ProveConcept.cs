using Danny.Core.Data;
using Danny.Core.Models;

namespace Danny.Core.Analysis;

public class ProveConcept
{
    public ProveConcept()
    {
    }

    //第一 保证跟踪重要指数的ETF之间其实是强相关的 好处是可以选一个方便建立股息率模型的
    //当前选择02800 是因为股息率历史数据更丰富 并且极值点很明确 方便做股息率跟踪模型
    //03136是02800的替换品是个新的指数 但是历史分红数据少了点
    //第二 检查所有港股通的股票平均rank的波动和选中的跟踪etf 02800品种是强相关的
    //对比过去一年的QuantileRank数据 13个月考虑了边界的情况
    public bool CanUseETFToEvaludateCandidateStocksPrice(string trackingCode = "02800")
    {
        //三个重要的指数
        List<string> etfCodes = new() { "02800", "02828", "03136" };

        double correlatedCriteria = 0.95;

        //从什么时刻开始取数据进行比较 数据肯定是越新越好 但是数据过于新会导致数据不足 统计结果仍然不好
        DateTime begin = DateTime.Now.AddMonths(-13).FirstDayOfMonth();

        using MyDbContext dbContext = new();

        Dictionary<string, List<Kline>> dicts = new();
        var allHKStocks = dbContext.Stocks.Where(x => x.MarketCode == MarketCode.HK)
            .Select(x => x.Code).ToList();

        foreach (var group in dbContext.Klines.Where(x => allHKStocks.Contains(x.StockCode))
            .ToList().Where(x => x.Timestamp.UnixTimestampToDateTime() >= begin).GroupBy(x => x.StockCode))
        {
            if (group != null)
            {
                var glist = group.ToList();
                if (glist.Count() > 0)
                    dicts.Add(group.Key, group.ToList());
            }
        }

        //etf几个指标之间是非常相关的可替代的
        foreach (var a in etfCodes)
        {
            if (a != trackingCode)
            {
                var len = Math.Min(dicts[a].Count, dicts[trackingCode].Count());
                var aLow = dicts[a].TakeLast(len).Select(x => x.Low);
                var bLow = dicts[trackingCode].TakeLast(len).Select(x => x.Low);
                var corrlationResult = Correlation.Pearson(aLow, bLow);
                if (Math.Round(corrlationResult, 2) < correlatedCriteria)
                {
                    Console.WriteLine($"{a}_{trackingCode}_{corrlationResult} 低于定义的合格相关性系数 {correlatedCriteria}");
                    return false;
                }
            }
        }

        int standardCount = dicts[trackingCode].Count;

        void AddRank(string stockCode, List<double> rankQuantile, DateTime dp)
        {
            var mkline = dicts[stockCode].Single(x => x.Timestamp >= dp.ToUnixTimestamp()
                    && x.Timestamp < dp.AddMonths(1).ToUnixTimestamp());

            var randScore = Statistics.QuantileRank(dicts[stockCode].Select(x => x.Low), mkline.Low);

            rankQuantile.Add(randScore);
        }

        List<double> allStockRankQuantile = new();
        List<double> etfRankQuantile = new();

        dicts[trackingCode].Select(x =>
            x.Timestamp.UnixTimestampToDateTime().FirstDayOfMonth())
            .ToList().ForEach(dp =>
        {
            List<double> rankQuantile = new();
            allHKStocks.Where(x => !etfCodes.Contains(x)).ToList().ForEach(cand =>
            {
                //这样可以去掉新股的干扰
                if (dicts[cand].Count == standardCount)
                {
                    AddRank(cand, rankQuantile, dp);
                }
            });
            var result = new DescriptiveStatistics(rankQuantile);
            allStockRankQuantile.Add(result.Mean);

            AddRank(trackingCode, etfRankQuantile, dp);
        });

        var correlationBetweenTrackingEtfAndAllHK = Correlation.Pearson(etfRankQuantile, allStockRankQuantile);
        if (correlationBetweenTrackingEtfAndAllHK < correlatedCriteria)
        {
            Console.WriteLine($"{correlatedCriteria} 低于定义的合格相关性系数 {correlatedCriteria}");
            return false;
        }

        Console.WriteLine($"ETF{trackingCode}相关性检测-PASS 可以用来代表当前港股的估值水平");
        return true;
    }

    //
    //证明汇率和指数的关系-出口决定 供求决定
    public void CheckWhatInflunceStockPrice(string trackingCode = "02800")
    {
        using MyDbContext dbContext = new();
        DateTime begin = DateTime.Now.AddMonths(-60).FirstDayOfMonth();

        var currency = dbContext.TradingData.Where(x => x.Symbol == TradingEconomicsSymbol.USDCNYSymbol)
            .OrderBy(x => x.Timestamp).
            ToList().Where(x => x.Timestamp.UnixTimestampToDateTime() >= begin).Select(x => x.Value).ToList();

        var chinaBondRate = dbContext.TradingData.Where(x => x.Symbol == TradingEconomicsSymbol.Cngg10yrSymbol).OrderBy(x => x.Timestamp).ToList()
            .Where(x => x.Timestamp.UnixTimestampToDateTime() >= begin).Select(x => x.Value).ToList();

        var usBondRate = dbContext.TradingData.Where(x => x.Symbol == TradingEconomicsSymbol.Usgg10yrSymbol).OrderBy(x => x.Timestamp).ToList()
            .Where(x => x.Timestamp.UnixTimestampToDateTime() >= begin).Select(x => x.Value).ToList();

        var close = dbContext.Klines.Where(x => x.StockCode == trackingCode).OrderBy(x => x.Timestamp).ToList().
            Where(x => x.Timestamp.UnixTimestampToDateTime() >= begin).Select(x => x.Close).ToList();

        //汇率影响
        Console.WriteLine(Correlation.Pearson(currency, close));
        //国债收益率
        Console.WriteLine(Correlation.Pearson(chinaBondRate, close));
        //美债收益率
        Console.WriteLine(Correlation.Pearson(usBondRate, close));
    }


    //var dividends = dbContext.Dividends.Where(x => x.StockCode == trackingCode).ToList();

    //List<double> stockDividendRates = new();
    //foreach (var kline in klines)
    //{
    //    //02800 一年分红两次
    //    var dividend = dividends.Where(x => x.ExcludeRightDate <= kline.Timestamp).TakeLast(2).Sum(x => x.Amount);
    //    var dividend_rate = dividend / kline.Low;
    //    stockDividendRates.Add(dividend_rate);
    //}
}






//证明美债收益和指数的关系-
// 1. 供求关系财政部发债和联储买 还是联储不买但美财政不发债就停摆
// 2. 美国经济好整体回报高

//证明国债收益和指数的关系
//中国经济要向好 国债收益才会提高

