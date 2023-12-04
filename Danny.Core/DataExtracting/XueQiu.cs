using Danny.Core.Models;
using System.Text.Json;
using Danny.Core.Common;
using Danny.Core.Data;
using System.Text.RegularExpressions;

namespace Danny.Core.DataExtracting;

public class XueQiu
{
    private readonly HttpClient XueQiuHttpClient = new();
    //只取2013年开始的分红
    private readonly  long dividentAfter = new DateTime(2013, 1, 1).ToUnixTimestamp();

    public XueQiu(string token)
	{
        XueQiuHttpClient.DefaultRequestHeaders.Remove("Cookie");
        XueQiuHttpClient.DefaultRequestHeaders.Add("Cookie", $"xq_a_token={token}");
    }

    private string AppendPrefixForXueqiuCode(Stock s)
    {
        if (s.MarketCode == MarketCode.SZ00 || s.MarketCode == MarketCode.SZ30)
            return "SZ" + s.Code;
        else if (s.MarketCode == MarketCode.SH68 || s.MarketCode == MarketCode.SH60)
            return "SH" + s.Code;
        else
            return s.Code;
    }

    public void DownloadHK_KLine()
    {
        DownloadAllPeriodType(DownloadKline, PeriodType.Month, MarketCode.HK);
        DownloadAllPeriodType(DownloadKline, PeriodType.Day, MarketCode.HK);
    }

    public void DownloadHK_Dividends()
    {
        DownloadAll(DownloadDividend, MarketCode.HK);
    }

    private void DownloadAll(Action<Stock> action, MarketCode mc)
    {
        using var dbcontext = new MyDbContext();
        var stocks = dbcontext.Stocks.Where(x => x.IsValid && x.MarketCode == mc).ToList();
        foreach (var stock in stocks)
            action(stock);
    }

    private void DownloadAllPeriodType(Action<Stock, PeriodType> action, PeriodType periodType, MarketCode market)
    {
        using var dbcontext = new MyDbContext();
        var stocks = dbcontext.Stocks.Where(x => x.IsValid && x.MarketCode == market).ToList();
        foreach (var stock in stocks)
            action(stock, periodType);
    }

    //下载MKline数据并存到数据库
    public void DownloadKline(Stock stock, PeriodType periodType)
	{
        var period = "month";
        
        if (periodType == PeriodType.Day)
        {
            period = "day";
        }

        string codeInUrl = $"https://stock.xueqiu.com/v5/stock/chart/kline.json?symbol={AppendPrefixForXueqiuCode(stock)}&begin={DateTime.Now.ToUnixTimestamp()}&period={period}&type=before&count=-300&indicator=kline,pe,pb,ps,pcf,market_capital,agt,ggt,balance";
        var jsonString = XueQiuHttpClient.HttpGetString(codeInUrl);
        using JsonDocument document = JsonDocument.Parse(jsonString);
        JsonElement root = document.RootElement;

        var error_code = root.GetProperty("error_code").GetUInt16();
        if (error_code != 0)
            throw new Exception($"Json History Data Has Error {stock.Code} {error_code}");
        else
        {
            JsonElement items = root.GetProperty("data").GetProperty("item");
            List<Kline> klines = new(items.GetArrayLength());
            foreach (var item in items.EnumerateArray())
            {
                var i = -1;
                var kline = new Kline();
                kline.PeriodType = periodType;
                var itemArray = item.EnumerateArray();
                kline.StockCode = stock.Code;
                kline.Timestamp = itemArray.ElementAt(++i).GetInt64();
                kline.Volumn = itemArray.ElementAt(++i).GetInt64();
                kline.Open = itemArray.ElementAt(++i).GetDouble();
                kline.High = itemArray.ElementAt(++i).GetDouble();
                kline.Low = itemArray.ElementAt(++i).GetDouble();
                kline.Close = itemArray.ElementAt(++i).GetDouble();
                kline.Chg = itemArray.ElementAt(++i).TryGetDoubleEx();
                kline.Percent = itemArray.ElementAt(++i).TryGetDoubleEx();
                kline.Turnoverrate = itemArray.ElementAt(++i).TryGetDoubleEx();
                kline.Amount = itemArray.ElementAt(++i).GetDouble();
                klines.Add(kline);
            }
            using var dbcontext = new MyDbContext();
            var old = dbcontext.Klines.Where(x => x.StockCode == stock.Code && x.PeriodType == periodType);
            if (old !=null)
                dbcontext.Klines.RemoveRange(old);
            dbcontext.Klines.AddRange(klines);
            dbcontext.SaveChanges();
        }
    }

    public void DownloadDividend(Stock stock)
    {
        var placeCode = stock.MarketCode == MarketCode.HK ? "hk" : "cn";

        string codeInUrl = $"https://stock.xueqiu.com/v5/stock/f10/{placeCode}/bonus.json?symbol={AppendPrefixForXueqiuCode(stock)}&size=40&extend=true";
        var jsonString = XueQiuHttpClient.HttpGetString(codeInUrl);
        using JsonDocument document = JsonDocument.Parse(jsonString);
        JsonElement root = document.RootElement;

        var error_code = root.GetProperty("error_code").GetUInt16();
        if (error_code != 0)
            throw new Exception($"Json History Data Has Error {stock.Code} {error_code}");
        else
        {
            JsonElement items = root.GetProperty("data").GetProperty("items");
            List<Dividend> dividends = new(items.GetArrayLength());

            foreach (var item in items.EnumerateArray())
            {
                if (GetDividendAmountFromJsonItem(item, stock, out Dividend ret))
                {
                    var existingOne = dividends.FirstOrDefault(x => x.PayDate == ret.PayDate
                    //这里简化操作了 如果同日的分红 currency不同 我们认为是出错了 还是把他们当一样的累加
                    //沪杭甬高速 例子
                    //&& x.CurrencyCode == ret.CurrencyCode
                    );
                    if (existingOne == null)
                        dividends.Add(ret);
                    else
                        existingOne.Amount += ret.Amount;
                }
                // 暂时可以发现是倒序排列的
                else
                    break;
            }

            if (dividends.Count != 0)
            {
                using var dbcontext = new MyDbContext();
                var old = dbcontext.Dividends.Where(x => x.StockCode == stock.Code);
                if (old != null)
                    dbcontext.Dividends.RemoveRange(old);
                dbcontext.Dividends.AddRange(dividends);
                dbcontext.SaveChanges();
            }
        }
    }

    private bool GetDividendAmountFromJsonItem(JsonElement item, Stock stock, out Dividend ret)
    {
        ret = new Dividend();
        ret.StockCode = stock.Code;
        if (stock.MarketCode == MarketCode.HK)
        {
            if (item.GetProperty("datedivpy").ValueKind == JsonValueKind.Null)
                return false;
            ret.PayDate = item.GetProperty("datedivpy").GetInt64();
            if (ret.PayDate < dividentAfter)
                return false;
            var raw = item.GetProperty("divdstep").GetString();
            ret.Amount = GetDividendAmountByRegex(raw, "\\d+[.|\\d]*");
            if (item.GetProperty("dertsdiv").ValueKind == JsonValueKind.Null)
                return false;
            ret.ExcludeRightDate = item.GetProperty("dertsdiv").GetInt64();
            //如果不能匹配到currency说明是很早的数据 忽略
            if (GetDividentCurrency(raw, "RMB|USD|HKD|CNY", out var cur))
                ret.CurrencyCode = cur;
            else
                return false;
        }
        else
        {
            //A股除权日和分红日是一天
            //dividend_date
            if (item.GetProperty("dividend_date").ValueKind == JsonValueKind.Null)
                return false;
            ret.PayDate = item.GetProperty("dividend_date").GetInt64();
            if (ret.PayDate < dividentAfter)
                return false;
            var raw = item.GetProperty("plan_explain").GetString();
            ret.Amount = GetDividendAmountByRegex(raw, "(?<=派)\\d+[.|\\d]*(?=元)");
            ret.ExcludeRightDate = item.GetProperty("dividend_date").GetInt64();
            ret.CurrencyCode = CurrencyCode.CNY;
        }
        return true;
    }

    //HK
    //\d+[.|\d]*
    //RMB|USD|HKD|CNY
    //中期派息每股股息0.21元（RMB ）
    //末期派息每股股息0.04243元（RMB ）,每股特别股息0.02358元（RMB ）
    //"每股股息",
    //"每股特别股息",
    //特别报告每股股息0.11元（HKD ）,每股特别股息0.3元（HKD ）
    //特别报告每股特别股息0.96元（HKD ）

    //A股
    //(?<=派)\d+[.|\d]*(?=元)
    //"10派2.1元"，
    //"10转10派2.1元"
    private bool GetDividentCurrency(string raw, string pattern, out CurrencyCode currencyCode)
    {
        var match = Regex.Match(raw, pattern);
        var val = match.Value;
        if (match.Value == "RMB")
            val = "CNY";
        return Enum.TryParse<CurrencyCode>(val, out currencyCode);
    }

    private double GetDividendAmountByRegex(string raw, string regexPatten)
    {
        var matchs = Regex.Matches(raw, regexPatten);
        var ret = 0.0;

        foreach (Match match in matchs)
        {
            ret += Convert.ToDouble(match.Value);
        }
        return ret;
    }
}

