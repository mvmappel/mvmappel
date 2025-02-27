#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class MAsXoverStrategy : Strategy
	{
		private SMA smaFast;
		private SMA smaSlow;
		private double PriorTradesAllProfit = 0;
		private double RunningSessionPNL = 0;
		
		// Button Management
		private System.Windows.Controls.Grid		buttonsGrid;
		private System.Windows.Controls.Button		buyButton, sellButton, longOnlyButton, shortOnlyButton, longAndShortButton;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Crossover that captures per trade chuncks based on threshold set";
				Name										= "MAsCrossoverStrategy";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				VolumeMALength								= 20;
				AtrThreshold								= 3.5;
				AdxThreshold								= 25;
				EMALength									= 9;
				Fast										= 9;
				Slow										= 26;
				Qty											= 1;
				Sl											= 50;
				Pt											= 50;
				StartTime									= DateTime.Parse("07:30", System.Globalization.CultureInfo.InvariantCulture);
				EndTime										= DateTime.Parse("15:30", System.Globalization.CultureInfo.InvariantCulture);
				SunOk										= false;
				MonOk                        				= true;
				TuesOk										= true;
				WedOk										= true;
				ThursOk										= true;
				FriOk										= true;
				SatOk										= false;
				LongOnly									= false;
				ShortOnly									= false;
				AddProfitTarget								= true;
				LetItRun									= true;
				DayProfitTarget								= 100;
				DayLossTarget								= -150;				
			}
			else if (State == State.Configure)
			{
				if (AddProfitTarget) {
					SetProfitTarget(@"Long", CalculationMode.Currency, Pt);
					SetProfitTarget(@"Short", CalculationMode.Currency, Pt);
				}
				SetStopLoss(@"Long", CalculationMode.Currency, Sl, false);
				SetStopLoss(@"Short", CalculationMode.Currency, Sl, false);
				
				RunningSessionPNL = 0;
			}
			else if (State == State.DataLoaded)
			{
				smaFast = SMA(Fast);
				smaSlow = SMA(Slow);

				smaFast.Plots[0].Brush = Brushes.Goldenrod;
				smaSlow.Plots[0].Brush = Brushes.SeaGreen;

				AddChartIndicator(smaFast);
				AddChartIndicator(smaSlow);
				
				// Button Management
				if (ChartControl != null)
					CreateWPFControls();
			}
			else if (State == State.Terminated) // Button Management
			{
				if (ChartControl != null)
					RemoveWPFControls();
			}
		}
		
		// Button Management
		private void CreateWPFControls()
		{
			// if the script has already added the controls, do not add a second time.
			if (UserControlCollection.Contains(buttonsGrid))
				return;
			
			// when making WPF changes to the UI, run the code on the UI thread of the chart
			ChartControl.Dispatcher.InvokeAsync((() =>
			{
				// this buttonGrid will contain the buttons
				buttonsGrid = new System.Windows.Controls.Grid
				{
					Background			= Brushes.Red,
					Name				= "ButtonsGrid",
					HorizontalAlignment	= HorizontalAlignment.Left,
					VerticalAlignment	= VerticalAlignment.Bottom
				};

				for (int i = 0; i < 3; i++)
					buttonsGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
				
				for (int i = 0; i < 3; i++)
					buttonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());

				buyButton = new System.Windows.Controls.Button
				{
					Name		= "BuyButton",
					Content		= "Buy",
					Foreground	= Brushes.White,
					Background	= Brushes.Green
				};

				buyButton.Click += OnButtonClick;				
				buttonsGrid.Children.Add(buyButton);
				System.Windows.Controls.Grid.SetRow(buyButton, 0);
				System.Windows.Controls.Grid.SetColumn(buyButton, 1);				

				sellButton = new System.Windows.Controls.Button
				{
					Name		= "SellButton",
					Content		= "Sell",
					Foreground	= Brushes.White,
					Background	= Brushes.Red
				};

				sellButton.Click += OnButtonClick;				
				buttonsGrid.Children.Add(sellButton);
				System.Windows.Controls.Grid.SetRow(sellButton, 1);
				System.Windows.Controls.Grid.SetColumn(sellButton, 1);
				
				longOnlyButton = new System.Windows.Controls.Button
				{
					Name		= "LongOnlyButton",
					Content		= "Long Only",
					Foreground	= Brushes.White,
					Background	= Brushes.Gray
				};

				longOnlyButton.Click += OnButtonClick;				
				buttonsGrid.Children.Add(longOnlyButton);
				System.Windows.Controls.Grid.SetRow(longOnlyButton, 0);
				System.Windows.Controls.Grid.SetColumn(longOnlyButton, 0);
				
				shortOnlyButton = new System.Windows.Controls.Button
				{
					Name		= "ShortOnlyButton",
					Content		= "Short Only",
					Foreground	= Brushes.White,
					Background	= Brushes.Gray
				};

				shortOnlyButton.Click += OnButtonClick;				
				buttonsGrid.Children.Add(shortOnlyButton);
				System.Windows.Controls.Grid.SetRow(shortOnlyButton, 1);
				System.Windows.Controls.Grid.SetColumn(shortOnlyButton, 0);
				
				longAndShortButton = new System.Windows.Controls.Button
				{
					Name		= "LongAndShortButton",
					Content		= "Long & Short",
					Foreground	= Brushes.White,
					Background	= Brushes.Gray
				};

				longAndShortButton.Click += OnButtonClick;				
				buttonsGrid.Children.Add(longAndShortButton);
				System.Windows.Controls.Grid.SetRow(longAndShortButton, 3);
				System.Windows.Controls.Grid.SetColumn(longAndShortButton, 0);				

				// add our button grid to the main UserControlCollection over the chart
				UserControlCollection.Add(buttonsGrid);
				
				if (!LongOnly && !ShortOnly) {
					longAndShortButton.IsEnabled = false;
					longAndShortButton.IsEnabled = false;
					longAndShortButton.Content = "Long & Short *";
					longAndShortButton.Background = Brushes.Black;
					longAndShortButton.FontStyle = FontStyles.Italic;
					longAndShortButton.FontWeight = FontWeights.Bold;					
				}
			}));
		}		
		
		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;	
			
			double CurrRealizedPNL = 0;
			CurrRealizedPNL = Account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
			if (CurrRealizedPNL >= DayProfitTarget || CurrRealizedPNL <= DayLossTarget)			
				CloseStrategy("MAsCrossoverStrategy");	
			
			if ((Times[0][0].TimeOfDay >= StartTime.TimeOfDay) && (Times[0][0].TimeOfDay <= EndTime.TimeOfDay)) {
				// Trade away
				//Print("Inside time span: " + Times[0][0].TimeOfDay);
			}
			else {
				//Print("Outside time span - Start time: " + Times[0][0].TimeOfDay);
				return;
			}
		
			if ((Time[0].DayOfWeek == DayOfWeek.Monday) && !MonOk)
				return;
			if ((Time[0].DayOfWeek == DayOfWeek.Tuesday) && !TuesOk)
				return;
			if ((Time[0].DayOfWeek == DayOfWeek.Wednesday) && !WedOk)
				return;
			if ((Time[0].DayOfWeek == DayOfWeek.Thursday) && !ThursOk)
				return;
			if ((Time[0].DayOfWeek == DayOfWeek.Friday) && !FriOk)
				return;
			if ((Time[0].DayOfWeek == DayOfWeek.Saturday) && !SatOk)
				return;
			if ((Time[0].DayOfWeek == DayOfWeek.Sunday) && !SunOk)
				return;			
					
			double v = Volume[0];
			double va = EMA(Volume, VolumeMALength)[0];
			double atr = ATR(14)[0];
			double adx = ADX(14)[0];
			double ema = EMA(Close, EMALength)[0];
			int reverseQty = 0;
			bool noTrades = Position.MarketPosition == MarketPosition.Flat;
			
			if (LetItRun) {
				if (CrossAbove(smaFast, smaSlow, 1)) {
						if (!ShortOnly) {
								EnterLong(Convert.ToInt32(Qty), @"Long");
						}
				}
	
				if (CrossBelow(smaFast, smaSlow, 1)) {
						if (!LongOnly) {
								EnterShort(Convert.ToInt32(Qty), @"Short");
						}
				}
			} else {
				if (v > va && adx > AdxThreshold && CrossAbove(smaFast, smaSlow, 1) && noTrades)
						EnterLong(Convert.ToInt32(Qty), @"Long");
				
				if (v > va && adx > AdxThreshold && CrossBelow(smaFast, smaSlow, 1) && noTrades)
						EnterShort(Convert.ToInt32(Qty), @"Short");			
			}
		}
		
		// Button Management
		private void OnButtonClick(object sender, RoutedEventArgs rea)
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;

			if (button == buyButton)
			{
				EnterLong(Convert.ToInt32(Qty), @"Long");
			}
				
			if (button == sellButton)
			{
				EnterShort(Convert.ToInt32(Qty), @"Short");
			}
			
			if (button == longOnlyButton)
			{
				ShortOnly = false;
				LongOnly = true;
				button.IsEnabled = false;
				button.Content = "Long Only *";
				button.Background = Brushes.Black;
				button.FontStyle = FontStyles.Italic;
				button.FontWeight = FontWeights.Bold;
			}
			
			if (button == shortOnlyButton)
			{
				ShortOnly = true;
				LongOnly = false;
			}
			
			if (button == longAndShortButton)
			{
				ShortOnly = false;
				LongOnly = false;
			}			
		}
		
		// Button Management
		private void RemoveWPFControls()
		{
			// when disabling the script, remove the button click handler methods from the click events
			// set the buttons to null so the garbage collector knows to clean them up and free memory
			ChartControl.Dispatcher.InvokeAsync((() =>
			{
				if (buttonsGrid != null)
				{
					if (buyButton != null)
					{
						buyButton.Click -= OnButtonClick;
						buyButton = null;
					}
					if (sellButton != null)
					{
						sellButton.Click -= OnButtonClick;
						sellButton = null;
					}					
					if (longOnlyButton != null)
					{
						longOnlyButton.Click -= OnButtonClick;
						longOnlyButton = null;
					}					
					if (shortOnlyButton != null)
					{
						shortOnlyButton.Click -= OnButtonClick;
						shortOnlyButton = null;
					}
					if (longAndShortButton != null)
					{
						longAndShortButton.Click -= OnButtonClick;
						longAndShortButton = null;
					}

					UserControlCollection.Remove(buttonsGrid);
				}
			}));
		}		

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="VolumeMALength", Order=1, GroupName="Parameters")]
		public int VolumeMALength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="AtrThreshold", Order=2, GroupName="Parameters")]
		public double AtrThreshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="AdxThreshold", Order=3, GroupName="Parameters")]
		public int AdxThreshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="EMALength", Order=4, GroupName="Parameters")]
		public int EMALength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Qty", Order=5, GroupName="Parameters")]
		public int Qty
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Sl", Order=6, GroupName="Parameters")]
		public int Sl
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Pt", Order=7, GroupName="Parameters")]
		public int Pt
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="StartTime", Order=8, GroupName="Parameters")]
		public DateTime StartTime
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="EndTime", Order=9, GroupName="Parameters")]
		public DateTime EndTime
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="SunOk", Order=10, GroupName="Parameters")]
		public bool SunOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="MonOk", Order=11, GroupName="Parameters")]
		public bool MonOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="TuesOk", Order=12, GroupName="Parameters")]
		public bool TuesOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="WedOk", Order=13, GroupName="Parameters")]
		public bool WedOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ThursOk", Order=14, GroupName="Parameters")]
		public bool ThursOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="FriOk", Order=15, GroupName="Parameters")]
		public bool FriOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="SatOk", Order=16, GroupName="Parameters")]
		public bool SatOk
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="LongOnly", Order=17, GroupName="Parameters")]
		public bool LongOnly
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ShortOnly", Order=18, GroupName="Parameters")]
		public bool ShortOnly
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="AddProfitTarget", Order=19, GroupName="Parameters")]
		public bool AddProfitTarget
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="LetItRun", Order=20, GroupName="Parameters")]
		public bool LetItRun
		{ get; set; }		

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast", GroupName = "NinjaScriptStrategyParameters", Order = 0)]
		public int Fast
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow", GroupName = "NinjaScriptStrategyParameters", Order = 1)]
		public int Slow
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="DayProfitTarget", Order=21, GroupName="Parameters")]
		public double DayProfitTarget
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="DayLossTarget", Order=22, GroupName="Parameters")]
		public double DayLossTarget
		{ get; set; }		
		#endregion
	}
}
