using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvPowerToTemp.Interfaces
{
    public interface IDataConverter
    {
        Task<List<List<PowerReading>>> ConvertData(List<List<PowerReading>> data, string[] args);
    }
}
