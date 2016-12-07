namespace ValMarRealTickAPI
{
    partial class Watch_Stock
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textSymbol = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textWeeksLookBack = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textTradesPerWeek = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textRecentTradesForPrice = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textStopGap1 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textVolumesToPurchase = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.textStockExchange = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textMaxSecondsToHold = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.textTrendDownSeconds = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.textWaitAfterSellSeconds = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.textStopGap2 = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.textSecondsStopGap1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textSymbol
            // 
            this.textSymbol.Location = new System.Drawing.Point(248, 43);
            this.textSymbol.Name = "textSymbol";
            this.textSymbol.Size = new System.Drawing.Size(198, 26);
            this.textSymbol.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(38, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Symbol";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(38, 120);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(137, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Weeks Look Back";
            // 
            // textWeeksLookBack
            // 
            this.textWeeksLookBack.Location = new System.Drawing.Point(248, 114);
            this.textWeeksLookBack.Name = "textWeeksLookBack";
            this.textWeeksLookBack.Size = new System.Drawing.Size(198, 26);
            this.textWeeksLookBack.TabIndex = 3;
            this.textWeeksLookBack.Text = "3";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(38, 197);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(131, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "Trades Per Week";
            // 
            // textTradesPerWeek
            // 
            this.textTradesPerWeek.Location = new System.Drawing.Point(248, 191);
            this.textTradesPerWeek.Name = "textTradesPerWeek";
            this.textTradesPerWeek.Size = new System.Drawing.Size(198, 26);
            this.textTradesPerWeek.TabIndex = 5;
            this.textTradesPerWeek.Text = "20";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(38, 280);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(176, 20);
            this.label4.TabIndex = 6;
            this.label4.Text = "Recent Trades for Price";
            // 
            // textRecentTradesForPrice
            // 
            this.textRecentTradesForPrice.Location = new System.Drawing.Point(248, 274);
            this.textRecentTradesForPrice.Name = "textRecentTradesForPrice";
            this.textRecentTradesForPrice.Size = new System.Drawing.Size(198, 26);
            this.textRecentTradesForPrice.TabIndex = 7;
            this.textRecentTradesForPrice.Text = "50";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(553, 280);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 20);
            this.label5.TabIndex = 8;
            this.label5.Text = "Stop Gap 1";
            // 
            // textStopGap1
            // 
            this.textStopGap1.Location = new System.Drawing.Point(734, 274);
            this.textStopGap1.Name = "textStopGap1";
            this.textStopGap1.Size = new System.Drawing.Size(216, 26);
            this.textStopGap1.TabIndex = 9;
            this.textStopGap1.Text = ".0004";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(42, 461);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(152, 20);
            this.label6.TabIndex = 10;
            this.label6.Text = "Volume to Purchase";
            // 
            // textVolumesToPurchase
            // 
            this.textVolumesToPurchase.Location = new System.Drawing.Point(248, 454);
            this.textVolumesToPurchase.Name = "textVolumesToPurchase";
            this.textVolumesToPurchase.Size = new System.Drawing.Size(198, 26);
            this.textVolumesToPurchase.TabIndex = 11;
            this.textVolumesToPurchase.Text = "100";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(46, 597);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(168, 53);
            this.button1.TabIndex = 12;
            this.button1.Text = "Watch Symbol";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(553, 49);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(125, 20);
            this.label7.TabIndex = 13;
            this.label7.Text = "Stock Exchange";
            // 
            // textStockExchange
            // 
            this.textStockExchange.Location = new System.Drawing.Point(734, 43);
            this.textStockExchange.Name = "textStockExchange";
            this.textStockExchange.Size = new System.Drawing.Size(216, 26);
            this.textStockExchange.TabIndex = 14;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(46, 527);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(160, 20);
            this.label8.TabIndex = 15;
            this.label8.Text = "Max Seconds to Hold";
            // 
            // textMaxSecondsToHold
            // 
            this.textMaxSecondsToHold.Location = new System.Drawing.Point(248, 520);
            this.textMaxSecondsToHold.Name = "textMaxSecondsToHold";
            this.textMaxSecondsToHold.Size = new System.Drawing.Size(198, 26);
            this.textMaxSecondsToHold.TabIndex = 16;
            this.textMaxSecondsToHold.Text = "1200";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(307, 597);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(139, 53);
            this.button2.TabIndex = 17;
            this.button2.Text = "Start Trades";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(553, 120);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(162, 20);
            this.label9.TabIndex = 18;
            this.label9.Text = "Trend Down Seconds";
            // 
            // textTrendDownSeconds
            // 
            this.textTrendDownSeconds.Location = new System.Drawing.Point(734, 114);
            this.textTrendDownSeconds.Name = "textTrendDownSeconds";
            this.textTrendDownSeconds.Size = new System.Drawing.Size(216, 26);
            this.textTrendDownSeconds.TabIndex = 19;
            this.textTrendDownSeconds.Text = "120";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(553, 197);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(177, 20);
            this.label10.TabIndex = 20;
            this.label10.Text = "Wait After Sell Seconds";
            // 
            // textWaitAfterSellSeconds
            // 
            this.textWaitAfterSellSeconds.Location = new System.Drawing.Point(734, 191);
            this.textWaitAfterSellSeconds.Name = "textWaitAfterSellSeconds";
            this.textWaitAfterSellSeconds.Size = new System.Drawing.Size(216, 26);
            this.textWaitAfterSellSeconds.TabIndex = 21;
            this.textWaitAfterSellSeconds.Text = "120";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(553, 454);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(91, 20);
            this.label11.TabIndex = 22;
            this.label11.Text = "Stop Gap 2";
            // 
            // textStopGap2
            // 
            this.textStopGap2.Location = new System.Drawing.Point(734, 448);
            this.textStopGap2.Name = "textStopGap2";
            this.textStopGap2.Size = new System.Drawing.Size(216, 26);
            this.textStopGap2.TabIndex = 23;
            this.textStopGap2.Text = ".0004";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(553, 362);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(158, 20);
            this.label12.TabIndex = 24;
            this.label12.Text = "Seconds Stop Gap 1";
            // 
            // textSecondsStopGap1
            // 
            this.textSecondsStopGap1.Location = new System.Drawing.Point(734, 356);
            this.textSecondsStopGap1.Name = "textSecondsStopGap1";
            this.textSecondsStopGap1.Size = new System.Drawing.Size(216, 26);
            this.textSecondsStopGap1.TabIndex = 25;
            this.textSecondsStopGap1.Text = "600";
            // 
            // Watch_Stock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1324, 682);
            this.Controls.Add(this.textSecondsStopGap1);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.textStopGap2);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.textWaitAfterSellSeconds);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.textTrendDownSeconds);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.textMaxSecondsToHold);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.textStockExchange);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textVolumesToPurchase);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textStopGap1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textRecentTradesForPrice);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textTradesPerWeek);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textWeeksLookBack);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textSymbol);
            this.Name = "Watch_Stock";
            this.Text = "Watch_Stock";
            this.Load += new System.EventHandler(this.Watch_Stock_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textSymbol;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textWeeksLookBack;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textTradesPerWeek;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textRecentTradesForPrice;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textStopGap1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textVolumesToPurchase;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textStockExchange;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textMaxSecondsToHold;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textTrendDownSeconds;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textWaitAfterSellSeconds;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox textStopGap2;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox textSecondsStopGap1;
    }
}