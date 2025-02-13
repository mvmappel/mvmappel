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
	public class VolumeAndATRStrategy : Strategy
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Strategy that uses volume and average true range";
				Name										= "VolumeAndATRStrategy";
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
				Qty								= 4;
				Sl								= 16;
				Pt								= 50;
				StartTime						= DateTime.Parse("07:30", System.Globalization.CultureInfo.InvariantCulture);
				EndTime							= DateTime.Parse("14:00", System.Globalization.CultureInfo.InvariantCulture);
				filterTimeofDay1Start			= DateTime.Parse("08:00", System.Globalization.CultureInfo.InvariantCulture);
				filterTimeofDay1End				= DateTime.Parse("08:00", System.Globalization.CultureInfo.InvariantCulture);
				filterTimeofDay2Start			= DateTime.Parse("08:00", System.Globalization.CultureInfo.InvariantCulture);
				filterTimeofDay2End				= DateTime.Parse("08:00", System.Globalization.CultureInfo.InvariantCulture);
				filterTimeofDay3Start			= DateTime.Parse("08:00", System.Globalization.CultureInfo.InvariantCulture);
				filterTimeofDay2End				= DateTime.Parse("08:00", System.Globalization.CultureInfo.InvariantCulture);
				filterTimeofDay4Start			= DateTime.Parse("08:00", System.Globalization.CultureInfo.InvariantCulture);
				filterTimeofDay4End				= DateTime.Parse("08:00", System.Globalization.CultureInfo.InvariantCulture);				
				SunOk							= false;
				MonOk                        	= true;
				TuesOk							= true;
				WedOk							= true;
				ThursOk							= true;
				FriOk							= true;
				SatOk							= false;
				fltrTOD1						= false;
				fltrTOD2						= false;
				fltrTOD3						= false;
				fltrTOD4						= false;
				}
			else if (State == State.Configure)
			{
				SetProfitTarget(@"Long", CalculationMode.Ticks, Pt);
				SetProfitTarget(@"Short", CalculationMode.Ticks, Pt);
				SetStopLoss(@"Long", CalculationMode.Ticks, Sl, false);
				SetStopLoss(@"Short", CalculationMode.Ticks, Sl, false);
			}
		}

		
		// 5 minute chart
		// volume > average volume (SMA length 20)
		// atr > 3.5
		// adx > 25
		// only trade during eastern standard time market hours

		// For longs close > open and close > 9 EMA
		// For shorts open > close and the close < 9EMA

		// stop loss 16 ticks
		// profit target 16 ticks
		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;
			
			// Filtering out different time of day periods
			if ((Times[0][0].TimeOfDay < StartTime.TimeOfDay)
				 || (Times[0][0].TimeOfDay > EndTime.TimeOfDay))
				return;
			
//			if (fltrTOD1) {
//				if ((Times[0][0].TimeOfDay > filterTimeofDay1Start.TimeOfDay)
//					 && (Times[0][0].TimeOfDay < filterTimeofDay1End.TimeOfDay))
//					return;
//			}
			
//			if (fltrTOD2) {
//				if ((Times[0][0].TimeOfDay > filterTimeofDay2Start.TimeOfDay)
//					 && (Times[0][0].TimeOfDay < filterTimeofDay2End.TimeOfDay))
//					return;
//			}
			
//			if (fltrTOD3) {
//				if ((Times[0][0].TimeOfDay > filterTimeofDay3Start.TimeOfDay)
//					 && (Times[0][0].TimeOfDay < filterTimeofDay3End.TimeOfDay))
//					return;
//			}
			
//			if (fltrTOD4) {
//				if ((Times[0][0].TimeOfDay > filterTimeofDay4Start.TimeOfDay)
//					 && (Times[0][0].TimeOfDay < filterTimeofDay4End.TimeOfDay))
//					return;
//			}
			
			bool TradeDayOk = true;
			switch (Time[0].DayOfWeek) {
				case DayOfWeek.Sunday:
					if (!SunOk) {TradeDayOk = false;}
					break;
				case DayOfWeek.Monday:
					if (!MonOk) {TradeDayOk = false;}
					break;
				case DayOfWeek.Tuesday:
					if (!TuesOk) {TradeDayOk = false;}
					break;
				case DayOfWeek.Wednesday:
					if (!WedOk) {TradeDayOk = false;}
					break;
				case DayOfWeek.Thursday:
					if (!ThursOk) {TradeDayOk = false;}
					break;
				case DayOfWeek.Friday:
					if (!FriOk) {TradeDayOk = false;}
					break;
				case DayOfWeek.Saturday:
					if (!SatOk) {TradeDayOk = false;}
					break;									
			}			
			
			if (!TradeDayOk) return;
			
			double v = Volume[0];
			double va = SMA(Volume, VolumeMALength)[0];
			double atr = ATR(14)[0];
			double adx = ADX(14)[0];
			double ema = EMA(Close, EMALength)[0];
			bool noTrades = Position.MarketPosition == MarketPosition.Flat;
			
			if (v > va && atr > AtrThreshold && adx > AdxThreshold && Close[0] > Open[0] && Close[0] > ema && noTrades)
				EnterLong(Convert.ToInt32(Qty), @"Long");
			
			if (v > va && atr > AtrThreshold && adx > AdxThreshold && Close[0] < Open[0] && Close[0] < ema && noTrades)
				EnterShort(Convert.ToInt32(Qty), @"Short");
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
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="filterTimeofDay1Start", Order=10, GroupName="Parameters")]
		public DateTime filterTimeofDay1Start
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="filterTimeofDay1End", Order=11, GroupName="Parameters")]
		public DateTime filterTimeofDay1End
		{ get; set; }		

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="filterTimeofDay2Start", Order=12, GroupName="Parameters")]
		public DateTime filterTimeofDay2Start
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="filterTimeofDay2End", Order=13, GroupName="Parameters")]
		public DateTime filterTimeofDay2End
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="filterTimeofDay3Start", Order=14, GroupName="Parameters")]
		public DateTime filterTimeofDay3Start
		{ get; set; }		
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="filterTimeofDay3End", Order=15, GroupName="Parameters")]
		public DateTime filterTimeofDay3End
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="filterTimeofDay4Start", Order=16, GroupName="Parameters")]
		public DateTime filterTimeofDay4Start
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="filterTimeofDay4End", Order=17, GroupName="Parameters")]
		public DateTime filterTimeofDay4End
		{ get; set; }			
		
		[NinjaScriptProperty]
		[Display(Name="SunOk", Order=18, GroupName="Parameters")]
		public bool SunOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="MonOk", Order=19, GroupName="Parameters")]
		public bool MonOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="TuesOk", Order=20, GroupName="Parameters")]
		public bool TuesOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="WedOk", Order=21, GroupName="Parameters")]
		public bool WedOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ThursOk", Order=22, GroupName="Parameters")]
		public bool ThursOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="FriOk", Order=23, GroupName="Parameters")]
		public bool FriOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="SatOk", Order=24, GroupName="Parameters")]
		public bool SatOk
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="fltrTOD1", Order=25, GroupName="Parameters")]
		public bool fltrTOD1
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="fltrTOD2", Order=26, GroupName="Parameters")]
		public bool fltrTOD2
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="fltrTOD3", Order=27, GroupName="Parameters")]
		public bool fltrTOD3
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="fltrTOD4", Order=28, GroupName="Parameters")]
		public bool fltrTOD4
		{ get; set; }		
		#endregion

	}
}
