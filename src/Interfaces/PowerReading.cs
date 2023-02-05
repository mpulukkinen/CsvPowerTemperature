namespace CsvPowerToTemp.Interfaces;

public class PowerReading
{
    public DateTime Time { get; set; }
    public float Power { get; set; }
    public int? Temp { get; set; }
    public string DateAsDayComparable => Time.ToString("yyyy-MM-dd"); // NOTE: in sycn with fmi
    public string? Source { get; set; }
}