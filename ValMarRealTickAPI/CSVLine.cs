using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValMarRealTickAPI
{
    class CSVLine
    {
        public string action;
        public int volume;
        public double price;
        public DateTime timeStamp;
        public DateTime currentTime;

        public CSVLine(string action, int volume, double price, DateTime timeStamp)
        {
            currentTime = DateTime.Now;
            this.action = action;
            this.volume = volume;
            this.price = price;
            this.timeStamp = timeStamp;
        }
    }
}
