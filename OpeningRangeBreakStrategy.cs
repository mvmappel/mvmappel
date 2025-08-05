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
	public class OpeningRangeBreakStrategy : Strategy
	{
		
		private double ORHigh;
		private double ORLow;
		private string atmStrategyId 		= string.Empty;
		private string orderIdB		 		= string.Empty;
		private string orderIdS		 		= string.Empty;
		private bool   isAtmStrategyCreated	= false;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"A strategy that trades off the opening range high and low";
				Name										= "OpeningRangeBreakStrategy";
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
				IsAutoScale									= false;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				OrStartTime						= DateTime.Parse("09:30", System.Globalization.CultureInfo.InvariantCulture);
				OrEndTime						= DateTime.Parse("10:00", System.Globalization.CultureInfo.InvariantCulture);
				AddPlot(Brushes.Orange, "OrHighPlot");
				AddPlot(Brushes.Orange, "OrLowPlot");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom strategy logic here.
			if (CurrentBar < 20) return;
			
			if (State != State.Realtime) return;
			
			if (Times[0][0].TimeOfDay == OrStartTime.TimeOfDay) {
				ORLow = Low[0];
				ORHigh = High[0];
			}
			
			if (Times[0][0].TimeOfDay > OrStartTime.TimeOfDay && Times[0][0].TimeOfDay < OrEndTime.TimeOfDay) {
				if (Low[0] < ORLow)
					ORLow = Low[0];
				
				if (High[0] > ORHigh)
					ORHigh = High[0];
			}
			
			OrHighPlot[0] = ORHigh;
			OrLowPlot[0] = ORLow;
			
			if (Times[0][0].TimeOfDay > OrEndTime.TimeOfDay && atmStrategyId.Length == 0 && CrossAbove(Close, ORHigh, 1)) {
				
				isAtmStrategyCreated = false;
				atmStrategyId = GetAtmStrategyUniqueId();
				orderIdB = GetAtmStrategyUniqueId();
				
				double buyPrice = 0.0;
				buyPrice = GetCurrentAsk() + 4 * TickSize;
				
				AtmStrategyCreate(OrderAction.Buy, OrderType.Market, 0, buyPrice, TimeInForce.Day, orderIdB, "courseAtmStrategy", atmStrategyId, (atmCallbackErrorCode, atmCallBackId) => {
					if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyId)
						isAtmStrategyCreated = true;
				});
			}
			
			if (Times[0][0].TimeOfDay > OrEndTime.TimeOfDay && atmStrategyId.Length == 0 && CrossBelow(Close, ORLow, 1)) {
				
				isAtmStrategyCreated = false;
				atmStrategyId = GetAtmStrategyUniqueId();
				orderIdS = GetAtmStrategyUniqueId();
				
				double sellPrice = 0.0;
				sellPrice = GetCurrentBid() - 4 * TickSize;
				
				AtmStrategyCreate(OrderAction.Sell, OrderType.Market, 0, sellPrice, TimeInForce.Day, orderIdS, "courseAtmStrategy", atmStrategyId, (atmCallbackErrorCode, atmCallBackId) => {
					if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyId)
						isAtmStrategyCreated = true;
				});
			}
			
			if (!isAtmStrategyCreated)
				return;
			
			if (orderIdB.Length > 0) {
				string[] status = GetAtmStrategyEntryOrderStatus(orderIdB);
				
				if (status.GetLength(0) > 0) {
					if (status[2] == "Filled" || status[2] == "Cancelled" || status[2] == "Rejected")
						orderIdB = string.Empty;
				}
			} else if (orderIdS.Length > 0) {
				string[] status = GetAtmStrategyEntryOrderStatus(orderIdS);
				
				if (status.GetLength(0) > 0) {
					if (status[2] == "Filled" || status[2] == "Cancelled" || status[2] == "Rejected")
						orderIdS = string.Empty;
				}
			} else if (atmStrategyId.Length > 0 && GetAtmStrategyMarketPosition(atmStrategyId) == Cbi.MarketPosition.Flat)
				atmStrategyId = string.Empty;
		}

		#region Properties
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="OrStartTime", Order=1, GroupName="Parameters")]
		public DateTime OrStartTime
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="OrEndTime", Order=2, GroupName="Parameters")]
		public DateTime OrEndTime
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> OrHighPlot
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> OrLowPlot
		{
			get { return Values[1]; }
		}
		#endregion

	}
}
