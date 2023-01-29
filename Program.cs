using System.Globalization;
using Csv;
namespace CsvPowerToTemp;
class Program
{
    private static WeatherService _weather = new WeatherService();
    static async Task Main(string[] args)
    {
        var argsAsList = args.ToList();
        if(argsAsList.Any(a => a == "-h" || a == "?" || a == "help"))
        {
            Console.WriteLine("Ppwer usage by temperature by Matti Pulkkinen for customes of Elenia...");
            Console.WriteLine("-c Clear temperature cache");
            Console.WriteLine("-d Split point for data for comparison (d.M.yyyy format, like 3.12.2022 for 3rd of December 2022). If not defined, split by csv files");
            Console.WriteLine("Copy our csv files to this folder, do not modify them, especially not the name");
            Console.WriteLine("Power measurements are average consumption per hour when average temperature has been X");
            return;
        }

        var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csv");

        var listOfAllReadings = new List<List<PowerReading>>();
        CultureInfo provider = CultureInfo.InvariantCulture;

        var splitByDate = argsAsList.Count > 1 && argsAsList.Any(a => a == "-d");

        var dateSplit = DateTime.Now;

        if(splitByDate) {
            dateSplit = DateTime.ParseExact(argsAsList[argsAsList.IndexOf("-d") + 1], "d.M.yyyy", provider);
        }

        var cacheLocation = "./tempCache.txt";

        if(argsAsList.Any(a => a == "-c") && File.Exists(cacheLocation))
        {
            File.Delete(cacheLocation);
        }

        var dateSplitted = false;

        var currentList = new List<PowerReading>();

        var tempDict = new Dictionary<string, int>();

        var location = "Lempäälä";

        if(argsAsList.IndexOf("-l") > 0)
        {
            location = argsAsList[argsAsList.IndexOf("-l") + 1];
        }

        if(File.Exists(cacheLocation))
        {
            var lines = (await File.ReadAllTextAsync(cacheLocation)).Split("\n").Where(s => !string.IsNullOrEmpty(s)).ToList();

            lines.ForEach(l => {
                var lineSplit = l.Split(":");
                tempDict[lineSplit[0]] = int.Parse(lineSplit[1]);
            });
        }

        foreach (var file in files)
        {
            Console.WriteLine($"Processing file: {file}, split by date: {splitByDate}, date to split: {dateSplit}");

            var year = int.Parse(string.Join("", file.Split("_")[1].Take(4)));

            var previousTemp = 0;

            var tempDictTimeFormat = "dd.MM.yyyy";

            foreach (var line in CsvReader.ReadFromText(File.ReadAllText(file)))
            {
                // Header is handled, each line will contain the actual row data
                var time = line[0];
                var power = line[1];

                power = power.Replace(",", ".");

                if(string.IsNullOrEmpty(power)) {
                    // Elenia makes empty values for if you don't take whole year
                    break;
                }

                var temp = line[2];

                temp = temp.Replace(",", ".");

                try {
                    var actualDate = DateTime.ParseExact(time, "d.M. mm:hh:ss", provider);
                    actualDate = new DateTime(year, actualDate.Month, actualDate.Day, actualDate.Hour, actualDate.Minute, actualDate.Second);

                    //Console.WriteLine($"Time: {time}, actualDate: {actualDate}, power: {power}, temp: {temp}");

                    if(actualDate.Hour == 0 && actualDate.Minute == 0) {
                        if(string.IsNullOrEmpty(temp))
                        {
                            if(tempDict.ContainsKey(actualDate.ToString(tempDictTimeFormat)))
                            {
                                previousTemp = tempDict[actualDate.ToString(tempDictTimeFormat)];
                            } else {
                                Console.WriteLine($"Date was empty but expected, try to find from open sources: {actualDate}");

                                var tempFromExternalSource = await _weather.GetAvgTemperatureForDate(actualDate);

                                if(tempFromExternalSource != int.MaxValue)
                                {
                                    Console.WriteLine($"Could not get tempareture for {actualDate} in {location}");
                                    previousTemp = await _weather.GetAvgTemperatureForDate(actualDate);
                                    tempDict[actualDate.ToString(tempDictTimeFormat)] = previousTemp;
                                }
                            }
                        } else
                        {
                            previousTemp = (int)Math.Round(float.Parse(temp));
                        }
                    }

                    var reading = new PowerReading {Power = float.Parse(power), Temp = previousTemp, Time = actualDate};

                    if(splitByDate && actualDate >= dateSplit && !dateSplitted) {
                        Console.WriteLine($"DO SPLIT: {dateSplit}");
                        listOfAllReadings.Add(currentList);
                        currentList = new List<PowerReading>();
                        dateSplitted = true;
                    }

                    currentList.Add(reading);
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
            }

            if(!splitByDate) {
                listOfAllReadings.Add(currentList);
                currentList = new List<PowerReading>();
            }
        }

        var tempCacheOutput = "";
        foreach(var kvp in tempDict)
        {
            tempCacheOutput += kvp.Key + ":" + kvp.Value.ToString() + "\n";
        }

        if(File.Exists(cacheLocation))
        {
            File.Delete(cacheLocation);
        }

        File.WriteAllText(cacheLocation, tempCacheOutput);
        listOfAllReadings.Add(currentList);

        var dict = new Dictionary<int, int>();

        var index = 1;
        var showOlyMatching = true;

        if(argsAsList.Contains("-cl"))
        {
            Console.WriteLine("Clip daily results by removing 12% of the highest values (like warming up sauna etc)");

            for(int i = 0; i< listOfAllReadings.Count; i++) {
                listOfAllReadings[i] = TemperatureModifiers.ClipHighest10PercentOfHourlyConsumption(listOfAllReadings[i]);
            }
        }

        var samplesAsDays = false;
        if(argsAsList.Contains("-s"))
        {
            samplesAsDays = true;
            Console.WriteLine("Sum hourly comsumpion to daily");

            for(int i = 0; i< listOfAllReadings.Count; i++) {
                listOfAllReadings[i] = TemperatureModifiers.SumToDaily(listOfAllReadings[i]);
            }
        }

        // TODO: Flag for showing consumption by day

        if(showOlyMatching && listOfAllReadings.Count == 2)
        {
            var group1 = listOfAllReadings[0].GroupBy(r => r.Temp).Select(r => {
                    return (r.Key, power: r.Average(v => v.Power), sampleCount: r.Count());
                }).OrderBy(r => r.Key).ToList();

            var group2 = listOfAllReadings[1].GroupBy(r => r.Temp).Select(r => {
                    return (r.Key, power: r.Average(v => v.Power), sampleCount: r.Count());
                }).OrderBy(r => r.Key).ToList();

            group1.ForEach(x =>
            {
                if(group2.FindIndex(y => y.Key == x.Key) >= 0) {
                    var match = group2.FirstOrDefault(y => y.Key == x.Key);
                    Console.WriteLine($"Temperature: {x.Key.ToString().PadRight(3)}, Sample1: power: {x.power.ToString("0.0000")}Kwh, sample count: {x.sampleCount.ToString().PadRight(3)}{(samplesAsDays ? "d" : "h")}. " +
                        $"Sample 2: power: {match.power.ToString("0.0000")}Kwh, sample count: {match.sampleCount.ToString().PadRight(3)}{(samplesAsDays ? "d" : "h")}, " +
                        $"power difference: {(match.power - x.power).ToString("0.0000")}Kwh");
                }
            });
        } else
        {
            foreach (var list in listOfAllReadings)
            {
                Console.WriteLine($"----Split {index}---");
                index++;
                var output = list.GroupBy(r => r.Temp).Select(r => {
                    return (r.Key, power: r.Average(v => v.Power));
                }).OrderBy(r => r.Key).ToList();

                output.ForEach(o => {
                    Console.WriteLine($"Temperature: {o.Key}: power: {o.power} kwh");
                });
            }
        }
    }
}
