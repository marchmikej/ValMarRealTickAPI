using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValMarRealTickAPI
{
    public class Stock
    {
        public string name;
        public string exchange;
        private int volumesPurchased;
        private int volumesToTrade;
        private int dollarAmountToPurchase;
        public double highPrice;
        private List<int> topTradeVolume;
        private int lowTradeVolIndex;
        private bool initialized;
        private DateTime purchaseTime;
        private List<Trade> recentTrades;
        private bool buy;  //When buy is true then the main program will purchase the stock
        private bool sell;  //When sell is true then the main program will sell the stock
        private int trendDownSeconds;
        private DateTime lastTrendDownTime;
        public int tradesPerWeek;
        public int weeksLookBack;
        public double maxSecondsToHold;
        public double stopGap1;
        public double stopGap2;
        public int secondsStopGap1;
        public int recentTradesToKeep;
        public bool showTrades;
        public bool showBids;
        public int averageVolumeHistory;
        private int waitAfterSellSeconds;
        private DateTime lastSellTime;
        private Queue<CSVLine> csvLines;
        private DateTime lastWriteCSV;
        public int startHour;
        public int startMinute;
        public int endHour;
        public int endMinute;

        public Stock(string name, string exchange, int tradesPerWeek, int weeksLookBack, int maxSecondsToHold, double stopGap, double stopGap2, int secondsStopGap1, int recentTradesToKeep, int trendDownSeconds, int waitAfterSellSeconds, int startHour, int startMinute, int endHour, int endMinute, int dollarAmountToPurchase)
        {
            this.name = name;
            this.exchange = exchange;
            topTradeVolume = new List<int>();
            lowTradeVolIndex = -1;
            initialized = false;
            volumesPurchased = 0;
            buy = false;
            sell = false;
            highPrice = 0;
            recentTrades = new List<Trade>();
            showBids = false;
            showTrades = false;
            averageVolumeHistory = 0;

            this.tradesPerWeek = tradesPerWeek;
            this.weeksLookBack = weeksLookBack;
            this.maxSecondsToHold = maxSecondsToHold;
            this.stopGap1 = stopGap;
            this.stopGap2 = stopGap2;
            this.secondsStopGap1 = secondsStopGap1;
            this.recentTradesToKeep = recentTradesToKeep;
            this.trendDownSeconds = trendDownSeconds;
            this.waitAfterSellSeconds = waitAfterSellSeconds;
            this.endHour = endHour;
            this.endMinute = endMinute;
            this.startHour = startHour;
            this.startMinute = startMinute;
            this.dollarAmountToPurchase = dollarAmountToPurchase;

            // Initializing below times minus the buffer so they will not wait to buy at initial launch
            lastSellTime = DateTime.Now.AddSeconds(waitAfterSellSeconds * -1);
            lastTrendDownTime = DateTime.Now.AddSeconds(trendDownSeconds * -1); ;
            writeToFile("New Stock Created");
            csvLines = new Queue<CSVLine>();
            lastWriteCSV = DateTime.Now;
        }

        public void writeToFile(string newLine)
        {
            //Removed writing to file on 11/21/2016 no one is using this log at the moment.
            /*
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string path = Path.Combine(folder, name + ".log");
            newLine = DateTime.Now + " " + newLine;
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(newLine);
                }
            }

            // This text is always added, making the file longer over time
            // if it is not deleted.
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(newLine);
            }
            */
        }

        public void writeToCSV(string action, int volume, double price, DateTime timeStamp)
        {
            csvLines.Enqueue(new CSVLine(action, volume, price, timeStamp));
        }

        public void printToCSV()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string path = Path.Combine(folder, name + ".csv");
            if (Variables.runSimulation)
            {
                path = Path.Combine(folder, name + "_simulation.csv");
            } 

            if ((DateTime.Now - lastWriteCSV).TotalSeconds > 60  || Variables.runSimulation) {
                lastWriteCSV = DateTime.Now;
                if (!File.Exists(path))
                {
                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine("timestamp, timegiven, action, volume, price");
                    }
                }

                // This text is always added, making the file longer over time
                // if it is not deleted.
                using (StreamWriter sw = File.AppendText(path))
                {
                    while (csvLines.Count > 0)
                    {
                        CSVLine newLine = csvLines.Dequeue();
                        sw.WriteLine(newLine.currentTime + "," + newLine.timeStamp + "," + newLine.action + "," + newLine.volume + "," + newLine.price);
                    }
                }
            }
        }

        public void addTrade(Trade newTrade)
        {
            writeToCSV("TRADE", newTrade.volume, newTrade.amount, newTrade.time);
            if(recentTrades.Count > recentTradesToKeep)
            {
                recentTrades.RemoveAt(0);
            }
            recentTrades.Add(newTrade);
        }

        public List<Trade> getRecentTrades()
        {
            return recentTrades;
        }

        // When true the stock has had it's history checked and the volume at which to trade should be set
        public bool isInitialized()
        {
            return initialized;
        }

        public void setInitialized()
        {
            initialized = true;
        }

        // Adds volume to topTradeVolumeList if it is one of the top volumes
        public void addVolumeToList(int volumeChange)
        {
            if (topTradeVolume.Count < tradesPerWeek * weeksLookBack)
            {
                topTradeVolume.Add(volumeChange);
                getLowTradeIndex(true);  //Update lowTradeVol;
            }
            else if (topTradeVolume[getLowTradeIndex()] < volumeChange)
            {
                topTradeVolume[getLowTradeIndex()] = volumeChange;
                getLowTradeIndex(true);
            }

        }

        // This will return the index of the lowest trade volume index
        private int getLowTradeIndex()
        {
            return getLowTradeIndex(false);
        }

        // If true then the lowTradVol is updated
        private int getLowTradeIndex(bool update)
        {
            if (lowTradeVolIndex == -1 || update)
            {
                lowTradeVolIndex = 0;
                for (int i = 0; i < topTradeVolume.Count; i++)
                {
                    if (topTradeVolume[i] < topTradeVolume[lowTradeVolIndex])
                    {
                        lowTradeVolIndex = i;
                    }
                }
            }
            return lowTradeVolIndex;
        }

        public int getTradeVol()
        {
            return topTradeVolume[lowTradeVolIndex];
        }

        public void printTopTradeVol()
        {
            Console.WriteLine("Printing top trade volume for {0} size {1}", name, topTradeVolume.Count);
            for (int i = 0; i < topTradeVolume.Count; i++)
            {
                Console.WriteLine("{0} {1}", name, topTradeVolume[i]);
            }
        }

        //If this is false then sell stock immediately
        public bool stockTimeOK()
        {
            if( (DateTime.Now - purchaseTime).TotalSeconds > maxSecondsToHold)
            {
                return false;
            }
            return true;
        }

        public void buySent()
        {
            buy = false;
            //volumesPurchased = volumesToTrade;
            purchaseTime = DateTime.Now;
        }

        public void sellSent()
        {
            sell = false;
            // Added on 11/18/2016 so Adam could specify a buffer in between the last sell and the next 
            // available buy.
            lastSellTime = DateTime.Now;
        }

        public void soldStock()
        {
            //volumesPurchased = 0;
            highPrice = 0;
        }

        public int getVolumesToTrade()
        {
            int volumesToTradeNow = (int)(dollarAmountToPurchase / recentTrades[0].amount);
            return volumesToTradeNow;
        }

        public bool stockHeld()
        {
            if(volumesPurchased > 0)
            {
                return true;
            }
            return false;
        }

        public bool setBuy()
        {
            if (recentTrades.Count < recentTradesToKeep)
            {
                return buy;
            } else if(recentTrades[0].amount < recentTrades[recentTradesToKeep-1].amount)
            {
                // Need to ensure that we are not still in the trending down period
                // if yes then send message to the log
                if ((DateTime.Now - lastTrendDownTime).TotalSeconds > trendDownSeconds)
                {
                    // Added on 11/19/2016 because Adam wanted a buffer to buy after the last sell
                    // To many times we would sell and then buy again at to quick a time
                    // He wanted to be able to add a buffer
                    if ((DateTime.Now - lastSellTime).TotalSeconds > waitAfterSellSeconds)
                    {
                        buy = true;
                        writeToCSV("SETBUY", 0, 0, DateTime.Now);
                        writeToFile("Buying Stock");
                        writeToFile("Previous trade price: " + recentTrades[recentTrades.Count - 1].amount);
                    }
                    else
                    {
                        writeToCSV("NOBUYINWAITAFTERLASTSELL", 0, 0, DateTime.Now);
                    }
                } else
                {
                    writeToCSV("NOBUYINTRENDINGDOWNWAIT", 0, 0, DateTime.Now);
                }
            } else
            {
                lastTrendDownTime = DateTime.Now;
                writeToCSV("NOBUYTRENDINGDOWN", 0, 0, DateTime.Now);
            }
            return buy;
        }

        public void tradeComplete(string buyOrSell, int volumeTraded, double pricedTraded)
        {
            writeToCSV("TRADECOMPLETE" + buyOrSell, volumeTraded, pricedTraded, DateTime.Now);
            //volumesPurchased = volumesPurchased + volumeTraded;
            if(buyOrSell=="Buy")
            {
                volumesPurchased = volumesPurchased + volumeTraded;
            } else if(buyOrSell=="Sell")
            {
                volumesPurchased = volumesPurchased - volumeTraded;
            }
        }

        public void buyComplete(string buyOrSell, int volumeTraded, double pricedTraded)
        {
            writeToCSV("BUY", volumeTraded, pricedTraded, DateTime.Now);
            volumesPurchased = volumesPurchased + volumeTraded;
        }

        public void saleComplete(string buyOrSell, int volumeTraded, double pricedTraded)
        {
            writeToCSV("SELL", volumeTraded, pricedTraded, DateTime.Now);
            volumesPurchased = volumesPurchased - volumeTraded;
        }

        public bool shouldBuy()
        {
            return buy;
        }

        public void setSell()
        {
            sell = true;
            writeToCSV("SETSELL", 0, 0, DateTime.Now);
            writeToFile("Selling Stock");
            writeToFile("Previous trade price: " + recentTrades[recentTrades.Count - 1].amount);
        }

        public bool shouldSell()
        {
            if(volumesPurchased == volumesToTrade)
            {
                //Checks if we have exceeded hold time
                if ((DateTime.Now - purchaseTime).TotalSeconds > maxSecondsToHold)
                {
                    sell = true;
                    writeToCSV("SETSELL", 0, 0, DateTime.Now);
                    writeToFile("Selling Stock");
                    writeToFile("Previous trade price: " + recentTrades[recentTrades.Count - 1].amount);
                }
                return sell;
            }
            return false;
        }

        public void newPrice(double newTradePrice)
        {
            //If stock price goes up that is our new high water mark
            if(newTradePrice > highPrice)
            {
                highPrice = newTradePrice;
            } else
            {
                //If price of stock dips below stop gap percentage then sell
                if(getCurrentStopGapPrice() > newTradePrice)
                {
                    sell = true;
                    writeToFile("Selling Stock");
                    writeToFile("Previous trade price: " + recentTrades[recentTrades.Count - 1].amount);
                }
            }
        }

        public double getCurrentStopGapPrice()
        {
            if ((DateTime.Now - purchaseTime).TotalSeconds > secondsStopGap1)
            {
                return highPrice - (highPrice * stopGap2);
            } else
            {
                return highPrice - (highPrice * stopGap1);
            }
        }

        public int getVolumesPurchased()
        {
            return volumesPurchased;
        }
    }
}
