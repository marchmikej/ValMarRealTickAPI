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
            } else if (textTrendDownSeconds.Text == "")
            {
                MessageBox.Show("Please enter stock trend down in seconds");
                return;
            } else if (textWaitAfterSellSeconds.Text == "")
            {
                MessageBox.Show("Please enter seconds to wait after sell");
                return;
            }
            else if (textDollarAmountToPurchase.Text == "")
            {
                MessageBox.Show("Please enter dollar amount to purchase");
                return;
            }
            else if (textRoute.Text == "")
            {
                MessageBox.Show("Please enter route");
                return;
            }
            try
            {
                if (!Variables.stocks.ContainsKey(textSymbol.Text))
                {
                    Helper.WriteLine("Added Stock: " + textSymbol.Text);
                    Variables.stocks.Add(textSymbol.Text, new Stock(textSymbol.Text, textStockExchange.Text, Convert.ToInt32(textTradesPerWeek.Text), Convert.ToInt32(textWeeksLookBack.Text), Convert.ToInt32(textMaxSecondsToHold.Text), Convert.ToDouble(textStopGap1.Text), Convert.ToDouble(textStopGap2.Text), Convert.ToInt32(textSecondsStopGap1.Text), Convert.ToInt32(textRecentTradesForPrice.Text), Convert.ToInt32(textTrendDownSeconds.Text), Convert.ToInt32(textWaitAfterSellSeconds.Text), Convert.ToInt32(textStartHour.Text), Convert.ToInt32(textStartMinute.Text), Convert.ToInt32(textEndHour.Text), Convert.ToInt32(textEndMinute.Text), Convert.ToInt32(textDollarAmountToPurchase.Text), textRoute.Text, checkPurchasePriceSell.Checked));
                    if(checkDemo.Checked)
                    {
                        Variables.isDemo = true;
                    }
                } else
                {
                    Helper.WriteLine("Stock has already been added no duplicates allowed: " + textSymbol.Text);
                }
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
            buttonRunSim.Enabled = false;  //Make simulation button unusable
            if (Variables.runTrades)
            {
                Variables.runTrades = false;
                button2.Text = "Start Trades";
            } else
            {
                Variables.runTrades = true;
                button2.Text = "Stop Trades";
            }
        }

        private void buttonRunSim_Click(object sender, EventArgs e)
        {
            Variables.runSimulation = true;
            button2.Enabled = false;
            Variables.runTrades = true;
            Variables.simulationDays = Convert.ToInt32(textSimDays.Text);
        }
    }
}
