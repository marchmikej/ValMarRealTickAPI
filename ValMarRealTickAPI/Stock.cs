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
        public double highPrice;
        private List<int> topTradeVolume;
        private int lowTradeVolIndex;
        private bool initialized;
        private DateTime purchaseTime;
        private List<Trade> recentTrades;
        private bool buy;  //When buy is true then the main program will purchase the stock
        private bool sell;  //When sell is true then the main program will sell the stock
        public int tradesPerWeek;
        public int weeksLookBack;
        public double maxSecondsToHold;
        public double stopGap;
        public int recentTradesToKeep;
        public bool showTrades;
        public bool showBids;


        public Stock(string name, string exchange, int volumesToPurchase, int tradesPerWeek, int weeksLookBack, int maxSecondsToHold, double stopGap, int recentTradesToKeep)
        {
            this.name = name;
            this.exchange = exchange;
            topTradeVolume = new List<int>();
            lowTradeVolIndex = -1;
            initialized = false;
            volumesPurchased = 0;
            this.volumesToTrade = volumesToPurchase;
            buy = false;
            sell = false;
            highPrice = 0;
            recentTrades = new List<Trade>();
            showBids = false;
            showTrades = false;

            this.tradesPerWeek = tradesPerWeek;
            this.weeksLookBack = weeksLookBack;
            this.maxSecondsToHold = maxSecondsToHold;
            this.stopGap = stopGap;
            this.recentTradesToKeep = recentTradesToKeep;

            writeToFile("New Stock Created");
        }

        public void writeToFile(string newLine)
        {
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
        }

        public void addTrade(Trade newTrade)
        {
            if(recentTrades.Count > Variables.currentStock().recentTradesToKeep)
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
            if (topTradeVolume.Count < Variables.currentStock().tradesPerWeek * Variables.currentStock().weeksLookBack)
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
            for (int i = 0; i < topTradeVolume.Count; i++)
            {
                Console.WriteLine("{0} {1}", name, topTradeVolume[i]);
            }
        }

        //If this is false then sell stock immediately
        public bool stockTimeOK()
        {
            if( (DateTime.Now - purchaseTime).TotalSeconds > Variables.currentStock().maxSecondsToHold)
            {
                return false;
            }
            return true;
        }

        public void buySent()
        {
            buy = false;
            volumesPurchased = volumesToTrade;
            purchaseTime = DateTime.Now;
        }

        public void sellSent()
        {
            sell = false;
        }

        public void soldStock()
        {
            volumesPurchased = 0;
            highPrice = 0;
        }

        public int getVolumesToTrade()
        {
            return volumesToTrade;
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
            if(recentTrades.Count < Variables.currentStock().recentTradesToKeep)
            {
                return buy;
            } else if(recentTrades[0].amount < recentTrades[Variables.currentStock().recentTradesToKeep-1].amount)
            {
                buy = true;
                writeToFile("Buying Stock");
                writeToFile("Previous trade price: " + recentTrades[recentTrades.Count - 1].amount);
            }
            return buy;
        }
        
        public bool shouldBuy()
        {
            return buy;
        }

        public void setSell()
        {
            sell = true;
            writeToFile("Selling Stock");
            writeToFile("Previous trade price: " + recentTrades[recentTrades.Count - 1].amount);
        }

        public bool shouldSell()
        {
            if(stockHeld())
            {
                //Checks if we have exceeded hold time
                if ((DateTime.Now - purchaseTime).TotalSeconds > Variables.currentStock().maxSecondsToHold)
                {
                    sell = true;
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
            return highPrice - (highPrice * Variables.currentStock().stopGap);
        }

        public int getVolumesPurchased()
        {
            return volumesPurchased;
        }
    }
}
