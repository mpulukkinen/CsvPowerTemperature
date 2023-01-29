using System.Globalization;
using System.Linq;
using Csv;
using CsvPowerToTemp.DataConverters;
using CsvPowerToTemp.DataHealers;
using CsvPowerToTemp.DataVisualizers;
using CsvPowerToTemp.Interfaces;
using CsvPowerToTemp.PowerReadingProviders;

namespace CsvPowerToTemp;
class Program
{
    private static List<IPowerReadingProvider> _providers = new () { new ReadFromCsvFilesInCurrentFolder() };
    private static List<IDataConverter> _converters = new() { new SplitDataByDateOrFile(), new SumToDaily()}; // TODO: Orcer here is important, probably should do something about it
    private static List<IDataHealer> _healers = new() { new FillMissingTemperatureFromFmi() };
    private static List<IPowerReadingVisualizer> _visualizers = new() { new PrintResultsToConsole() };

    static async Task Main(string[] args)
    {
        var argsAsList = args.ToList();
        if(argsAsList.Any(a => a == "-h" || a == "?" || a == "help"))
        {
            Console.WriteLine("Power usage by temperature by Matti Pulkkinen for customes of Elenia...");
            Console.WriteLine("Power measurements are average consumption per hour or day when average temperature has been X");
            var helps = _providers.OfType<IHelpProvider>()
                .Concat(_converters.OfType<IHelpProvider>())
                .Concat(_healers.OfType<IHelpProvider>())
                .Concat(_visualizers.OfType<IHelpProvider>());

            foreach (var help  in helps) {
                help.PrintHelpToConsole();
            }            

            return;
        }

        Console.WriteLine("Collecting data...");

        var listOfAllReadings = _providers.Select(async s => await s.GetPowerReadings(args)).Select(s => s.Result).ToList();

        Console.WriteLine("Data connected, healing...");

        foreach (var h in _healers)
        {
            foreach(var l in listOfAllReadings)
            {
                await h.HealData(l, args);
            }            
        }

        Console.WriteLine("Healing done, converting...");

        var splittedList = new List<List<PowerReading>>();

        foreach (var c in _converters)
        {
            listOfAllReadings = await c.ConvertData(listOfAllReadings, args);
        }

        Console.WriteLine("Converting done, visualizing...");

        foreach (var v in _visualizers)
        {
            v.VisualizeData(listOfAllReadings, args);
        }        
    }
}
