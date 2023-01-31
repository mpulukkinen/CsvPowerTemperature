using CsvPowerToTemp.PowerReadingProviders;
using NUnit.Framework;
using Serilog;
using System.Globalization;

namespace UnitTests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/debug.log")
                .CreateLogger();
    }

    [Test]
    public async Task ParseExampleFileWithAllCultures()
    {
        CultureInfo[] cinfo = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures);


        foreach (CultureInfo cul in cinfo)
        {
            Thread.CurrentThread.CurrentUICulture = cul;
            Thread.CurrentThread.CurrentCulture = cul;
            CultureInfo.DefaultThreadCurrentCulture = cul;
            var csvParser = new ReadFromCsvFilesInCurrentFolder();
            var list = await csvParser.GetPowerReadings(Array.Empty<string>());
            Assert.AreEqual(48, list.Count);
        }
    }
}