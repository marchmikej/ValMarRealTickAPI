using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RealTick.Api.Application;
using RealTick.Api.Domain;
using RealTick.Api.Domain.Livequote;
using RealTick.Api.Domain.Intraday;
using System.IO;

namespace ValMarRealTickAPI
{
    public partial class Form1 : Form
    {
        ToolkitApp _app;
        LiveQuoteTable _table;
        int stockIndex;

        public Form1(ToolkitApp app, int stockIndex)
        {
            _app = app;
            this.stockIndex = stockIndex;
            InitializeComponent();
            LiveQuoteForm_Load();
        }

        private void LiveQuoteForm_Load()
        {
            _table = new LiveQuoteTable(_app);
            _table.WantData(_table.TqlForBidAskTrade(Variables.stocks[stockIndex].name, null), true, true);
            _table.OnData += new EventHandler<DataEventArgs<LivequoteRecord>>(_table_OnData);
            _table.OnDead += new EventHandler<EventArgs>(_table_OnDead);
            _table.OnLive += new EventHandler<EventArgs>(_table_OnLive);
            // note that we pass "this" as an argument, so the form received events on its
            // own thread, not whatever worker thread first receives the event
            _table.Start(this);
        }

        void Log(string stFmt, params object[] args)
        {
            string st = DateTime.Now.ToShortTimeString() + ": " + string.Format(stFmt, args);

            listBox1.Items.Add(st);
            while (listBox1.Items.Count >= 23)
                listBox1.Items.RemoveAt(0);
        }

        void _table_OnLive(object sender, EventArgs e)
        {
            Log("CONNECTED OK");
        }

        void _table_OnDead(object sender, EventArgs e)
        {
            Log("CONNECTION FAILED OR LOST");
        }

        void _table_OnData(object sender, DataEventArgs<LivequoteRecord> e)
        {
            foreach (LivequoteRecord rec in e)
            {
                if (rec.Trdprc1 != null)
                {
                    Log("{0} TRADE {1} VOLUME {2} TIME {3}", rec.DispName, rec.Trdprc1, rec.Trdvol1, rec.Trdtim1);
                    Log("{0} TRADE PRICE {1}", rec.DispName, Convert.ToDouble(rec.Trdprc1.ToString()));
                    if (Variables.stocks[stockIndex].stockHeld())
                    {
                        Helper.WriteLine("Form Watching {0} for sell price {1} with purchased vol {2} {3}.", Variables.stocks[stockIndex].name, Variables.stocks[stockIndex].getCurrentStopGapPrice(), Variables.stocks[stockIndex].getVolumesPurchased(), Variables.stocks[stockIndex].stockHeld());
                        try
                        {
                            Variables.stocks[stockIndex].newPrice(Convert.ToDouble(rec.Trdprc1.ToString()));
                        }
                        catch (FormatException)
                        {
                            Helper.WriteLine("In FORM Unable to convert to double selling stock {0}", Variables.stocks[stockIndex].name);
                            Variables.stocks[stockIndex].setSell();
                        }
                    }
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            _table.Dispose();
            _table = null;
            DialogResult = DialogResult.OK;
        }
    }
}
