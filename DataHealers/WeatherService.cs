using System.Xml;

namespace CsvPowerToTemp.DataHealers;

public class WeatherService
{
    private bool _isFirstQuery = true;
    private HttpClient? _client;
    public async Task<int> GetAvgTemperatureForDate(DateTime time)
    {
        if (_isFirstQuery)
        {
            _client = new();
            _client.BaseAddress = new Uri("http://opendata.fmi.fi/");
            _isFirstQuery = false;
        }

        var start = time.ToString("yyyy-MM-dd");
        var end = time.AddDays(1).ToString("yyyy-MM-dd");

        var request = "wfs/fin?service=WFS&version=2.0.0&request=getFeature&storedquery_id=fmi::observations::weather::timevaluepair&place=Lempäälä" +
                                                                    "&parameters=t2m&" +
                                                                    $"starttime={start}T00:00:00Z&" +
                                                                    $"endtime={end}T00:00:00Z&" +
                                                                    "timestep=60&";

        try
        {
            var res = await _client!.GetAsync(request);
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = await res.Content.ReadAsStringAsync();

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(content);
                var xnList = xml.GetElementsByTagName("wml2:value");

                var temps = new List<float>();
                foreach (XmlNode xn in xnList)
                {
                    string temp = xn.InnerText;
                    if (!string.IsNullOrEmpty(temp))
                    {
                        temps.Add(float.Parse(temp));
                    }
                }

                if (temps.Count > 0)
                {
                    var avg = temps.Average();
                    Console.WriteLine($"Average temp for {time} is {avg}");
                    return (int)Math.Round(avg);
                }

                return int.MaxValue;

            }
            else
            {
                Console.WriteLine($"Status code: {res.StatusCode}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return int.MaxValue;
    }
}