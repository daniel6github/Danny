using System.ComponentModel.DataAnnotations.Schema;

namespace Danny.Core.Models;

public class Stock
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public MarketCode MarketCode { get; set; }
    public bool IsValid { get; set; }
    public List<Kline> Klines { get; } = new();
    public List<Dividend> Dividends { get; } = new();

    [NotMapped]
    public string SymbolClose => $"{CloseSymbolPrefix}:{Code}";

    [NotMapped]
    public string SymbolDivident => $"{DividentSymbolPrefix}:{Code}";

    [NotMapped]
    public string SymbolPd => $"{PdSymbolPrefix}:{Code}";

    public const string SymbolPrefix = "Stock";

    public const string CloseSymbolPrefix = $"{SymbolPrefix}:Close";

    public const string DividentSymbolPrefix = $"{SymbolPrefix}:Divident";

    public const string PdSymbolPrefix = $"{SymbolPrefix}:pd";

}

public enum MarketCode
{
    HK, SH60, SH68, SZ00, SZ30
}

public enum CurrencyCode
{
    CNY, HKD, USD
}

public enum PeriodType
{
    Month, Day
}

public class TradingEconomicsSymbol
{
    public const string Usgg10yrSymbol = "usgg10yr:ind";
    public const string Cngg10yrSymbol = "gcny10yr:gov";
    public const string USDCNYSymbol = "usdcny:cur";

    public static readonly string[] Symbols = new string[] { Usgg10yrSymbol, Cngg10yrSymbol, USDCNYSymbol };
}

public class Kline
{
    public string StockCode { get; set; } = null!;

    public Stock Stock { get; set; } = null!;
    //0
    public long Timestamp { get; set; }
    //1
    public long Volumn { get; set; }
    //2
    public double Open { get; set; }
    //3
    public double High { get; set; }
    //4
    public double Low { get; set; }
    //5
    public double Close { get; set; }
    //6 价格变化
    public double Chg { get; set; }
    //7
    public double Percent { get; set; }
    //8
    //换手率
    public double Turnoverrate { get; set; }
    //9 成交额
    public double Amount { get; set; }

    public PeriodType PeriodType { get; set; }

}

public class Dividend
{
    public string StockCode { get; set; } = null!;

    public Stock Stock { get; set; } = null!;

    public long PayDate { get; set; }

    public long ExcludeRightDate {get; set;}

    public CurrencyCode CurrencyCode { get; set; }

    public double Amount { get; set; }
}

public class BondYield
{
    public string Symbol { get; set; }
    public double Yield { get; set; }
    public long Timestamp { get; set; }
}

public class TradingData
{
    public string Symbol { get; set; }
    public double Value { get; set; }
    public long Timestamp { get; set; }
    public PeriodType PeriodType { get; set; }   
}