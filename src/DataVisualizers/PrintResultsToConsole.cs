using CsvPowerToTemp.Interfaces;

namespace CsvPowerToTemp.DataVisualizers
{
    internal class PrintResultsToConsole : IPowerReadingVisualizer, IHelpProvider
    {
        public void PrintHelpToConsole()
        {
            Console.WriteLine("--- Console visualizer ---");
            Console.WriteLine("-m Show only matching results of samples (for example: if date splitted, print results only for those that have both at lest one -8 celsius result)");
        }

        public void VisualizeData(List<List<PowerReading>> data, string[] args)
        {
            var samplesAsDays = data.FirstOrDefault() is List<PowerReading> first && first.Count > 1 && first[0].DateAsDayComparable != first[1].DateAsDayComparable;
            var index = 1;

            var showOlyMatching = args.Any(a => a == "-m");

            if (showOlyMatching && data.Count == 2)
            {
                var group1 = data[0].GroupBy(r => r.Temp).Select(r => {
                    return (r.Key, power: r.Average(v => v.Power), sampleCount: r.Count());
                }).OrderBy(r => r.Key).ToList();

                var group2 = data[1].GroupBy(r => r.Temp).Select(r => {
                    return (r.Key, power: r.Average(v => v.Power), sampleCount: r.Count());
                }).OrderBy(r => r.Key).ToList();

                group1.ForEach(x =>
                {
                    if (group2.FindIndex(y => y.Key == x.Key) >= 0)
                    {
                        var match = group2.FirstOrDefault(y => y.Key == x.Key);
                        Console.WriteLine($"Temperature: {x.Key,-3}, Sample1: power: {x.power.ToString("0.0000")} Kwh, " +
                            $"sample count: {x.sampleCount,-3}{(samplesAsDays ? "d" : "h")}. " +
                            $"Sample 2: power: {match.power.ToString("0.0000")} Kwh, sample count: {match.sampleCount,-3}{(samplesAsDays ? "d" : "h")}, " +
                            $"power difference: {(match.power - x.power).ToString("0.0000")}Kwh");
                    }
                });
            }
            else
            {
                foreach (var list in data)
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
}
