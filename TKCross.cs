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
namespace NinjaTrader.NinjaScript.Strategies.IchimokuSystems
{
	public class TKCross : Strategy
	{
		int tenkanPeriod = 9, kijunPeriod = 26, senkouBPeriod = 52, chikouPeriod = 26;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "TKCross";
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
				Qty											= 1;
				SunOk										= false;
				MonOk                        				= false;
				TuesOk										= false;
				WedOk										= false;
				ThursOk										= false;
				FriOk										= false;
				SatOk										= false;
				DayBoundry									= DateTime.Parse("23:59", System.Globalization.CultureInfo.InvariantCulture);
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.Configure)
			{
				AddChartIndicator(Ichimoku(tenkanPeriod,kijunPeriod,senkouBPeriod,chikouPeriod));
			}
		}

		protected override void OnBarUpdate()
		{
			
			if (CurrentBar < senkouBPeriod * 2) return;
												
			if ((Time[0].DayOfWeek == DayOfWeek.Monday) && !MonOk)
				return;
			if ((Time[0].DayOfWeek == DayOfWeek.Tuesday) && Times[0][0].TimeOfDay <= DayBoundry.TimeOfDay && !TuesOk)
				return;
			if ((Time[0].DayOfWeek == DayOfWeek.Wednesday) && Times[0][0].TimeOfDay <= DayBoundry.TimeOfDay && !WedOk)
				return;
			if ((Time[0].DayOfWeek == DayOfWeek.Thursday) && !ThursOk)
				return;
			if ((Time[0].DayOfWeek == DayOfWeek.Friday) && !FriOk)
				return;
			if ((Time[0].DayOfWeek == DayOfWeek.Saturday) && !SatOk)
				return;
			if ((Time[0].DayOfWeek == DayOfWeek.Sunday) && !SunOk)
				return;	
			
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				searchSignals();
			}
			else
			{
				manageTheTrade(); 
			}
			
		}

		private void searchSignals()
		{
			/// T/K Cross event (bullish event):			
			bool tk_cross_event = CrossAbove(Ichimoku(tenkanPeriod,kijunPeriod,senkouBPeriod,chikouPeriod).Tenkan,Ichimoku(tenkanPeriod,kijunPeriod,senkouBPeriod,chikouPeriod).Kijun,1);
			
			/// The future cloud is green (indicate about continuotion of the bullish trend)
			bool future_kumo_green = Ichimoku(tenkanPeriod,kijunPeriod,senkouBPeriod,chikouPeriod).FutureSenkouA[0] >= Ichimoku(tenkanPeriod,kijunPeriod,senkouBPeriod,chikouPeriod).FutureSenkouB[0];
			
			/// The price above the chikou (indicate about momentum)
			bool chikou_price = Close[0] > Close[26]; 
			
            /// Price Above the Cloud (Indicate about healty bullish trend)
			double senkou_a = Ichimoku(tenkanPeriod,kijunPeriod,senkouBPeriod,chikouPeriod).SenkouA[0];
			double senkou_b = Ichimoku(tenkanPeriod,kijunPeriod,senkouBPeriod,chikouPeriod).SenkouB[0];
			double current_price = Close[0]; 
			bool price_kumo = current_price > senkou_a && current_price > senkou_b;
			
			
			if (price_kumo && tk_cross_event && future_kumo_green && chikou_price)
			{
				/// Enter long
				EnterLong(Convert.ToInt32(Qty), @"Long");
				
				/// Set stop loss
				double kijun = Ichimoku(tenkanPeriod,kijunPeriod,senkouBPeriod,chikouPeriod).Kijun[0]; 
				SetStopLoss(CalculationMode.Price,kijun);
			}
		}
	
		
	
		
		private void manageTheTrade()
		{
			double kijun = Ichimoku(tenkanPeriod,kijunPeriod,senkouBPeriod,chikouPeriod).Kijun[0];
			
			if (Position.MarketPosition == MarketPosition.Long)
			{	
				if (Close[0] < kijun)
					ExitLong();
			}

		}
		
		#region Properties
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Qty", Order=1, GroupName="Parameters")]
		public int Qty
		{ get; set; }
		

		[NinjaScriptProperty]
		[Display(Name="SunOk", Order=2, GroupName="Parameters")]
		public bool SunOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="MonOk", Order=3, GroupName="Parameters")]
		public bool MonOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="TuesOk", Order=4, GroupName="Parameters")]
		public bool TuesOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="WedOk", Order=5, GroupName="Parameters")]
		public bool WedOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ThursOk", Order=6, GroupName="Parameters")]
		public bool ThursOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="FriOk", Order=7, GroupName="Parameters")]
		public bool FriOk
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="SatOk", Order=8, GroupName="Parameters")]
		public bool SatOk
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="DayBoundry", Order=9, GroupName="Parameters")]
		public DateTime DayBoundry
		{ get; set; }
		
		#endregion
	}
}
