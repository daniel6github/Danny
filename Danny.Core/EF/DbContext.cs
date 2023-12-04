using Microsoft.EntityFrameworkCore;
using Danny.Core.Models;


namespace Danny.Core.Data;

public class MyDbContext : DbContext
{
    public DbSet<Stock> Stocks { get; set; }

    public DbSet<Kline> Klines { get; set; }

    public DbSet<Dividend> Dividends { get; set; }

    public DbSet<BondYield> BondYields {get; set;}

    public DbSet<TradingData> TradingData { get; set; }

    public MyDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=danny;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.Code);
        });

        modelBuilder.Entity<Kline>(entity =>
        {
            entity.HasKey(e => new { e.StockCode, e.Timestamp, e.PeriodType });
        });

        modelBuilder.Entity<Dividend>(entity =>
        {
            entity.HasKey(e => new { e.StockCode, e.PayDate });
        });

        modelBuilder.Entity<BondYield>(entity =>
        {
            entity.HasKey(e => new { e.Symbol, e.Timestamp });
        });

        modelBuilder.Entity<TradingData>(entity =>
        {
            entity.HasKey(e => new { e.Symbol, e.Timestamp, e.PeriodType });
        });

        modelBuilder.Entity<Stock>()
        .HasMany(e => e.Klines)
        .WithOne(e => e.Stock)
        .HasForeignKey(e => e.StockCode)
        .IsRequired();

        modelBuilder.Entity<Stock>()
        .HasMany(e => e.Dividends)
        .WithOne(e => e.Stock)
        .HasForeignKey(e => e.StockCode)
        .IsRequired(false);
    }
}




