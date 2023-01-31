using CsvPowerToTemp.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvPowerToTemp.DataConverters
{
    internal class SplitDataByDateOrFile : IDataConverter, IHelpProvider
    {
        public Task<List<List<PowerReading>>> ConvertData(List<List<PowerReading>> data, string[] args)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            var splitByDate = args.Length > 1 && args.Any(a => a == "-d");

            var dateSplit = DateTime.Now;

            if (splitByDate)
            {
                dateSplit = DateTime.ParseExact(args[args.ToList().IndexOf("-d") + 1], "d.M.yyyy", provider);
            }

            var dateSplitted = false;

            List<List<PowerReading>> output = new List<List<PowerReading>>();

            var currentList = new List<PowerReading>();

            data.ForEach(x =>
            {
                var previousSource = "";
                x.ForEach(y =>
                {
                    if (splitByDate)
                    {
                        if(y.Time >= dateSplit && !dateSplitted)
                        {
                            Log.Information($"DO SPLIT (date): {dateSplit}");
                            output.Add(currentList);
                            currentList = new List<PowerReading>() { y };
                            dateSplitted = true;
                        } 
                        else
                        {
                            currentList.Add(y);
                        }
                    }
                    else
                    {
                        if(!string.IsNullOrEmpty(previousSource) && previousSource != y.Source)
                        {
                            Log.Information("DO SPLIT (file)");
                            output.Add(currentList);
                            currentList = new List<PowerReading>() { y };
                        } else
                        {
                            currentList.Add(y);
                        }

                        previousSource = y.Source;
                    }
                });
            });

            if(currentList.Any())
            {
                output.Add(currentList);
            }

            return Task.FromResult(output);

        }

        public void PrintHelpToConsole()
        {
            Console.WriteLine("--- Date and file splitter ---");
            Console.WriteLine("-d Split point for data for comparison (d.M.yyyy format, like 3.12.2022 for 3rd of December 2022). If not defined, split by csv files");
        }
    }
}
