using CsvPowerToTemp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvPowerToTemp.Caches
{
    public class FileCache : ITemperatureCache
    {
        private Dictionary<string, int> _cache = new();
        public void ClearCache(string location)
        {
            if(File.Exists(CacheLocation(location)))
            {
                File.Delete(CacheLocation(location));
            }
        }

        public int this[string key]
        {
            get { return _cache[key]; }
            set { _cache[key] = value; }
        }

        private string CacheLocation(string location)
        {
            return $"./tempCache_{location}.txt";
        }

        public async Task InitializeCache(string location)
        {
            if (File.Exists(CacheLocation(location)))
            {
                var lines = (await File.ReadAllTextAsync(CacheLocation(location))).Split("\n").Where(s => !string.IsNullOrEmpty(s)).ToList();

                lines.ForEach(l => {
                    var lineSplit = l.Split(":");
                    _cache[lineSplit[0]] = int.Parse(lineSplit[1]);
                });
            }
        }

        public bool ContainsKey(string dateAsDayComparable)
        {
            return _cache.ContainsKey(dateAsDayComparable);
        }

        public void PersistData(string location)
        {
            var tempCacheOutput = "";
            foreach (var kvp in _cache)
            {
                tempCacheOutput += kvp.Key + ":" + kvp.Value.ToString() + "\n";
            }

            if (File.Exists(CacheLocation(location)))
            {
                File.Delete(CacheLocation(location));
            }

            File.WriteAllText(CacheLocation(location), tempCacheOutput);
        }
    }
}
