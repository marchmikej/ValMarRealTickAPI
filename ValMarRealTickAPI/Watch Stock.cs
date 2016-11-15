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

namespace ValMarRealTickAPI
{
    public partial class Watch_Stock : Form
    {
        ToolkitApp _app;
        public Watch_Stock(ToolkitApp app)
        {
            InitializeComponent();
            _app = app;
        }

        private void Watch_Stock_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textSymbol.Text == "")
            {
                MessageBox.Show("Please enter a ticker symbol");
                return;
            } else if (textWeeksLookBack.Text == "")
            {
                MessageBox.Show("Please enter proper weeks to look back");
                return;
            } else if (textStockExchange.Text == "")
            {
                MessageBox.Show("Please enter a stock exchange");
                return;
            }
            try
            {
                Variables.stocks.Add(textSymbol.Text, new Stock(textSymbol.Text, textStockExchange.Text, Convert.ToInt32(textVolumesToPurchase.Text), Convert.ToInt32(textTradesPerWeek.Text), Convert.ToInt32(textWeeksLookBack.Text), Convert.ToInt32(textMaxSecondsToHold.Text), Convert.ToDouble(textStopGap.Text), Convert.ToInt32(textRecentTradesForPrice.Text)));
            }
            catch (FormatException)
            {
                MessageBox.Show("Form not properly filled out.");
                return;
            }
            textSymbol.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(Variables.runTrades)
            {
                Variables.runTrades = false;
                button2.Text = "Start Trades";
            } else
            {
                Variables.runTrades = true;
                button2.Text = "Stop Trades";
            }
        }
    }
}
