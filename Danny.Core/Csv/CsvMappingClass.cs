using System;
using Danny.Core.Models;
using CsvHelper.Configuration.Attributes;

namespace Danny.Core.Csv
{
    public interface CsvStockId
    {
        string Name { get; set; }
        string Code { get; set; }

        MarketCode MarketCode { get; set; }
    }

    public class HKStockCSV : CsvStockId
    {
        [CsvHelper.Configuration.Attributes.Index(0)]
        public string Code { get; set; }

        [CsvHelper.Configuration.Attributes.Index(1)]
        public string Name { get; set; }

        [Ignore]
        public MarketCode MarketCode { get; set; }
    }

    public class SHStockCSV : CsvStockId
    {
        [CsvHelper.Configuration.Attributes.Index(0)]
        public string Code { get; set; }

        [CsvHelper.Configuration.Attributes.Index(3)]
        public string Name { get; set; }

        [Ignore]
        public MarketCode MarketCode { get; set; }
    }

    public class SZStockCSV : CsvStockId
    {
        [CsvHelper.Configuration.Attributes.Index(4)]
        public string Code { get; set; }

        [CsvHelper.Configuration.Attributes.Index(5)]
        public string Name { get; set; }

        [Ignore]
        public MarketCode MarketCode { get; set; }
    }

    //https://www.trahk.com.hk/zh-cn/trahk-fund/fund-information/?FundClass=NA&FundUnit=NA
    //盈富基金官网直接从网页copy
    public class ETFDividendCsv
    {
        [CsvHelper.Configuration.Attributes.Index(0)]
        public string ExcludeRightDate { get; set; }

        [CsvHelper.Configuration.Attributes.Index(2)]
        public string PayDate { get; set; }

        [CsvHelper.Configuration.Attributes.Index(3)]
        public double Amount { get; set; }
    }
}

