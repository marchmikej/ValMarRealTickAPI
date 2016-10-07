using System;
using System.Collections.Generic;
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
        public double purchasePrice;
        private List<int> topTradeVolume;
        private int lowTradeVolIndex;
        private bool initialized;
        private DateTime purchaseTime;
        private bool buy;

        public Stock(string name, string exchange, int volumesToPurchase)
        {
            this.name = name;
            this.exchange = exchange;
            topTradeVolume = new List<int>();
            lowTradeVolIndex = -1;
            initialized = false;
            volumesPurchased = 0;
            this.volumesToTrade = volumesToPurchase;
            buy = false;
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
            if (topTradeVolume.Count < Variables.tradesPerWeek * Variables.weeksLookBack)
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

        public bool stockTimeOK()
        {
            if( (DateTime.Now - purchaseTime).TotalSeconds > Variables.maxSecondsToHold)
            {
                return false;
            }
            return true;
        }

        public void orderSent()
        {
            buy = false;
            volumesPurchased = volumesToTrade;
            purchaseTime = DateTime.Now;
        }

        public void soldStock()
        {
            volumesPurchased = 0;
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

        public void setBuy()
        {
            buy = true;
        }
        
        public bool shouldBuy()
        {
            return buy;
        }
    }
}
