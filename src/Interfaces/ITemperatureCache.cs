using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvPowerToTemp.Interfaces
{
    public interface ITemperatureCache
    {
        public int this[string key]
        {
            get; set;
        }
        void ClearCache(string location);
        bool ContainsKey(string dateAsDayComparable);
        Task InitializeCache(string location);
        void PersistData(string location);
    }
}
