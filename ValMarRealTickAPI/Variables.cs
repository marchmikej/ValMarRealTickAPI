using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValMarRealTickAPI
{
    public static class Variables
    {
        //public const int tradesPerWeek = 60;
        //public const int weeksLookBack = 2;
        //public const double maxSecondsToHold = 1200;
        //public const double stopGap = .0008;
        //public const int recentTradesToKeep = 10;
        public const string route = "DEMO";
        public static bool runTrades = false;
        public static int barLookBack = 3;
        public static bool runProgram = true;
        public static bool runSimulation = false;
        public static int simulationDays = 65;
/*
        public static Stock[] stocks = new Stock[7] {
            new Stock("GWPH", "NAS", 100, 60, 2, 1200, .0008, 10),
            new Stock("SSYS", "NAS", 75, 60, 2, 1200, .0008, 10),
            new Stock("INSY", "NAS", 75, 60, 2, 1200, .0008, 10),
            new Stock("DVAX", "NAS", 100, 60, 2, 1200, .0008, 10),
            new Stock("ACAD", "NAS", 100, 60, 2, 1200, .0008, 10),
            new Stock("AMAT", "NAS", 100, 60, 2, 1200, .0008, 10),
            new Stock("DNR", "NYS", 100, 60, 2, 1200, .0008, 10)
        };
        */
        public static Dictionary<string, Stock> stocks = new Dictionary<string, Stock>();
        //public static Stock[] stocks = new Stock[2] { new Stock("IBM", "NYS", 25), new Stock("AAPL", "NAS", 25) };
        public static string currentStockIndex = "none";

        public static Stock currentStock()
        {
            return stocks[currentStockIndex];
        }
    }
}
