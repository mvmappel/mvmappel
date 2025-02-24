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
	public class MAsCrossoverStrategy : Strategy
	{
		private SMA smaFast;
		private SMA smaSlow;
		private double PriorTradesAllProfit = 0;
		private double RunningSessionPNL = 0;
		
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
				VolumeMALength					= 20;
				AtrThreshold					= 3.5;
				AdxThreshold					= 25;
				EMALength						= 9;
				Fast							= 7;
				Slow							= 50;
				Qty								= 1;
				Sl								= 200;
				Pt								= 400;
				StartTime						= DateTime.Parse("07:30", System.Globalization.CultureInfo.InvariantCulture);
				EndTime							= DateTime.Parse("14:00", System.Globalization.CultureInfo.InvariantCulture);
				SunOk							= false;
				MonOk                        	= false;
				TuesOk							= true;
				WedOk							= true;
				ThursOk							= true;
				FriOk							= true;
				SatOk							= false;
				LongOnly						= false;
				ShortOnly						= false;
				AddProfitTarget					= false;
				LetItRun						= true;
				DayPNLTarget					= 300;
				PerTradeThreshold			    = 35;
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
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;
			
//			if (Bars.IsFirstBarOfSession) {
//				PriorTradesAllProfit = SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;
//			}
			
			double CurrUnrealizedPNL = 0;
			//CurrUnrealizedPNL = Account.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar) + Account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
			CurrUnrealizedPNL = Account.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar);
//			Print("PNL: " + CurrPNL + "Target: " + DayPNLTarget);
			if (CurrUnrealizedPNL >= PerTradeThreshold) {
				if (Position.MarketPosition == MarketPosition.Long) {
					ExitLong(Convert.ToInt32(Qty));
					EnterLong(Convert.ToInt32(Qty), @"Long");
				}
				
				if (Position.MarketPosition == MarketPosition.Short) {
					ExitShort(Convert.ToInt32(Qty));
					EnterShort(Convert.ToInt32(Qty), @"Short");
				}
				
				RunningSessionPNL += CurrUnrealizedPNL;
				//return;
			}
			
			double CurrRealizedPNL = 0;
			//CurrUnrealizedPNL = Account.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar) + Account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
			CurrRealizedPNL = Account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
//			Print("PNL: " + CurrPNL + "Target: " + DayPNLTarget);
			if (CurrRealizedPNL >= DayPNLTarget) {
				if (Position.MarketPosition == MarketPosition.Long) {
					ExitLong(Convert.ToInt32(Qty));
				}
				
				if (Position.MarketPosition == MarketPosition.Short) {
					ExitShort(Convert.ToInt32(Qty));
				}
				
				return;
			}			
			
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
//							if (noTrades) {
								EnterLong(Convert.ToInt32(Qty), @"Long");
//							} 
//								else {
//								reverseQty = Qty * 2;
//								EnterLong(Convert.ToInt32(reverseQty), @"Long");
//							}
						}
				}
	
				if (CrossBelow(smaFast, smaSlow, 1)) {
						if (!LongOnly) {
//							if (noTrades) {
								EnterShort(Convert.ToInt32(Qty), @"Short");
//							}
//							else {
//								reverseQty = Qty * 2;
//								EnterShort(Convert.ToInt32(reverseQty), @"Short");
//							}
						}
				}
			} else {
				if (v > va && adx > AdxThreshold && CrossAbove(smaFast, smaSlow, 1) && noTrades)
						EnterLong(Convert.ToInt32(Qty), @"Long");
				
				if (v > va && adx > AdxThreshold && CrossBelow(smaFast, smaSlow, 1) && noTrades)
						EnterShort(Convert.ToInt32(Qty), @"Short");			
			}
			
			/*if (v > va && 
				atr > AtrThreshold && 
				adx > AdxThreshold && 
				Close[0] > Open[0] && 
				CrossAbove(smaFast, smaSlow, 1) &&
				noTrades)
					EnterLong(Convert.ToInt32(Qty), @"Long");
			*/
			
			/*if (v > va && 
				atr > AtrThreshold && 
				adx > AdxThreshold && 
				Close[0] < Open[0] && 
				CrossBelow(smaFast, smaSlow, 1) && 
				noTrades)
					EnterShort(Convert.ToInt32(Qty), @"Short");
			*/
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
		[Display(Name="PerTradeThreshold", Order=21, GroupName="Parameters")]
		public double PerTradeThreshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="DayPNLTarget", Order=22, GroupName="Parameters")]
		public double DayPNLTarget
		{ get; set; }		
		#endregion
	}
}
