﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvPowerToTemp.Interfaces
{
    public interface IPowerReadingProvider
    {
        public Task<List<PowerReading>> GetPowerReadings(string[] args);
    }
}
