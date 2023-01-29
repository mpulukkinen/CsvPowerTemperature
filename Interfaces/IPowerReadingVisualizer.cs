using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvPowerToTemp.Interfaces
{
    internal interface IPowerReadingVisualizer : IHelpProvider
    {
        void VisualizeData(List<List<PowerReading>> data, string[] args);
    }
}
