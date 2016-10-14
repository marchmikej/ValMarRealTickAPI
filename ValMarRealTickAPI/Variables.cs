using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValMarRealTickAPI
{
    public static class Variables
    {
        public const int tradesPerWeek = 20;
        public const int weeksLookBack = 2;
        public const double maxSecondsToHold = 1200;
        public const double stopGap = .0004;
        public const string route = "DEMO";
        public static Stock[] stocks = new Stock[7] {
            new Stock("GWPH", "NAS", 100),
            new Stock("SSYS", "NAS", 75),
            new Stock("INSY", "NAS", 75),
            new Stock("DVAX", "NAS", 100),
            new Stock("ACAD", "NAS", 100),
            new Stock("AMAT", "NAS", 100),
            new Stock("DNR", "NYS", 100)
        };
        //public static Stock[] stocks = new Stock[2] { new Stock("IBM", "NYS", 25), new Stock("AAPL", "NAS", 25) };
        public static int currentStockIndex = 0;

        public static Stock currentStock()
        {
            return stocks[currentStockIndex];
        }
    }
}
