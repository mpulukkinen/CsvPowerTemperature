using CsvPowerToTemp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace CsvPowerToTemp.DataHealers
{
    internal class FillMissingTemperatureFromFmi : IDataHealer, IHelpProvider
    {
        private static WeatherService _weather = new WeatherService();
        public async Task HealData(List<PowerReading> data, string[] args)
        {
            var tempDict = new Dictionary<string, int>();
            var cacheLocation = "./tempCache.txt";

            if (args.Any(a => a == "-c") && File.Exists(cacheLocation))
            {
                File.Delete(cacheLocation);
            }

            var location = "Lempäälä";

            var ix = args.ToList().IndexOf("-l");

            if (ix > 0)
            {
                location = args[ix + 1];
            }

            if (File.Exists(cacheLocation))
            {
                var lines = (await File.ReadAllTextAsync(cacheLocation)).Split("\n").Where(s => !string.IsNullOrEmpty(s)).ToList();

                lines.ForEach(l => {
                    var lineSplit = l.Split(":");
                    tempDict[lineSplit[0]] = int.Parse(lineSplit[1]);
                });
            }

            foreach(var d in data)
            {                
                if (d.Temp == null)
                {
                    if (tempDict.ContainsKey(d.DateAsDayComparable))
                    {
                        d.Temp = tempDict[d.DateAsDayComparable];
                    }
                    else
                    {
                        Console.WriteLine($"Temperature was null, try to find from open sources: {d.Time}");

                        var tempFromExternalSource = await _weather.GetAvgTemperatureForDate(d.Time);

                        if (tempFromExternalSource != int.MaxValue)
                        {
                            Console.WriteLine($"Temperature for {d.DateAsDayComparable} in {location}");
                            tempDict[d.DateAsDayComparable] = tempFromExternalSource;
                            d.Temp = tempFromExternalSource;
                        }
                        else
                        {
                            Console.WriteLine($"Unable to get temperature for {d.DateAsDayComparable} in {location}");
                        }
                    }
                } 
                else
                {
                    tempDict[d.DateAsDayComparable] = d.Temp.Value;
                }
            }

            var tempCacheOutput = "";
            foreach (var kvp in tempDict)
            {
                tempCacheOutput += kvp.Key + ":" + kvp.Value.ToString() + "\n";
            }

            if (File.Exists(cacheLocation))
            {
                File.Delete(cacheLocation);
            }

            File.WriteAllText(cacheLocation, tempCacheOutput);
        }

        public void PrintHelpToConsole()
        {
            Console.WriteLine("--- Fill missing temperaturs from FMI by location ---");
            Console.WriteLine("-c Clear temperature cache");
            Console.WriteLine("-l <location> (like: -l Lempäälä for example, check from logs )");
        }
    }
}
