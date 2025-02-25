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
	public class FastEma : Strategy
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "FastEma";
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
				ShortPeriod					= 8;
				LongPeriod					= 10;
			}
			else if (State == State.Configure)
			{
				EMA ema_short = EMA(ShortPeriod);
				EMA ema_long = EMA(LongPeriod);
				
				ema_short.Plots[0].Brush = Brushes.Green;
				ema_short.Plots[0].Width = 3;
				ema_long.Plots[0].Brush = Brushes.Red;
				ema_short.Plots[0].Width = 3;
				
				AddChartIndicator(ema_short);
				AddChartIndicator(ema_long);
				
				AddChartIndicator(TrilingStopSmooth(10,1.5,TrilingType.Atr));
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < LongPeriod)
				return;
			
	
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				if (CrossAbove(EMA(ShortPeriod),EMA(LongPeriod),1) )
				{
					EnterLong();
					SetStopLoss(CalculationMode.Price, 2*ATR(10)[0]);
				}				
			}
			else
			{
				manageTheTrade();	
			}

		}
		
		private void manageTheTrade()
		{                                     
			double stop_long = TrilingStopSmooth(10,1.5,TrilingType.Atr).TrilingForLong[0];
			double close = Close[0]; 
			
			if (Position.MarketPosition == MarketPosition.Long)
			{
				if (close < stop_long)
					ExitLong();
			}

		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ShortPeriod", Order=1, GroupName="Parameters")]
		public int ShortPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LongPeriod", Order=2, GroupName="Parameters")]
		public int LongPeriod
		{ get; set; }
		#endregion

	}
}
