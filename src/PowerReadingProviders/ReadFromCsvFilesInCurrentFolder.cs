using Csv;
using CsvPowerToTemp.Interfaces;
using Serilog;
using System.Globalization;

namespace CsvPowerToTemp.PowerReadingProviders
{
    public class ReadFromCsvFilesInCurrentFolder : IPowerReadingProvider, IHelpProvider
    {
        public async Task<List<PowerReading>> GetPowerReadings(string[] args)
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csv");
            CultureInfo provider = CultureInfo.InvariantCulture;

            var currentList = new List<PowerReading>();

            if(!files.Any())
            {
                Log.Warning($"No csv files found from {Directory.GetCurrentDirectory()}");
                return currentList;
            }

            foreach (var file in files)
            {
                Log.Debug($"Processing file: {file}"); //, split by date: {splitByDate}, date to split: {dateSplit}");
                
                ProcessContents(provider, currentList, File.ReadAllText(file), Path.GetFileName(file));
            }

            return currentList;
        }

        public static void ProcessContents(CultureInfo provider, List<PowerReading> currentList, string content, string file)
        {
            var isOomi = file.StartsWith("Sähkö_");

            var dateFormat = isOomi ? "d.M.yyyy H:mm" : "d.M. H:mm:ss"; //  1.2.2022 0:00

            var year = isOomi ? 0 : int.Parse(string.Join("", Path.GetFileName(file).Split("_")[1].Take(4)));
            foreach (var line in CsvReader.ReadFromText(content))
            {
                if(line.ColumnCount < 3)
                {
                    continue;
                }
                // Header is handled, each line will contain the actual row data
                var time = line[0].Trim();
                var power = line[1];

                power = power.Replace(",", ".");

                if (string.IsNullOrEmpty(power))
                {
                    // Elenia makes empty values if you don't take whole year
                    break;
                }

                var temp = isOomi ? line[3]: line[2];

                temp = temp.Replace(",", ".");

                try
                {
                    var actualDate = DateTime.ParseExact(time, dateFormat, provider);
                    if (!isOomi)
                    {
                        actualDate = new DateTime(year, actualDate.Month, actualDate.Day, actualDate.Hour, actualDate.Minute, actualDate.Second);
                    }

                    var pwr = float.Parse(power, CultureInfo.InvariantCulture);
                    int? tmp = string.IsNullOrEmpty(temp) ? null : (int)Math.Round(float.Parse(temp, CultureInfo.InvariantCulture));

                    var reading = new PowerReading
                    {
                        Power = pwr,
                        Temp = tmp,
                        Time = actualDate,
                        Source = file
                    };
                    currentList.Add(reading);
                }
                catch (Exception e)
                {
                    Log.Error(e, $"Error processing line {line}");
                }
            }

            if(isOomi)
            {
                // Oomi data has hourly temperature values, conve
            }
        }

        public void PrintHelpToConsole()
        {
            Console.WriteLine("--- Read from csv files ---");
            Console.WriteLine($"Go to asiakas.elenia.fi -> Elenia Aina -> Katso kulutustiedot -> (choose time perioid) -> Lataa kulutus tuntitasolla -> (repeat with different time perioid if you like to compare");
            Console.WriteLine($"Copy our csv files to this ({Directory.GetCurrentDirectory()}) folder, do not modify them, especially not the name");
        }
    }
}
