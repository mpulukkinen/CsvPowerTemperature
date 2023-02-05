using CsvPowerToTemp.Caches;
using CsvPowerToTemp.DataConverters;
using CsvPowerToTemp.DataHealers;
using CsvPowerToTemp.DataVisualizers;
using CsvPowerToTemp.Interfaces;
using CsvPowerToTemp.PowerReadingProviders;
using Serilog;

namespace CsvPowerToTemp;
public class Program
{
    private static List<IPowerReadingProvider> _providers = new () { new ReadFromCsvFilesInCurrentFolder() };
    private static List<IDataConverter> _converters = new() { new SplitDataByDateOrFile(), new SumToDaily()}; // TODO: Orcer here is important, probably should do something about it
    private static List<IDataHealer> _healers = new() { new FillMissingTemperatureFromFmi() };
    private static List<IPowerReadingVisualizer> _visualizers = new() { new PrintResultsToConsole() };

    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/debug.log")
                .CreateLogger();

        AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

        Log.Information("--- Start CsvPowerToTemp ---");


        var argsAsList = args.ToList();
        if (argsAsList.Any(a => a == "-h" || a == "?" || a == "help"))
        {
            Console.WriteLine("Power usage by temperature by Matti Pulkkinen for customes of Elenia...");
            Console.WriteLine("Power measurements are average consumption per hour or day when average temperature has been X");
            var helps = _providers.OfType<IHelpProvider>()
                .Concat(_converters.OfType<IHelpProvider>())
                .Concat(_healers.OfType<IHelpProvider>())
                .Concat(_visualizers.OfType<IHelpProvider>());

            foreach (var help in helps)
            {
                help.PrintHelpToConsole();
            }

            return;
        }

        Log.Debug("Collecting data...");

        var listOfAllReadings = _providers.Select(async s => await s.GetPowerReadings(args)).Select(s => s.Result).ToList();

        listOfAllReadings = await ProcessData(args, listOfAllReadings, new FileCache());

        Log.CloseAndFlush();
        Console.WriteLine($"(Missing weather data profided my Finnish Meteorology Institute ({DateTime.Now}). This program modifies data by calculating daily average based on that data)");
        Console.WriteLine("Press any key to quit");
        Console.ReadLine();
    }

    public static async Task<List<List<PowerReading>>> ProcessData(string[] args, List<List<PowerReading>> listOfAllReadings, ITemperatureCache cache, string locationOverride = "")
    {
        Console.WriteLine("Data connected, healing...");

        foreach (var h in _healers)
        {
            foreach (var l in listOfAllReadings)
            {
                await h.HealData(l, args, cache, locationOverride);
            }
        }

        Console.WriteLine("Healing done, converting...");

        foreach (var c in _converters)
        {
            listOfAllReadings = await c.ConvertData(listOfAllReadings, args);
        }

        Console.WriteLine("Converting done, visualizing...");

        foreach (var v in _visualizers)
        {
            v.VisualizeData(listOfAllReadings, args);
        }

        return listOfAllReadings;
    }

    private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
    {
        Log.Error((Exception)e.ExceptionObject, "Unhandled exception");
    }
}
