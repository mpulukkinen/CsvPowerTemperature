using Csv;
using CsvPowerToTemp.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvPowerToTemp.PowerReadingProviders
{
    internal class ReadFromCsvFilesInCurrentFolder : IPowerReadingProvider, IHelpProvider
    {
        public async Task<List<PowerReading>> GetPowerReadings(string[] args)
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csv");
            CultureInfo provider = CultureInfo.InvariantCulture;

            var currentList = new List<PowerReading>();

            if(!files.Any())
            {
                Console.WriteLine($"No csv files found from {Directory.GetCurrentDirectory()}");
                return currentList;
            }

            foreach (var file in files)
            {
                Console.WriteLine($"Processing file: {file}"); //, split by date: {splitByDate}, date to split: {dateSplit}");

                var year = int.Parse(string.Join("", file.Split("_")[1].Take(4)));

                foreach (var line in CsvReader.ReadFromText(File.ReadAllText(file)))
                {
                    // Header is handled, each line will contain the actual row data
                    var time = line[0];
                    var power = line[1];

                    power = power.Replace(",", ".");

                    if (string.IsNullOrEmpty(power))
                    {
                        // Elenia makes empty values if you don't take whole year
                        break;
                    }

                    var temp = line[2];

                    temp = temp.Replace(",", ".");

                    try
                    {
                        var actualDate = DateTime.ParseExact(time, "d.M. mm:hh:ss", provider);
                        actualDate = new DateTime(year, actualDate.Month, actualDate.Day, actualDate.Hour, actualDate.Minute, actualDate.Second);

                        //Console.WriteLine($"Time: {time}, actualDate: {actualDate}, power: {power}, temp: {temp}");

                        var reading = new PowerReading { Power = float.Parse(power), Temp = string.IsNullOrEmpty(temp) ? null: (int)Math.Round(float.Parse(temp)), Time = actualDate, Source = file};
                        currentList.Add(reading);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }                
            }

            return currentList;
        }

        public void PrintHelpToConsole()
        {
            Console.WriteLine("--- Read from csv files ---");
            Console.WriteLine($"Copy our csv files to this ({Directory.GetCurrentDirectory()}) folder, do not modify them, especially not the name");
        }
    }
}
