namespace Danny.Test;
using Danny.Core.Common;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        Console.WriteLine(DateTime.Now.ToUnixTimestamp());
        Console.WriteLine(1696263648805.UnixTimestampToDateTime());
    }

    [Test]
    public void Test2()
    {
        //new OverallPriceReview().DoSomething();
        ;
    }
}
