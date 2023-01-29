namespace CsvPowerToTemp;

public class TemperatureModifiers
{
    public static List<PowerReading> ClipHighest10PercentOfHourlyConsumption(List<PowerReading> list)
    {
        var output = new List<PowerReading>();
        foreach(var g in list.GroupBy(g => g.DateAsDayComparable)) {
            var valsAsList = g.ToList();
            var amountToReduce = (int)Math.Round(valsAsList.Count * 0.12d);

            /*var debugList = g.OrderBy(o => o.Power).Reverse().Take(amountToReduce).ToList();

            debugList.ForEach(t => Console.WriteLine($"Took out {t.Power} from {t.DateAsDayComparable}"));

            var nextInLIne = g.OrderBy(o => o.Power).Reverse().Skip(amountToReduce).First();

            Console.WriteLine($"Next in line: {nextInLIne.Power} from {nextInLIne.DateAsDayComparable}");*/

            output.AddRange(g.OrderBy(o => o.Power).Reverse().Skip(amountToReduce));
        }

        return output;
    }

    public static List<PowerReading> SumToDaily(List<PowerReading> list)
    {
        var output = new List<PowerReading>();
        foreach(var g in list.GroupBy(g => g.DateAsDayComparable)) {
            var valsAsList = g.ToList();
            var sum = valsAsList.Sum(s => s.Power);
            var reading = new PowerReading {Power = sum, Temp = valsAsList[0].Temp, Time = valsAsList[0].Time};
            output.Add(reading);
        }

        return output;
    }
}