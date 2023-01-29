using CsvPowerToTemp.Interfaces;

namespace CsvPowerToTemp.DataConverters
{
    internal class ClipHighestConsumptions : IDataConverter, IHelpProvider
    {
        public Task<List<List<PowerReading>>> ConvertData(List<List<PowerReading>> data, string[] args)
        {
            if (args.Contains("-cl"))
            {
                var output = new List<List<PowerReading>>();
                for (int i = 0; i < data.Count; i++)
                {
                    output.Add(TemperatureModifiers.ClipHighestFourHourlyConsumption(data[i]));
                }

                return Task.FromResult(output);
            }

            return Task.FromResult(data);
        }

        public void PrintHelpToConsole()
        {
            Console.WriteLine("--- Clip consumption peaks ---");
            Console.WriteLine("-cl Clip daily results by removing 4 of the highest values (like warming up sauna etc)");
        }
    }
}
