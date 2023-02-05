using CsvPowerToTemp.Interfaces;
using Serilog;
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
        public async Task HealData(List<PowerReading> data, string[] args, ITemperatureCache cache, string locationOverride = "")
        {
            var location = string.IsNullOrEmpty(locationOverride) ? "Lempäälä": locationOverride;

            var ix = args.ToList().IndexOf("-l");

            if (ix > 0 && string.IsNullOrEmpty(locationOverride))
            {
                location = args[ix + 1];
            }            

            if (args.Any(a => a == "-c"))
            {
                cache.ClearCache(location);
            }

            await cache.InitializeCache(location);

            foreach (var d in data)
            {                
                if (d.Temp == null)
                {
                    if (cache.ContainsKey(d.DateAsDayComparable))
                    {
                        d.Temp = cache[d.DateAsDayComparable];
                    }
                    else
                    {
                        var currentIndex = data.IndexOf(d) + 1;

                        var end = new DateTime(d.Time.Ticks);

                        // look ahead
                        for (int i = currentIndex; i < data.Count; i++)
                        {
                            if (data[i].Temp == null && !cache.ContainsKey(data[i].DateAsDayComparable))
                            {                 
                                end = data[i].Time;
                            } 
                            else
                            {
                                break;
                            }
                        }

                        var tempFromExternalSource = await _weather.GetAvgTemperatureForDate(d.Time, end, location);

                        foreach (var kvp in tempFromExternalSource) 
                        {
                            cache[kvp.Key] = kvp.Value;                            
                        }

                        if (cache.ContainsKey(d.DateAsDayComparable))
                        {
                            Console.WriteLine($"Temperature was null, found from open sources: {d.Time}: {cache[d.DateAsDayComparable]}");
                            d.Temp = cache[d.DateAsDayComparable];
                        }
                        else
                        {
                            Log.Warning($"Unable to get temperature for {d.DateAsDayComparable} in {location}");
                        }
                    }
                } 
                else
                {
                    cache[d.DateAsDayComparable] = d.Temp.Value;
                }
            }

            cache.PersistData(location);
        }

        public void PrintHelpToConsole()
        {
            Console.WriteLine("--- Fill missing temperaturs from FMI by location ---");
            Console.WriteLine("-c Clear temperature cache");
            Console.WriteLine("-l <location> (like: -l Lempäälä for example, check from logs )");
        }
    }
}
