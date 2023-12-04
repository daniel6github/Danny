using System.Windows.Input;
using CommandLine;
using CsvHelper;
using Danny.Core.Csv;
using Danny.Core.DataExtracting;
using Danny.Core.Analysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.IO;

namespace Danny.Console;

public interface ICommand
{
    void Execute(ISettings settings);
}

[Verb("SyncData", HelpText = "test")]
public class GetDataCmd : ICommand
{
    public void Execute(ISettings settings)
    {
        settings.TryGetValue("directory:stockList", out string stockfolder);
        settings.TryGetValue("directory:etfDividend", out string etfdividentfolder);

        LoadCsvData.LoadStockListFromCsvToDb(Path.Combine(settings.WorkingDirectory,
            stockfolder));
        LoadCsvData.UpdateETFDividends(Path.Combine(settings.WorkingDirectory,
            etfdividentfolder));

        settings.TryGetValue("tradingEconomicsToken:usBond", out string usToken);
        settings.TryGetValue("tradingEconomicsToken:cnBond", out string cnToken);

        settings.TryGetValue("tradingEconomicsToken:usdCny", out string usdcnyCurToken);
        settings.TryGetValue("xueQiuToken", out string xueQiuToken);

        var te = new TradingEconomics(new TradingEconomicsToken(usToken, cnToken
            , usdcnyCurToken));
        te.GetTradingDataFromTrendingEconomics(Core.Models.PeriodType.Day);
        te.GetTradingDataFromTrendingEconomics(Core.Models.PeriodType.Month);

        var xq = new XueQiu(xueQiuToken);
        xq.DownloadHK_KLine();
        xq.DownloadHK_Dividends();
    }
}
        
[Verb("CsvReport", HelpText = "test")]
public class CsvReportCmd : ICommand
{
    public void Execute(ISettings settings)
    {
        settings.TryGetValue("directory:output", out string output);

        var report = new CsvReport(Path.Combine(settings.WorkingDirectory, output));
        settings.TryGetValue("etfCodes", out string etfcodes);
        report.MacroFactors(etfcodes.Split(","));
        settings.TryGetValue("stockCodes", out string stockcodes);

        report.StockPbFactors(stockcodes.Split(","));
    }
}

public interface ISettings
{
    //tradingEconomicsToken:cnBond
    bool TryGetValue(string path, out string value);
    string WorkingDirectory { get; }

}

public class Settings1 : ISettings
{
    private readonly JsonDocument _document;
            
    public Settings1()
    {
        string json = File.ReadAllText("Settings.json");   
        _document = JsonDocument.Parse(json);
    }

    public string WorkingDirectory
    {
        get
        {
            TryGetValue("directory:root", out string value);
            return value;
        }
    }

    public bool TryGetValue(string path, out string value)
    {
        string[] paths = path.Split(":");
        JsonElement je = _document.RootElement;
        value = string.Empty;
        foreach (var p in paths)
        {
            if (!je.TryGetProperty(p, out je))
                return false;
        }

        value = je.GetString() ?? string.Empty;
        return value != string.Empty;
    }
}

public interface IApp
{
    void Run(string[] args);
}

public class App : IApp
{
    private readonly ISettings _settings;
    public App(ISettings settings)
    {
        _settings = settings;
    }

    public void Run(string[] args)
    {
        Parser.Default.ParseArguments<GetDataCmd, CsvReportCmd>(args).WithParsed(x => (x as ICommand)?.Execute(_settings));
    }
}

public class Program
{
    static void Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISettings, Settings1>();
        services.AddTransient<IApp, App>();
        var sp = services.BuildServiceProvider();
        sp.GetService<IApp>()?.Run(args);
    }
}
