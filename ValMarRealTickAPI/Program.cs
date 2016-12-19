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
        static void Main(string[] args)
        {
            Console.WriteLine("VALMARAPI STARTING");
             
            using (var app = new ClientAdapterToolkitApp())
            {
                // Launches Form for variable to be inputed and stocks to be started for watching
                Task.Run(() =>
                {
                    Watch_Stock watch_form = new Watch_Stock(app);
                    watch_form.ShowDialog();
                });

                while (Variables.runProgram)
                {
                    while (Variables.runTrades)
                    {
                        // Set variables if we are running a simulation
                        if(Variables.runSimulation)
                        {
                            Variables.runProgram = false;
                            Variables.runTrades = false;
                        }
                        foreach (string item in Variables.stocks.Keys)
                        {
                            Variables.currentStockIndex = item;
                            Variables.currentStock().printToCSV();
                            Console.WriteLine("Evalulating: {0}", Variables.currentStock().name);
                            if (!Variables.currentStock().isInitialized() && !Variables.runSimulation)
                            {
                                Task.Run(() =>
                                {
                                    Form1 form = new Form1(app, Variables.currentStockIndex);
                                    form.ShowDialog();
                                });
                                System.Threading.Thread.Sleep(10000);
                                RunIntraBar(app);
                                System.Threading.Thread.Sleep(10000);

                            }
                            if (!Variables.currentStock().stockHeld())
                            {
                                RunIntraBar(app);
                            }

                            if (Variables.currentStock().shouldBuy() && !Variables.runSimulation)
                            {
                                writeToFile("Purchasing " + Variables.currentStock().name + " at " + DateTime.Now.ToString());
                                RunPlaceOrder(app);
                            }
                            if (Variables.currentStock().shouldSell() && !Variables.runSimulation)
                            {
                                writeToFile("Selling " + Variables.currentStock().name + " at " + DateTime.Now.ToString());
                                RunPlaceOrder(app);
                            }
                        }
                        System.Threading.Thread.Sleep(5000);
                    }
                }
            }
            Console.WriteLine("Shutting Down");
            System.Threading.Thread.Sleep(1000);
        }

        public static void writeToFile(string newLine)
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
                    dayBackToSearch = Variables.currentStock().weeksLookBack * 5;
                }
                //If simulation go back extra time for eval
                if (Variables.runSimulation)
                {
                    WriteLine("Kicking off simulation");
                    dayBackToSearch += Variables.simulationDays;
                    table.WantData(table.TqlForIntradayBars(Variables.currentStock().name, minuteInterval, dayBackToSearch, false, null, null, null, null), true, false);
                    table.OnIntraday += new EventHandler<DataEventArgs<IntradayRecord>>(simulation_OnIntraday);
                    table.OnDead += new EventHandler<EventArgs>(table_OnDead);
                    table.Start();
                    WaitAny(10000, _evtGotData);
                }
                else
                {
                    table.WantData(table.TqlForIntradayBars(Variables.currentStock().name, minuteInterval, dayBackToSearch, false, null, null, null, null), true, false);
                    table.OnIntraday += new EventHandler<DataEventArgs<IntradayRecord>>(table_OnIntraday);
                    table.OnDead += new EventHandler<EventArgs>(table_OnDead);
                    table.Start();
                    WaitAny(10000, _evtGotData);
                }
            }
            WriteLine("DONE");
        }

        static void table_OnDead(object sender, EventArgs e)
        {
            WriteLine("CONNECTION FAILED");
            _evtGotData.Set();
        }

        //Calculates volume change over X minute period
        static int calculateVolumeChange(IntradayRecord rec, int startingRecord)
        {
            //Removed for updated formula provided by John on 11/15/2016
            /*
            int volumeChange1 = rec.AcVol1[i - 2] - rec.AcVol1[i - 3];
            int volumeChange2 = rec.AcVol1[i - 1] - rec.AcVol1[i - 2];
            int volumeChange3 = rec.AcVol1[i] - rec.AcVol1[i - 1];
            int totalVolumeChange = volumeChange1 + volumeChange2 + volumeChange3;
            return totalVolumeChange; */

            int averageVolumeChange = 0;

            for(int j=0;j<Variables.barLookBack;j++)
            {
                averageVolumeChange += rec.AcVol1[startingRecord - j];
            }
            return averageVolumeChange / Variables.barLookBack;
        }

        static void table_OnIntraday(object sender, DataEventArgs<IntradayRecord> e)
        {
            foreach (IntradayRecord rec in e)
            {
                WriteLine("DATE\t\tTIME\t\tACVOL\tHIGH\tLOW\tOPEN");
                // Skip this evaluation if rec.Count == 0 no data was returned
                if (rec.Count == 0)
                {
                    continue;
                }
                if (!Variables.currentStock().isInitialized())
                {
                    //Calculate average volume for the look back time
                    // Added on 11/15/2016 for john's vol3 calculation
                    int tempVolumeCount = 0;
                    int tempVolumesUsed = 0;
                    for (int i = 0; i < rec.Count; i++)
                    {
                        // Added time code on 12/17/2016.  This portion does not trade or use setup time outside of given hours
                        if ((Variables.currentStock().startHour > rec.TrdTim1[i].Hours) || (Variables.currentStock().startHour == rec.TrdTim1[i].Hours && Variables.currentStock().startMinute > rec.TrdTim1[i].Minutes))
                        {
                            continue;
                        }
                        else if ((Variables.currentStock().endHour < rec.TrdTim1[i].Hours) || (Variables.currentStock().endHour == rec.TrdTim1[i].Hours && Variables.currentStock().endMinute < rec.TrdTim1[i].Minutes))
                        {
                            continue;
                        }
                        tempVolumeCount += rec.AcVol1[i];
                        tempVolumesUsed++;
                    }
                    Variables.currentStock().averageVolumeHistory = tempVolumeCount / tempVolumesUsed;

                    //Calculate the top differences over the look back time
                    for (int i = 0; i < rec.Count; i++)
                    {
                        // Added time code on 12/17/2016.  This portion does not trade or use setup time outside of given hours
                        if ((Variables.currentStock().startHour > rec.TrdTim1[i].Hours) || (Variables.currentStock().startHour == rec.TrdTim1[i].Hours && Variables.currentStock().startMinute > rec.TrdTim1[i].Minutes))
                        {
                            continue;
                        }
                        else if ((Variables.currentStock().endHour < rec.TrdTim1[i].Hours) || (Variables.currentStock().endHour == rec.TrdTim1[i].Hours && Variables.currentStock().endMinute < rec.TrdTim1[i].Minutes))
                        {
                            continue;
                        }
                        /*
                        WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                            rec.TrdDate[i].ToShortDateString(),
                            rec.TrdTim1[i],
                            rec.AcVol1[i],
                            rec.High1[i],
                            rec.Low1[i],
                            rec.OpenPrc[i]);
                            */
                        //Calculate volume change over last 3 minutes and add to stocks list
                        if (i > Variables.barLookBack)
                        {
                            // Updated on 11/15/2016 for john's vol3 calculation
                            Variables.currentStock().addVolumeToList(calculateVolumeChange(rec, i) - Variables.currentStock().averageVolumeHistory);
                        }
                    }
                    Variables.currentStock().setInitialized();
                    Variables.currentStock().printTopTradeVol();
                    //3 second delay was needed otherwise we were getting no data returned everyonce in a while
                    System.Threading.Thread.Sleep(3000);
                }
                else 
                {
                    int i = rec.Count - 1;
                    // Added time code on 12/17/2016.  This portion does not trade or use setup time outside of given hours
                    if ((Variables.currentStock().startHour > rec.TrdTim1[i].Hours) || (Variables.currentStock().startHour == rec.TrdTim1[i].Hours && Variables.currentStock().startMinute > rec.TrdTim1[i].Minutes))
                    {
                        continue;
                    }
                    else if ((Variables.currentStock().endHour < rec.TrdTim1[i].Hours) || (Variables.currentStock().endHour == rec.TrdTim1[i].Hours && Variables.currentStock().endMinute < rec.TrdTim1[i].Minutes))
                    {
                        continue;
                    }
           
                    WriteLine("Record count: {0}", i);
                    WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                        rec.TrdDate[i].ToShortDateString(),
                        rec.TrdTim1[i],
                        rec.AcVol1[i],
                        rec.High1[i],
                        rec.Low1[i],
                        rec.OpenPrc[i]);

                    if (i > Variables.barLookBack)
                    {
                        // Updated on 11/15/2016 for john's vol3 calculation
                        int totalVolumeChange = calculateVolumeChange(rec, i) - Variables.currentStock().averageVolumeHistory;

                        Variables.currentStock().writeToCSV("VOLUMECHANGELAST3MIN", totalVolumeChange, Variables.currentStock().getTradeVol(), DateTime.Now);
                        WriteLine("Current: {0} at {1} volume change over 3 minutes, buy is {2}", Variables.currentStock().name, totalVolumeChange, Variables.currentStock().getTradeVol());
                        if (Variables.currentStock().getTradeVol() < totalVolumeChange)
                        {
                            //This sets the current stock to a buy status which will be executed above
                            //because I was unsure of the best way to pass app to the purchase method.
                            if (Variables.currentStock().setBuy())
                            {
                                //Buy!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                                WriteLine("Purchase: {0} at {1} volume change!!!!", Variables.currentStock().name, totalVolumeChange);
                            }
                            else
                            {
                                WriteLine("No Purchase because stock is trending down: {0} at {1} volume change!!!!", Variables.currentStock().name, totalVolumeChange);
                            }
                        }
                        // Add this volume to list if in the top trade volumes
                        Variables.currentStock().addVolumeToList(totalVolumeChange);
                    }
                }
            }
            _evtGotData.Set();
        }

        /*************************************************************************************************/
        /* End intra day bar data                                                                        */
        /*************************************************************************************************/
        /*************************************************************************************************/
        /* Start IntraBar Simulation code                                                                */
        /*************************************************************************************************/
        static void simulation_OnIntraday(object sender, DataEventArgs<IntradayRecord> e)
        {
            WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            double amountPurchased = 0;
            double amountSold = 0;
            int minutesHeld = 0;
            int numberOfBuys = 0;

            foreach (IntradayRecord rec in e)
            {
                // Variables added to track stocks for simulation
                int stocksPurchased = 0;
                double highPrice = 0;
                DateTime purchaseTime = new DateTime();
                WriteLine("DATE\t\tTIME\t\tACVOL\tHIGH\tLOW\tOPEN");
                // Skip this evaluation if rec.Count == 0 no data was returned
                if (rec.Count == 0)
                {
                    continue;
                }
                if (!Variables.currentStock().isInitialized())
                {
                    //Calculate average volume for the look back time
                    // Added on 11/15/2016 for john's vol3 calculation
                    int tempVolumeCount = 0;
                    int tempVolumeNumber = 0;
                    //5 is for days of the week
                    //8 is for hours of the day
                    //60 is for minutes per hour
                    for (int i = 0; i < Variables.currentStock().weeksLookBack*5*8*60; i++)
                    {
                        if ((Variables.currentStock().startHour > rec.TrdTim1[i].Hours) || (Variables.currentStock().startHour == rec.TrdTim1[i].Hours && Variables.currentStock().startMinute > rec.TrdTim1[i].Minutes))
                        {
                            //continue;
                        } else if((Variables.currentStock().endHour < rec.TrdTim1[i].Hours) || (Variables.currentStock().endHour == rec.TrdTim1[i].Hours && Variables.currentStock().endMinute < rec.TrdTim1[i].Minutes))
                        {
                            //continue;
                        } else
                        {
                            /*
                            WriteLine("INIT: {0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                                rec.TrdDate[i].ToShortDateString(),
                                rec.TrdTim1[i],
                                rec.AcVol1[i],
                                rec.High1[i],
                                rec.Low1[i],
                                rec.OpenPrc[i]);
                                */
                            tempVolumeCount += rec.AcVol1[i];
                            tempVolumeNumber++;
                        }
                    }
                    Variables.currentStock().averageVolumeHistory = tempVolumeCount / tempVolumeNumber;

                    //Calculate the top differences over the look back time
                    //5 is for days of the week
                    //8 is for hours of the day
                    //60 is for minutes per hour
                    for (int i = 0; i < Variables.currentStock().weeksLookBack * 5 * 8 * 60; i++)
                    {
                        if ((Variables.currentStock().startHour > rec.TrdTim1[i].Hours) || (Variables.currentStock().startHour == rec.TrdTim1[i].Hours && Variables.currentStock().startMinute > rec.TrdTim1[i].Minutes))
                        {
                            //continue;
                        } else if ((Variables.currentStock().endHour < rec.TrdTim1[i].Hours) || (Variables.currentStock().endHour == rec.TrdTim1[i].Hours && Variables.currentStock().endMinute < rec.TrdTim1[i].Minutes))
                        {
                            //continue;
                        } else
                        {
                            //Calculate volume change over last 3 minutes and add to stocks list
                            if (i > Variables.barLookBack)
                            {
                                // Updated on 11/15/2016 for john's vol3 calculation
                                Variables.currentStock().addVolumeToList(calculateVolumeChange(rec, i) - Variables.currentStock().averageVolumeHistory);
                            }
                        }
                    }
                    Variables.currentStock().setInitialized();
                    Variables.currentStock().printTopTradeVol();
                }
                // Run through simulation
                for (int i = Variables.currentStock().weeksLookBack * 5 * 8 * 60; i < rec.Count; i++)
                {
                    if(stocksPurchased > 0)
                    {
                        minutesHeld++;
                    }
                    if ((Variables.currentStock().startHour > rec.TrdTim1[i].Hours) || (Variables.currentStock().startHour == rec.TrdTim1[i].Hours && Variables.currentStock().startMinute > rec.TrdTim1[i].Minutes))
                    {
                        continue;
                    }
                    else if ((Variables.currentStock().endHour < rec.TrdTim1[i].Hours) || (Variables.currentStock().endHour == rec.TrdTim1[i].Hours && Variables.currentStock().endMinute < rec.TrdTim1[i].Minutes))
                    {
                        continue;
                    }

                    //int i = rec.Count - 1;  Removed for i in for loop because of simulation
                    DateTime tempDate = new DateTime(rec.TrdDate[i].Year, rec.TrdDate[i].Month, rec.TrdDate[i].Day, rec.TrdTim1[i].Hours, rec.TrdTim1[i].Minutes, rec.TrdTim1[i].Seconds);
                   /* WriteLine("Record count: {0}", i);
                    WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                        rec.TrdDate[i].ToShortDateString(),
                        rec.TrdTim1[i],
                        rec.AcVol1[i],
                        rec.High1[i],
                        rec.Low1[i],
                        rec.OpenPrc[i]);*/
                    if (i > Variables.barLookBack)
                    {
                        // Updated on 11/15/2016 for john's vol3 calculation
                        int totalVolumeChange = calculateVolumeChange(rec, i) - Variables.currentStock().averageVolumeHistory;
                        Variables.currentStock().addTrade(new Trade(tempDate, 100, Convert.ToDouble(rec.OpenPrc[i].DecimalValue)));
                        Variables.currentStock().writeToCSV("VOLUMECHANGELAST3MIN", totalVolumeChange, Variables.currentStock().getTradeVol(), tempDate);
                        //WriteLine("Current: {0} at {1} volume change over 3 minutes, buy is {2}", Variables.currentStock().name, totalVolumeChange, Variables.currentStock().getTradeVol());
                        if (Variables.currentStock().getTradeVol() < totalVolumeChange && stocksPurchased == 0)
                        {
                            if (rec.OpenPrc[i].DecimalValue <= rec.OpenPrc[i - 1].DecimalValue)
                            {
                                Variables.currentStock().writeToCSV("NOBUYTRENDINGDOWN", Variables.currentStock().getVolumesToTrade(), Convert.ToDouble(rec.OpenPrc[i].DecimalValue), tempDate);
                            }
                            else
                            {
                                //We are buying this stock
                                numberOfBuys++;
                                stocksPurchased = Variables.currentStock().getVolumesToTrade();
                                highPrice = Convert.ToDouble(rec.OpenPrc[i].DecimalValue);
                                purchaseTime = tempDate;
                                amountPurchased += Variables.currentStock().getVolumesToTrade() * Convert.ToDouble(rec.OpenPrc[i].DecimalValue);
                                Variables.currentStock().writeToCSV("BUY", Variables.currentStock().getVolumesToTrade(), Convert.ToDouble(rec.OpenPrc[i].DecimalValue), tempDate);
                            }
                        }
                        if(stocksPurchased > 0)
                        {
                            if(highPrice < Convert.ToDouble(rec.OpenPrc[i].DecimalValue))
                            {
                                highPrice = Convert.ToDouble(rec.OpenPrc[i].DecimalValue);
                            } else if((tempDate - purchaseTime).TotalSeconds > Variables.currentStock().maxSecondsToHold)
                            {
                                amountSold += stocksPurchased * Convert.ToDouble(rec.OpenPrc[i].DecimalValue);
                                Variables.currentStock().writeToCSV("SELLMAXSECONDSREACHED", Variables.currentStock().getVolumesToTrade(), Convert.ToDouble(rec.OpenPrc[i].DecimalValue), tempDate);
                                stocksPurchased = 0;
                            }
                            else
                            {
                                double stopGap = 0;
                                if ((tempDate - purchaseTime).TotalSeconds > Variables.currentStock().secondsStopGap1)
                                {
                                    stopGap = highPrice - (highPrice * Variables.currentStock().stopGap2);
                                }
                                else
                                {
                                    stopGap = highPrice - (highPrice * Variables.currentStock().stopGap1);
                                }
                                if(stopGap > Convert.ToDouble(rec.OpenPrc[i].DecimalValue))
                                {
                                    amountSold += Variables.currentStock().getVolumesToTrade() * Convert.ToDouble(rec.OpenPrc[i].DecimalValue);
                                    stocksPurchased = 0;
                                    Variables.currentStock().writeToCSV("SELLSTOPGAP", Variables.currentStock().getVolumesToTrade(), Convert.ToDouble(rec.OpenPrc[i].DecimalValue), tempDate);
                                }
                            }
                        }
                        // Add this volume to list if in the top trade volumes
                        Variables.currentStock().addVolumeToList(totalVolumeChange);
                    }
                }
            }
            String outputMessageBox = Variables.currentStock().name + "\n";
            Variables.currentStock().writeToCSV("AMOUNTPURCHASED", 0, amountPurchased, DateTime.Now);
            WriteLine("Amount Purchased: " + amountPurchased);
            outputMessageBox += "Amount Purchased: " + amountPurchased + "\n";
            Variables.currentStock().writeToCSV("AMOUNTSOLD", 0, amountSold, DateTime.Now);
            WriteLine("Amount Sold: " + amountSold);
            outputMessageBox += "Amount Sold: " + amountSold + "\n";
            Variables.currentStock().writeToCSV("NUMBEROFBUYS", 0, numberOfBuys, DateTime.Now);
            WriteLine("Number of Buys: " + numberOfBuys);
            outputMessageBox += "Number of Buys: " + numberOfBuys + "\n";
            Variables.currentStock().writeToCSV("AVERAGEHOLDTIME", 0, minutesHeld / numberOfBuys, DateTime.Now);
            WriteLine("Average Hold Time: " + minutesHeld / numberOfBuys);
            outputMessageBox += "Average Hold Time: " + minutesHeld / numberOfBuys + "\n";
            Variables.currentStock().printToCSV();
            _evtGotData.Set();
            System.Windows.Forms.MessageBox.Show(outputMessageBox);
        }
        /*********************************************************************/
        /*  End IntraBar simulation                                          */
        /*********************************************************************/
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
                WriteLine("=====================================================");
                WriteLine("Type: {0} Status: {1}", ord.Type, ord.CurrentStatus);
                WriteLine("=====================================================");
                if (ord.Type == "UserSubmitOrder")
                {
                    if (ord.CurrentStatus == "COMPLETED" || ord.CurrentStatus == "DELETED")
                    {
                        _state = State.OrderDone;
                    }
                    if (ord.CurrentStatus == "DELETED")
                    {
                        WriteLine("Purchase failed user submit deleted for {0}", ord.DispName);
                        writeToFile("Purchase failed user submit deleted for " + ord.DispName + " at " + DateTime.Now.ToString());
                        Variables.stocks[ord.DispName].soldStock();
                    }
                }

                if (ord.Type == "ExchangeTradeOrder")
                {
                    //Trade successful 
                    try
                    {
                        Variables.stocks[ord.DispName].tradeComplete(ord.Buyorsell, Convert.ToInt32(ord.Volume.ToString()), Convert.ToDouble(ord.Price.ToString()));
                        Variables.stocks[ord.DispName].highPrice = Convert.ToDouble(ord.Price.ToString());
                        Variables.stocks[ord.DispName].writeToFile("Trade at " + Convert.ToDouble(ord.Price.ToString()));
                    }
                    catch (FormatException)
                    {
                        writeToFile("Unable to convert to double selling stock");
                        WriteLine("Unable to convert to double selling stock {0}", ord.DispName);
                        Variables.stocks[ord.DispName].setSell();
                    }
                    WriteLine("GOT FILL FOR {0} {1} AT {2} for {3}", ord.Buyorsell, ord.Volume, ord.Price, ord.DispName);
                    writeToFile(ord.Buyorsell + " complete for: " + ord.DispName + " for " + ord.Volume + " at $" + ord.Price + " " + DateTime.Now.ToString());
                }
                if (ord.Type == "ExchangeKillOrder")
                {
                    WriteLine("GOT KILL");
                    writeToFile("Transaction failed transaction killed " + ord.DispName + " at " + DateTime.Now.ToString());
                    if(ord.Buyorsell == "SELL")
                    {
                        //Sell failed try to sell again
                        Variables.stocks[ord.DispName].setSell();
                    } else
                    {
                        //Purchase failed
                        Variables.stocks[ord.DispName].soldStock();
                    }
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
            //bld.SetAccount(null, "TEST", null, null);
            bld.SetAccount("LATEST", "TEST", "01", "CATALYST");
            //This will determine if we are selling or buying stock
            if (Variables.currentStock().shouldBuy())
            {
                bld.SetBuySell(OrderBuilder.BuySell.BUY);
                Variables.currentStock().buySent();
                bld.SetVolume(Variables.currentStock().getVolumesToTrade());
            }
            else
            {
                bld.SetBuySell(OrderBuilder.BuySell.SELL);
                Variables.currentStock().sellSent();
                Variables.currentStock().soldStock();
                bld.SetVolume(Variables.currentStock().getVolumesPurchased());
            }
            bld.SetExpiration(OrderBuilder.Expiration.DAY);
            bld.SetRoute(Variables.route);
            bld.SetSymbol(Variables.currentStock().name, Variables.currentStock().exchange, OrderBuilder.SecurityType.STOCK);
            bld.SetPriceMarket();
            cache.SubmitOrder(bld);

            _state = State.OrderInPlay;
            _event.Set();
        }
        /*************************************************************************************************/
        /* End Buy/Sell portion                                                                        */
        /*************************************************************************************************/
    }
}
