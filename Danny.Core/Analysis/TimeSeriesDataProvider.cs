using System;
using Danny.Core.Models;
using Danny.Core.Data;
using System.Collections;
using System.Text;

namespace Danny.Core.Analysis
{

	public class TimeSeriesData
	{
		public readonly DateTime Date;
		public readonly Dictionary<string, double?> DicData;

		public TimeSeriesData(DateTime dt)
		{
			DicData = new Dictionary<string, double?>();
			Date = dt;
		}
    }

    public class SortedTimeSeriesDataList : IReadOnlyCollection<TimeSeriesData>
    {
        private IList<TimeSeriesData> _seriesDataList;

        public int Count => _seriesDataList.Count;

        public readonly PeriodType PeriodType;

        private IList<string> _loadedSymbol;

        public SortedTimeSeriesDataList(IList<TimeSeriesData> timeSeriesDatas, PeriodType periodType, IList<string> loadedTypes)
        {
            _seriesDataList = timeSeriesDatas;
            PeriodType = periodType;
            _loadedSymbol = loadedTypes;
        }

        public IEnumerator<TimeSeriesData> GetEnumerator()
        {
            return _seriesDataList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _seriesDataList.GetEnumerator();
        }

        public string ToCsvString()
        {
            RefillMissingWithPreiousValue();

            if (this.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetCsvHeaderString());

            foreach (var v in _seriesDataList)
            {
                string line = v.Date.ToString("yyMMdd");
                foreach (var sym in _loadedSymbol.OrderBy(x => x))
                {
                    line += "," + v.DicData[sym] ?? string.Empty;
                }
                sb.AppendLine(line);
            }

            return sb.ToString();
        }

        private string GetCsvHeaderString()
        {
            string ret = "Date";
            foreach (var k in _loadedSymbol.OrderBy(x => x))
            {
                ret += "," + k;
            }
            return ret;
        }

        private void RefillMissingKeyWithEmpty()
        {
            foreach (var v in _seriesDataList)
            {
                foreach (var key in _loadedSymbol)
                {
                    if (!v.DicData.ContainsKey(key))
                    {
                        v.DicData.Add(key, null);
                    }
                }
            }
        }

        private void RefillMissingWithPreiousValue()
        {
            RefillMissingKeyWithEmpty();

            int i = 0;
            TimeSeriesData prevCompleteRecord = null;
            while (i < _seriesDataList.Count)
            {
                var current = _seriesDataList[i];
                if (current.DicData.Count(x => x.Value.HasValue) != _loadedSymbol.Count)
                {
                    if (prevCompleteRecord != null)
                    {
                        foreach (var kv in current.DicData)
                        {
                            if (!kv.Value.HasValue)
                            {
                                current.DicData[kv.Key] = prevCompleteRecord.DicData[kv.Key];
                            }
                        }
                        prevCompleteRecord = current;
                    }
                }
                else
                {
                    prevCompleteRecord = current;
                }
                i++;
            }
        }
    }

    public class TimeSeriesDataProvider
	{
		private SortedList<DateTime, TimeSeriesData> _seriesDataList = new SortedList<DateTime, TimeSeriesData>();

        private List<string> _loadedDataType = new List<string>();

        private MyDbContext _myDbContext;
        private readonly PeriodType _periodType;
		
		public TimeSeriesDataProvider(MyDbContext myDbContext, PeriodType pt)
		{
			_myDbContext = myDbContext;
            _periodType = pt;
		}

        public SortedTimeSeriesDataList GetTimeSeriesData(DateTime startFrom, List<string> symbols)
        {
            foreach (var symbol in symbols)
            {
                if (!_loadedDataType.Contains(symbol))
                {
                    if (TradingEconomicsSymbol.Symbols.Contains(symbol))
                    {
                        AddTradingEcnomicsData(symbol);
                    }
                    //读取股票数据
                    else if (symbol.StartsWith(Stock.SymbolPrefix))
                    {
                        AddStockData(symbol);
                    }
                }
            }

            var value = _seriesDataList.Values.Where(x => x.Date >= startFrom).ToList();

            return new SortedTimeSeriesDataList(value, _periodType, _loadedDataType);
        }

        private void AddStockData(string symbol)
        {
            string code = symbol.Split(":").Last();
            var stock = _myDbContext.Stocks.Find(code);

            if (stock == null)
                return;

            //一个symbol带出所有 暂时这么处理
            foreach (var kline in _myDbContext.Klines.Where(x => x.StockCode == stock.Code && x.PeriodType == _periodType).ToList())
            {
                //Add close price
                AddOne(kline.Timestamp, stock.SymbolClose, kline.Close);

                double currentDividend = GetLastYearDivident(kline);
                //Add Divident
                AddOne(kline.Timestamp, stock.SymbolDivident, currentDividend);

                //Add pd
                AddOne(kline.Timestamp, stock.SymbolPd, Math.Round(currentDividend * 100 / kline.Close, 4));
            }

            _loadedDataType.Add(stock.SymbolClose);
            _loadedDataType.Add(stock.SymbolDivident);
            _loadedDataType.Add(stock.SymbolPd);
        }

        private double GetLastYearDivident(Kline kl)
        {
            var dividents = _myDbContext.Dividends.Where(x => x.StockCode == kl.StockCode)
                .OrderBy(x => x.ExcludeRightDate)
                .ToList();

            //没有分红历史记录
            if (dividents.Count == 0)
                return 0.0;

            //先查看过去365天内有没有分红
            //这里加10天 有些除权的日期可能跨国365天
            var last = dividents.LastOrDefault(x => x.ExcludeRightDate > kl.Timestamp - new TimeSpan(365 + 10, 0, 0, 0).TotalMilliseconds
            && x.ExcludeRightDate <= kl.Timestamp
            );
            if (last == null)
                return 0.0;

            //以最近的一次分红 往前找300天 这里已经考虑了四次分红的情况 3个月 6个月 1年 一般是这个频率
            //var last = dividents.First();
            var span = last.ExcludeRightDate - new TimeSpan(305, 0, 0, 0).TotalMilliseconds;

            var lastYearDividends = dividents.Where(x => x.ExcludeRightDate <= last.ExcludeRightDate
            && x.ExcludeRightDate > span);

            return lastYearDividends.Sum(x => x.Amount);
        }
        

        //增加一个值到
        private void AddOne(long timestamp, string symbol, double? value)
        {
            DateTime dt = GetDateKey(timestamp);
            if (_seriesDataList.ContainsKey(dt))
            {
                var dicd = _seriesDataList[dt].DicData;
                if (dicd.ContainsKey(symbol))
                {
                    dicd[symbol] = value;
                }
                else
                {
                    dicd.Add(symbol, value);
                }
            }
            else
            {
                var temp = new TimeSeriesData(dt);
                temp.DicData.Add(symbol, value);
                _seriesDataList.Add(dt, temp);
            }
        }

        //
        private void AddTradingEcnomicsData(string symbol)
        {
            foreach (var td in _myDbContext.TradingData.Where(x => x.PeriodType == _periodType && x.Symbol == symbol))
            {
                AddOne(td.Timestamp, td.Symbol, td.Value);
            }

            _loadedDataType.Add(symbol);
        }

        //时序数据用时间作为key 以日为单位要去掉 时间信息 以月为单位的要去掉日和时间信息
        private DateTime GetDateKey(long timestamp)
        {
            if (_periodType == PeriodType.Day)
                return timestamp.UnixTimestampToDateTime().Date;
            else
                return timestamp.UnixTimestampToDateTime().Date.FirstDayOfMonth();
        }
    }
}

