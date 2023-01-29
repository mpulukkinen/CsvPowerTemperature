using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvPowerToTemp.Interfaces
{
    internal interface IDataHealer
    {
        Task HealData(List<PowerReading> data, string[] args);
    }
}
