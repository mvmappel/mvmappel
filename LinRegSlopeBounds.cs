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
	public class LinRegSlopeBounds : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "LinRegSlopeBounds";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				SlopePeriod					= 14;
				StdPeriod					= 40;
				StdMult					= 1;
				
				AddPlot(Brushes.RoyalBlue, "SLope");
				AddPlot(new Stroke(Brushes.Red,DashStyleHelper.Dash, 2) , PlotStyle.Line, "Upprr");
				AddPlot(new Stroke(Brushes.Red,DashStyleHelper.Dash, 2) , PlotStyle.Line, "Lower");
			}
			else if (State == State.Configure)
			{
				Plots[0].Width = 3;
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < Math.Max(StdPeriod , SlopePeriod))
				return; 
			
			Slope[0] = LinRegSlope(SlopePeriod)[0];
			Upper[0] = SMA(Slope, StdPeriod)[0] + StdMult * StdDev(Slope,StdPeriod)[0];
			Lower[0] = SMA(Slope, StdPeriod)[0] - StdMult * StdDev(Slope,StdPeriod)[0];
		}
		
		

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlopePeriod", Order=1, GroupName="Parameters")]
		public int SlopePeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="StdPeriod", Order=2, GroupName="Parameters")]
		public int StdPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="StdMult", Order=3, GroupName="Parameters")]
		public double StdMult
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Slope
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Upper
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Lower
		{
			get { return Values[2]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LinRegSlopeBounds[] cacheLinRegSlopeBounds;
		public LinRegSlopeBounds LinRegSlopeBounds(int slopePeriod, int stdPeriod, double stdMult)
		{
			return LinRegSlopeBounds(Input, slopePeriod, stdPeriod, stdMult);
		}

		public LinRegSlopeBounds LinRegSlopeBounds(ISeries<double> input, int slopePeriod, int stdPeriod, double stdMult)
		{
			if (cacheLinRegSlopeBounds != null)
				for (int idx = 0; idx < cacheLinRegSlopeBounds.Length; idx++)
					if (cacheLinRegSlopeBounds[idx] != null && cacheLinRegSlopeBounds[idx].SlopePeriod == slopePeriod && cacheLinRegSlopeBounds[idx].StdPeriod == stdPeriod && cacheLinRegSlopeBounds[idx].StdMult == stdMult && cacheLinRegSlopeBounds[idx].EqualsInput(input))
						return cacheLinRegSlopeBounds[idx];
			return CacheIndicator<LinRegSlopeBounds>(new LinRegSlopeBounds(){ SlopePeriod = slopePeriod, StdPeriod = stdPeriod, StdMult = stdMult }, input, ref cacheLinRegSlopeBounds);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LinRegSlopeBounds LinRegSlopeBounds(int slopePeriod, int stdPeriod, double stdMult)
		{
			return indicator.LinRegSlopeBounds(Input, slopePeriod, stdPeriod, stdMult);
		}

		public Indicators.LinRegSlopeBounds LinRegSlopeBounds(ISeries<double> input , int slopePeriod, int stdPeriod, double stdMult)
		{
			return indicator.LinRegSlopeBounds(input, slopePeriod, stdPeriod, stdMult);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LinRegSlopeBounds LinRegSlopeBounds(int slopePeriod, int stdPeriod, double stdMult)
		{
			return indicator.LinRegSlopeBounds(Input, slopePeriod, stdPeriod, stdMult);
		}

		public Indicators.LinRegSlopeBounds LinRegSlopeBounds(ISeries<double> input , int slopePeriod, int stdPeriod, double stdMult)
		{
			return indicator.LinRegSlopeBounds(input, slopePeriod, stdPeriod, stdMult);
		}
	}
}

#endregion
