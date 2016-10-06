using System;
using RealTick.Api.ClientAdapter;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using RealTick.Api.Application;
using RealTick.Api.Domain;
using RealTick.Api.Domain.Intraday;
using System.Diagnostics;
using RealTick.Api.Domain.Order;
using System.IO;

namespace ValMarRealTickAPI
{
    class Program
    {
        // the sample code expects the symbol to be in a variable or constant called "_symbol"  
        //static string _symbol = "IBM";
        enum Stocks { IBM, COSI, AAPL };
        static Stock[] stocks = new Stock[3] { new Stock("IBM", "NYS"), new Stock("COSI", "NYS"), new Stock("AAPL", "NYS") };
        static int currentStockIndex = 0;
       
        static void Main(string[] args)
        {
            Console.WriteLine("VALMARAPI STARTING");
             
            using (var app = new ClientAdapterToolkitApp())
            {
                //This portion of the program will initialize all of the stocks with data to analyze purchase 
                //for the day.
                for (int i = 0; i < stocks.Length; i++)
                {
                    currentStockIndex = i;
                    Console.WriteLine("Initializing: {0}", stocks[i].name);
                    RunIntraBar(app);
                    stocks[currentStockIndex].printTopTradeVol();
                    stocks[currentStockIndex].setInitialized();
                }
                System.Threading.Thread.Sleep(5000);
                
                while (true)
                {
                    for(int i = 0; i<stocks.Length; i++)
                    {
                        currentStockIndex = i;
                        Console.WriteLine("Evalulating: {0}", stocks[i].name);            
                        //_symbol = stocks[i].name;
                        RunIntraBar(app);
                        //RunPlaceOrder(app);
                    }
                    System.Threading.Thread.Sleep(5000);
                } 
            }         
        }

        private class Stock
        {
            public string name;
            public string exchange;
            public int tradesPerWeek;
            public int weeksLookBack;
            private List<int> topTradeVolume;
            private int lowTradeVolIndex;
            private bool initialized;

            public Stock(string name, string exchange)
            {
                this.name = name;
                this.exchange = exchange;
                tradesPerWeek = 10;
                weeksLookBack = 2;
                topTradeVolume = new List<int>();
                lowTradeVolIndex = -1;
                initialized = false;
            }

            public bool isInitialized()
            {
                return initialized;
            }

            public void setInitialized()
            {
                initialized = true;
            }

            public void addVolumeToList(int volumeChange)
            {
                if(topTradeVolume.Count < tradesPerWeek*weeksLookBack)
                {
                    topTradeVolume.Add(volumeChange);
                    getLowTradeIndex(true);  //Update lowTradeVol;
                } else if(topTradeVolume[getLowTradeIndex()]<volumeChange)
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
                if(lowTradeVolIndex == -1 || update)
                {
                    lowTradeVolIndex = 0;
                    for(int i=0;i<topTradeVolume.Count;i++)
                    {
                        if(topTradeVolume[i]<topTradeVolume[lowTradeVolIndex])
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
        }

        static void writeToFile(string newLine)
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string path = Path.Combine(folder, "valmarapi.log");
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

        //We have to single thread the data we get.  The symbol allows that to happen.
        static System.Threading.AutoResetEvent _evtGotData = new System.Threading.AutoResetEvent(false);
        // the sample code uses a method called "WriteLine" -- we can map this to Console.WriteLine() 
        static void WriteLine(string fmt, params object[] args)
        {
            Console.WriteLine(fmt, args);
        }

        // the sample code uses a method called "WaitAny" -- we can map this to WaitHandle.WaitOne()  
        static bool WaitAny(int timeout, WaitHandle handle)
        {
            handle.WaitOne(timeout);
            return true;
        }


        //This is the portion of code to get intra day bar data
        static void RunIntraBar(ToolkitApp app)
        {
            using (IntradayTable table = new IntradayTable(app))
            {
                //2nd input variable is the interval time for intraday bars
                int minuteInterval = 1;
                int dayBackToSearch = 0;
                if (!stocks[currentStockIndex].isInitialized())
                {
                    dayBackToSearch = stocks[currentStockIndex].weeksLookBack * 5;
                } 
            
                table.WantData(table.TqlForIntradayBars(stocks[currentStockIndex].name, minuteInterval, dayBackToSearch, false, null, null, null, null), true, false);
                table.OnIntraday += new EventHandler<DataEventArgs<IntradayRecord>>(table_OnIntraday);
                table.OnDead += new EventHandler<EventArgs>(table_OnDead);
                table.Start();
                WaitAny(10000, _evtGotData);
            }
            WriteLine("DONE");
        }

        static void table_OnDead(object sender, EventArgs e)
        {
            WriteLine("CONNECTION FAILED");
            _evtGotData.Set();
        }

        static void table_OnIntraday(object sender, DataEventArgs<IntradayRecord> e)
        {
            int max = 5000;
            int totalVolume = 0;
            foreach (IntradayRecord rec in e)
            {
                WriteLine("DATE\t\tTIME\t\tACVOL\tHIGH\tLOW\tOPEN");

                if (!stocks[currentStockIndex].isInitialized())
                {
                    for (int i = 0; i < rec.Count && i < max; i++)
                    {
                        WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                            rec.TrdDate[i].ToShortDateString(),
                            rec.TrdTim1[i],
                            rec.AcVol1[i],
                            rec.High1[i],
                            rec.Low1[i],
                            rec.OpenPrc[i]);
                        totalVolume += rec.AcVol1[i];
                        //Calculate volume change over last 3 minutes and add to stocks list
                        if (i > 2)
                        {
                            int volumeChange1 = rec.AcVol1[i - 2] - rec.AcVol1[i - 3];
                            int volumeChange2 = rec.AcVol1[i - 1] - rec.AcVol1[i - 2];
                            int volumeChange3 = rec.AcVol1[i] - rec.AcVol1[i - 1];
                            int totalVolumeChange = volumeChange1 + volumeChange2 + volumeChange3;

                            stocks[currentStockIndex].addVolumeToList(totalVolumeChange);
                        }
                    }
                }
                else 
                {
                    int i = rec.Count - 1;
                    WriteLine("Record count: {0}", i);
                    WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                        rec.TrdDate[i].ToShortDateString(),
                        rec.TrdTim1[i],
                        rec.AcVol1[i],
                        rec.High1[i],
                        rec.Low1[i],
                        rec.OpenPrc[i]);
                    totalVolume += rec.AcVol1[i];
                 
                    int volumeChange1 = rec.AcVol1[i - 2] - rec.AcVol1[i - 3];
                    int volumeChange2 = rec.AcVol1[i - 1] - rec.AcVol1[i - 2];
                    int volumeChange3 = rec.AcVol1[i] - rec.AcVol1[i - 1];
                    int totalVolumeChange = volumeChange1 + volumeChange2 + volumeChange3;
                    WriteLine("Current: {0} at {1} volume change over 3 minutes!!!!", stocks[currentStockIndex].name, totalVolumeChange);
                    if (stocks[currentStockIndex].getTradeVol() < totalVolumeChange)
                    {
                        //Buy!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        WriteLine("Purchase: {0} at {1} volume change!!!!", stocks[currentStockIndex].name, totalVolumeChange);
                        writeToFile("Purchasing " + stocks[currentStockIndex].name + " at " + DateTime.Now.ToString());
                    }
                }
                if (rec.Count >= max)
                    WriteLine("--- {0} MORE ROWS OMITTED --  ERROR IN PROCESSING", rec.Count - max);
            }
            _evtGotData.Set();
        }

        /// <summary>
        /// This is the portion of code to either purchase or sell a stock.
        /// </summary>
        private enum State { WaitingForConnect, OrderInPlay, OrderDone, ConnectionFailed };
        static State _state = State.WaitingForConnect;
        static readonly AutoResetEvent _event = new AutoResetEvent(false);

        static void RunPlaceOrder(ToolkitApp app)
        {
            using (var cache = new OrderCache(app))
            {
                cache.OnLive += CacheOnOnLive;
                cache.OnDead += CacheOnOnDead;
                cache.OnOrder += CacheOnOnOrder;

                _state = State.WaitingForConnect;
                cache.Start();

                while (_state != State.OrderDone && _state != State.ConnectionFailed)
                {
                    if (!WaitAny(10000, _event))
                    {
                        WriteLine("TIMED OUT WAITING FOR RESPONSE");
                        break;
                    }
                }

            } // end using cache
            WriteLine("DONE");
        }

        // one or more events have been received from the EMS
        static void CacheOnOnOrder(object sender, DataEventArgs<OrderRecord> dataEventArgs)
        {
            // NOTE: this logic assumes that only one order is in flight.  If you have multiple active orders, then you can distinguish 
            // among them by examining the OrderTag, which corresponds to the value from the OrderBuilder used to send the order.
            foreach (var ord in dataEventArgs)
            {
                //Removed because DisplayOrder is not showing up for this context
                //DisplayOrder(ord);
                WriteLine("Type: {0} Status: {1}", ord.Type, ord.CurrentStatus);
                if (ord.Type == "UserSubmitOrder")
                    if (ord.CurrentStatus == "COMPLETED" || ord.CurrentStatus == "DELETED")
                        _state = State.OrderDone;

                if (ord.Type == "ExchangeTradeOrder")
                {
                    WriteLine("GOT FILL FOR {0} {1} AT {2}", ord.Buyorsell, ord.Volume, ord.Price);
                }
                if (ord.Type == "ExchangeKillOrder")
                    WriteLine("GOT KILL");
            }
            _event.Set();
        }

        // connection to the EMS lost
        static void CacheOnOnDead(object sender, EventArgs eventArgs)
        {
            WriteLine("CONNECTION FAILED");
            _state = State.ConnectionFailed;
            _event.Set();
        }

        // connection to the EMS established, and account data received
        static void CacheOnOnLive(object sender, EventArgs eventArgs)
        {
            var cache = sender as OrderCache;
            string route = "TALX";
            int volume = 51;

            Trace.Assert(cache != null);

            WriteLine("SUBMITTING ORDER");

            // We send a market order, to maximize the chance that we will
            // successfully get a fill as desired for this example.
            var bld = new OrderBuilder(cache);
            bld.SetAccount(null, "TEST", null, null);
            bld.SetBuySell(OrderBuilder.BuySell.BUY);
            bld.SetExpiration(OrderBuilder.Expiration.DAY);
            bld.SetRoute(route);
            bld.SetSymbol(stocks[currentStockIndex].name, stocks[currentStockIndex].exchange, OrderBuilder.SecurityType.STOCK);
            bld.SetPriceMarket();
            bld.SetVolume(volume);
            cache.SubmitOrder(bld);

            _state = State.OrderInPlay;
            _event.Set();
        }
    }
}
