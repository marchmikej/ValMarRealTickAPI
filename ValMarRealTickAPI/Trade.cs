using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValMarRealTickAPI
{
    public class Trade
    {
        public DateTime time;
        public int volume;
        public double amount;

        public Trade(DateTime time, int volume, double amount)
        {
            this.time = time;
            this.volume = volume;
            this.amount = amount;
        }
    }
}
