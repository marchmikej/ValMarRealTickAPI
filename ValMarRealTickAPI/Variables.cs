using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValMarRealTickAPI
{
    public static class Variables
    {
        public const int tradesPerWeek = 10;
        public const int weeksLookBack = 2;
        public const double maxSecondsToHold = 1200;
        public const string route = "TALX";
        public static Stock[] stocks = new Stock[3] { new Stock("IBM", "NYS", 25), new Stock("COSI", "NAS", 75), new Stock("AAPL", "NAS", 25) };
        public static int currentStockIndex = 0;

        public static Stock currentStock()
        {
            return stocks[currentStockIndex];
        }
    }
}
