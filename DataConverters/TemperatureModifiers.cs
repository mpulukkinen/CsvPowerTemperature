using CsvPowerToTemp.Interfaces;

namespace CsvPowerToTemp;

public class TemperatureModifiers
{
    public static List<PowerReading> ClipHighestFourHourlyConsumption(List<PowerReading> list)
    {
        var output = new List<PowerReading>();
        foreach(var g in list.GroupBy(g => g.DateAsDayComparable)) {
            var valsAsList = g.ToList();

            // Only do this if there's readings for the whole day
            if(valsAsList.Count == 24) {
                var amountToReduce = 4;
                output.AddRange(g.OrderBy(o => o.Power).Reverse().Skip(amountToReduce));
            } 
            else
            {
                output.AddRange(valsAsList);
            }            
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