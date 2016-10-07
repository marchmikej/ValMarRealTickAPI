﻿using System;
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

namespace ValMarRealTickAPI
{
    class Program
    {
        // the sample code expects the symbol to be in a variable or constant called "_symbol"  
       
        static void Main(string[] args)
        {
            Console.WriteLine("VALMARAPI STARTING");
             
            using (var app = new ClientAdapterToolkitApp())
            {
                //This portion of the program will initialize all of the stocks with data to analyze purchase 
                //for the day.
                for (int i = 0; i < Variables.stocks.Length; i++)
                {
                    Variables.currentStockIndex = i;
                    Console.WriteLine("Initializing: {0}", Variables.currentStock().name);
                    RunIntraBar(app);
                    Variables.currentStock().printTopTradeVol();
                    Variables.currentStock().setInitialized();
                }
                
                while (true)
                {
                    for(int i = 0; i< Variables.stocks.Length; i++)
                    {
                        Variables.currentStockIndex = i;
                        Console.WriteLine("Evalulating: {0}", Variables.currentStock().name);  
                        if(!Variables.currentStock().stockHeld())
                        {
                            RunIntraBar(app);
                        }
                        else
                        {
                            //Check current stock price
                        }
                        
                        if(Variables.currentStock().shouldBuy())
                        {
                            Helper.writeToFile("Purchasing " + Variables.currentStock().name + " at " + DateTime.Now.ToString());
                            Variables.currentStock().orderSent();
                            RunPlaceOrder(app);
                        }                      
                    }
                    System.Threading.Thread.Sleep(5000);
                } 
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
 /*       static bool WaitAny(int timeout, WaitHandle handle)
        {
            handle.WaitOne(timeout);
            return true;
        } */
        private static bool WaitForIntervalOrError(int millisecondsTimeout, params System.Threading.WaitHandle[] errorConditionHandles)
        {
            // We are basically just calling System.Threading.WaitHandle.WaitAny on the handle(s) provided,
            // but we also include the example's _stopHandle in the list of handles;  this is an event that
            // gets fired when the user clicks the "Stop" button, allowing us to have a more responsive GUI.
            // In a command-line version of the same example, you would leave that part out.

            // This version is for when the example wants to wait for a specified time

            System.Threading.WaitHandle[] ar = new System.Threading.WaitHandle[errorConditionHandles.Length + 1];
            ar[0] = _stopEvent;
            for (int i = 0; i < errorConditionHandles.Length; i++)
                ar[i + 1] = errorConditionHandles[i];

            int n = System.Threading.WaitHandle.WaitAny(ar, millisecondsTimeout);
            if (n == System.Threading.WaitHandle.WaitTimeout)
                return true;

            if (n == 0)
            {
                WriteLine("CANCELLED BY USER");
                return false;
            }
            return false;
        }

        private static System.Threading.AutoResetEvent _stopEvent = new System.Threading.AutoResetEvent(false);

        private static bool WaitAny(int millisecondsTimeout, params System.Threading.WaitHandle[] successConditionHandles)
        {
            // We are basically just calling System.Threading.WaitHandle.WaitAny on the handle(s) provided,
            // but we also include the example's _stopHandle in the list of handles;  this is an event that
            // gets fired when the user clicks the "Stop" button, allowing us to have a more responsive GUI.
            // In a command-line version of the same example, you would leave that part out.

            // This version is for when the example wants to wait for a particular condition

            System.Threading.WaitHandle[] ar = new System.Threading.WaitHandle[successConditionHandles.Length + 1];
            ar[0] = _stopEvent;
            for (int i = 0; i < successConditionHandles.Length; i++)
                ar[i + 1] = successConditionHandles[i];

            int n = System.Threading.WaitHandle.WaitAny(ar, millisecondsTimeout);
            if (n == System.Threading.WaitHandle.WaitTimeout)
            {
                WriteLine("TIMED OUT WAITING FOR A RESPONSE");
                return false;
            }
            if (n == 0)
            {
                WriteLine("CANCELLED BY USER");
                return false;
            }
            return true;
        }
        /*************************************************************************************************/
        /* Start intra day bar data                                                                      */
        /*************************************************************************************************/
        static void RunIntraBar(ToolkitApp app)
        {
            using (IntradayTable table = new IntradayTable(app))
            {
                //2nd input variable is the interval time for intraday bars
                int minuteInterval = 1;
                int dayBackToSearch = 0;
                //If not initialized then look back the specified number of weeks, otherwise only search today.
                if (!Variables.currentStock().isInitialized())
                {
                    dayBackToSearch = Variables.weeksLookBack * 5;
                } 
            
                table.WantData(table.TqlForIntradayBars(Variables.currentStock().name, minuteInterval, dayBackToSearch, false, null, null, null, null), true, false);
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

        //Calculates volume change over 3 minute period
        static int calculateVolumeChange(IntradayRecord rec, int i)
        {
            int volumeChange1 = rec.AcVol1[i - 2] - rec.AcVol1[i - 3];
            int volumeChange2 = rec.AcVol1[i - 1] - rec.AcVol1[i - 2];
            int volumeChange3 = rec.AcVol1[i] - rec.AcVol1[i - 1];
            int totalVolumeChange = volumeChange1 + volumeChange2 + volumeChange3;
            return totalVolumeChange;
        }

        static void table_OnIntraday(object sender, DataEventArgs<IntradayRecord> e)
        {
            int max = 5000;
            foreach (IntradayRecord rec in e)
            {
                WriteLine("DATE\t\tTIME\t\tACVOL\tHIGH\tLOW\tOPEN");

                if (!Variables.currentStock().isInitialized())
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
                        //Calculate volume change over last 3 minutes and add to stocks list
                        if (i > 2)
                        {
                            Variables.currentStock().addVolumeToList(calculateVolumeChange(rec, i));
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

                    int totalVolumeChange = calculateVolumeChange(rec, i);
                    
                    WriteLine("Current: {0} at {1} volume change over 3 minutes, buy is {2}", Variables.currentStock().name, totalVolumeChange, Variables.currentStock().getTradeVol());
                    if (Variables.currentStock().getTradeVol() < totalVolumeChange)
                    {
                        //Buy!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        WriteLine("Purchase: {0} at {1} volume change!!!!", Variables.currentStock().name, totalVolumeChange);
                        //This sets the current stock to a buy status which will be executed above
                        //because I was unsure of the best way to pass app to the purchase method.
                        Variables.currentStock().setBuy();
                    }
                }
                if (rec.Count >= max)
                    WriteLine("--- {0} MORE ROWS OMITTED --  ERROR IN PROCESSING", rec.Count - max);
            }
            _evtGotData.Set();
        }

        /*************************************************************************************************/
        /* End intra day bar data                                                                       */
        /*************************************************************************************************/

        /*************************************************************************************************/
        /* Start Buy/Sell portion                                                                        */
        /*************************************************************************************************/
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
                {
                    if (ord.CurrentStatus == "COMPLETED" || ord.CurrentStatus == "DELETED")
                    {
                        _state = State.OrderDone;
                    }
                    if (ord.CurrentStatus == "DELETED")
                    {
                        WriteLine("Purchase failed user submit deleted for {0}", ord.DispName);
                        Helper.writeToFile("Purchase failed user submit deleted for " + ord.DispName + " at " + DateTime.Now.ToString());
                    }
                }

                if (ord.Type == "ExchangeTradeOrder")
                {
                    //Purchase successful 
                    WriteLine("GOT FILL FOR {0} {1} AT {2} for {3}", ord.Buyorsell, ord.Volume, ord.Price, ord.DispName);
                    Helper.writeToFile("Purchase complete for: " + ord.DispName + " at " + DateTime.Now.ToString());
                }
                if (ord.Type == "ExchangeKillOrder")
                {
                    WriteLine("GOT KILL");
                    Helper.writeToFile("Purchase failed transaction killed " + ord.DispName + " at " + DateTime.Now.ToString());
                }
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

            Trace.Assert(cache != null);

            WriteLine("SUBMITTING ORDER");

            // We send a market order, to maximize the chance that we will
            // successfully get a fill as desired for this example.
            var bld = new OrderBuilder(cache);
            bld.SetAccount(null, "TEST", null, null);
            //This will determine if we are selling or buying stock
            if (Variables.currentStock().shouldBuy())
            {
                bld.SetBuySell(OrderBuilder.BuySell.SELL);
            }
            else
            {
                bld.SetBuySell(OrderBuilder.BuySell.BUY);
            }
            bld.SetExpiration(OrderBuilder.Expiration.DAY);
            bld.SetRoute(Variables.route);
            bld.SetSymbol(Variables.currentStock().name, Variables.currentStock().exchange, OrderBuilder.SecurityType.STOCK);
            bld.SetPriceMarket();
            bld.SetVolume(Variables.currentStock().getVolumesToTrade());
            cache.SubmitOrder(bld);

            //Let stock know that it should not attempt to purchase anymore volumes.
            Variables.currentStock().orderSent();

            _state = State.OrderInPlay;
            _event.Set();
        }
        /*************************************************************************************************/
        /* End Buy/Sell portion                                                                        */
        /*************************************************************************************************/
    }
}
