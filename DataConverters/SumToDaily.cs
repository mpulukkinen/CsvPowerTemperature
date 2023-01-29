using CsvPowerToTemp.Interfaces;

namespace CsvPowerToTemp.DataConverters
{
    internal class SumToDaily : IDataConverter, IHelpProvider
    {
        public Task<List<List<PowerReading>>> ConvertData(List<List<PowerReading>> data, string[] args)
        {
            if (args.Contains("-s"))
            {
                var output = new List<List<PowerReading>>();

                for (int i = 0; i < data.Count; i++)
                {
                    output.Add(TemperatureModifiers.SumToDaily(data[i]));
                }

                return Task.FromResult(output);
            }

            return Task.FromResult(data);
        }

        public void PrintHelpToConsole()
        {
            Console.WriteLine("--- Clip consumption peaks ---");
            Console.WriteLine("-s Show total consumption by day");
        }
    }
}
