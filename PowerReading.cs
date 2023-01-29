namespace CsvPowerToTemp;

public class PowerReading
{
    public DateTime Time {get; set;}
    public float Power {get; set;}
    public int Temp {get; set;}

    public string DateAsDayComparable => Time.ToString("dd.MM.yyy");
}