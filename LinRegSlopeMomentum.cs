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
namespace NinjaTrader.NinjaScript.Strategies.TrendFollowing
{
	public class LinRegSlopeMomentum : Strategy
	{
		int slope_period = 14;
		int std_period = 40;
		double std_mult = 1.0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "LinRegSlopeMomentum";
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
				Qty											= 1;
			}
			else if (State == State.Configure)
			{
				AddChartIndicator(LinRegSlopeBounds(slope_period,std_period,std_mult));
				AddChartIndicator(TrilingStopSmooth(40,1.5,TrilingType.Atr));
				AddChartIndicator(ADX(8));
			}
		} 
		protected override void OnBarUpdate()
		{
			if (CurrentBar < 50) return; 
			
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				searchSignals();
			}
			else 
			{
				manageTheTrade(); 
			}
		}
		
		#region Signal Searching
	
		
		private void searchSignals()
		{
			double slope = LinRegSlopeBounds(slope_period,std_period,std_mult).Slope[0];
			bool bullish_crossover = CrossAbove(LinRegSlopeBounds(slope_period,std_period,std_mult).Slope,LinRegSlopeBounds(slope_period,std_period,std_mult).Upper,1);
			bool bearish_crossover = CrossBelow(LinRegSlopeBounds(slope_period,std_period,std_mult).Slope,LinRegSlopeBounds(slope_period,std_period,std_mult).Lower,1);
			double adx = ADX(8)[0];
			
			if (bullish_crossover && slope > 0 && adx > 25)
			{
				EnterLong(Convert.ToInt32(Qty), @"Long");
			}
//			else if (bearish_crossover && slope < 0 && adx > 25)
//			{
//				EnterShort(Convert.ToInt32(Qty), @"Long");
//			}
		}
		#endregion
		
		#region Trade managmanet
		private void manageTheTrade()
		{                                    
			double stop_long = TrilingStopSmooth(40,1.5,TrilingType.Atr).TrilingForLong[0];
			double stop_short = TrilingStopSmooth(40,1.5,TrilingType.Atr).TrilingForShort[0];
	
			
			double close = Close[0]; 
			
			if (Position.MarketPosition == MarketPosition.Long)
			{
				if (close < stop_long)
					ExitLong();
				
			}
			else if (Position.MarketPosition == MarketPosition.Short)
			{
				if (close > stop_short)
					ExitShort();
			}
		}
		

		#endregion 
		
		#region Properties
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Qty", Order=1, GroupName="Parameters")]
		public int Qty
		{ get; set; }
		
		#endregion		
	}
}
