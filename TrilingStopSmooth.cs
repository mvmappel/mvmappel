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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class TrilingStopSmooth : Indicator
	{
		double current_level = 0;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Can be used for trade managmant";
				Name										= "TrilingStopSmooth";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Period					= 14;
				Multiplier					= 1;
				Type						= TrilingType.Atr;
				AddPlot(Brushes.Green, "Triling For Long");
				AddPlot(Brushes.Red, "Triling For Short");

			}
			else if (State == State.Configure)
			{
				Plots[0].Width = 2;
				Plots[1].Width = 2;
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < Period) 
				return;
			
			/// Store the current value of the ATR / STD
			double unit = Type == TrilingType.Atr ? ATR(Period)[0] : StdDev(Period)[0];
			
			/// Check if is a green candle
			bool is_green_candle = Close[0] > Open[0];
			
			
			if (is_green_candle == false)
			{
				TrilingForLong[0] = TrilingForLong[1]; /// The Triling line for long stay the same
				
				TrilingForShort[0] = Close[0] + Multiplier * unit; /// Update the Triling line for short
				
			}
			else
			{
				TrilingForShort[0] = TrilingForShort[1]; /// The Triling line for short stay the same
				
				TrilingForLong[0] = Close[0] - Multiplier * unit; /// Update the Triling line for long
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Multiplier", Order=2, GroupName="Parameters")]
		public double Multiplier
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Triling Type", Order=2, GroupName="Parameters")]
		public TrilingType Type
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> TrilingForLong
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> TrilingForShort
		{
			get { return Values[1]; }
		}
		#endregion

	}
	
}

public enum TrilingType
{
	Atr,
	Std
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TrilingStopSmooth[] cacheTrilingStopSmooth;
		public TrilingStopSmooth TrilingStopSmooth(int period, double multiplier, TrilingType type)
		{
			return TrilingStopSmooth(Input, period, multiplier, type);
		}

		public TrilingStopSmooth TrilingStopSmooth(ISeries<double> input, int period, double multiplier, TrilingType type)
		{
			if (cacheTrilingStopSmooth != null)
				for (int idx = 0; idx < cacheTrilingStopSmooth.Length; idx++)
					if (cacheTrilingStopSmooth[idx] != null && cacheTrilingStopSmooth[idx].Period == period && cacheTrilingStopSmooth[idx].Multiplier == multiplier && cacheTrilingStopSmooth[idx].Type == type && cacheTrilingStopSmooth[idx].EqualsInput(input))
						return cacheTrilingStopSmooth[idx];
			return CacheIndicator<TrilingStopSmooth>(new TrilingStopSmooth(){ Period = period, Multiplier = multiplier, Type = type }, input, ref cacheTrilingStopSmooth);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TrilingStopSmooth TrilingStopSmooth(int period, double multiplier, TrilingType type)
		{
			return indicator.TrilingStopSmooth(Input, period, multiplier, type);
		}

		public Indicators.TrilingStopSmooth TrilingStopSmooth(ISeries<double> input , int period, double multiplier, TrilingType type)
		{
			return indicator.TrilingStopSmooth(input, period, multiplier, type);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TrilingStopSmooth TrilingStopSmooth(int period, double multiplier, TrilingType type)
		{
			return indicator.TrilingStopSmooth(Input, period, multiplier, type);
		}

		public Indicators.TrilingStopSmooth TrilingStopSmooth(ISeries<double> input , int period, double multiplier, TrilingType type)
		{
			return indicator.TrilingStopSmooth(input, period, multiplier, type);
		}
	}
}

#endregion
