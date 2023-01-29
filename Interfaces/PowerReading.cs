namespace CsvPowerToTemp.Interfaces;

public class PowerReading
{
    public DateTime Time { get; set; }
    public float Power { get; set; }
    public int? Temp { get; set; }
    public string DateAsDayComparable => Time.ToString("dd.MM.yyy");
    public string? Source { get; set; }
}