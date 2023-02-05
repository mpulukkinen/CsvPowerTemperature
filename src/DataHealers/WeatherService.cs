using Serilog;
using System.Linq;
using System.Xml;

namespace CsvPowerToTemp.DataHealers;

public class WeatherService
{
    private bool _isFirstQuery = true;
    private HttpClient? _client;
    public async Task<Dictionary<string, int>> GetAvgTemperatureForDate(DateTime start, DateTime end, string location)
    {
        if (_isFirstQuery)
        {
            _client = new();
            _client.BaseAddress = new Uri("http://opendata.fmi.fi/");
            _isFirstQuery = false;
        }

        var timeFormat = "yyyy-MM-ddTHH:mm:ss";

        var temps = new Dictionary<string, List<float>>();

        var maxDaysRequest = 30;

        var tempStart = start;
        var tempEnd = start;

        while (tempEnd < end)
        {
            tempEnd = tempStart.AddDays(maxDaysRequest) > end ? end : tempStart.AddDays(maxDaysRequest);

            var request = "wfs/fin?service=WFS&version=2.0.0&request=getFeature&storedquery_id=fmi::observations::weather::daily::timevaluepair" +
                                                                    $"&place={location}" +
                                                                    $"&starttime={tempStart.ToString(timeFormat)}Z&" +
                                                                    $"&endtime={tempEnd.ToString(timeFormat)}Z&";
            tempStart = tempStart.AddDays(maxDaysRequest);

            try
            {
                var res = await _client!.GetAsync(request);
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = await res.Content.ReadAsStringAsync();

                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(content);
                    var xnList = xml.GetElementsByTagName("wml2:MeasurementTimeseries");

                    foreach (XmlNode xn in xnList)
                    {
                        foreach (XmlAttribute att in xn.Attributes)
                        {
                            var attName = att.Name;
                            var attValue = att.Value;
                            if (attName == "gml:id" && attValue == "obs-obs-1-1-tday")
                            {
                                foreach (XmlNode cn in xn.ChildNodes)
                                {
                                    var temp = "";
                                    var date = "";
                                    foreach (XmlNode child in cn.ChildNodes[0].ChildNodes)
                                    {
                                        if (child.Name == "wml2:value")
                                        {
                                            temp = child.InnerText;
                                        }

                                        if (child.Name == "wml2:time")
                                        {
                                            date = child.InnerText.Substring(0, 10);
                                        }
                                    }

                                    if (temp != null && date != null)
                                    {
                                        if (!temps.ContainsKey(date))
                                        {
                                            temps[date] = new List<float>();
                                        }
                                        temps[date].Add(float.Parse(temp));
                                    }
                                }                                
                            }
                        }                        
                    }
                }
                else
                {
                    Log.Warning($"Status code: {res.StatusCode}, start: {start}, location {location}");
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"Unable to get temperature for {start} - {end} in {location}");
            }
        }        

        if (temps.Count > 0)
        {
            var avg = temps.Select(s => (s.Key, (int)Math.Round(s.Value.Average()))).ToDictionary(k => k.Key, v => v.Item2);
            return avg;
        }

        return new Dictionary<string, int>();
    }
}