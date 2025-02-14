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
	public class SupplyDemandZones : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"An indicator that plots supply and demand zones on chart";
				Name										= "SupplyDemandZones";
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
				SupplyBrush					= Brushes.OrangeRed;
				DemandBrush					= Brushes.Teal;
				Opacity						= 20;
				LookbackPeriod				= 90;
				ZoneSize					= 20;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if (CurrentBars[0] < 600) return;
			
			int highestBarsAgo = HighestBar(High, LookbackPeriod);
			double zoneHighTop = High[highestBarsAgo];
			double zoneHighBottom = zoneHighTop - ZoneSize;
			
			int lowestBarsAgo = LowestBar(Low, LookbackPeriod);
			double zoneLowTop = Low[lowestBarsAgo];
			double zoneLowBottom = zoneLowTop + ZoneSize;
			
			Draw.RegionHighlightY(this, "Supply", true, zoneHighTop, zoneHighBottom, SupplyBrush, SupplyBrush, Opacity);
			Draw.RegionHighlightY(this, "Demand", true, zoneLowTop, zoneLowBottom, DemandBrush, DemandBrush, Opacity);
		}

		#region Properties
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="SupplyBrush", Order=1, GroupName="Parameters")]
		public Brush SupplyBrush
		{ get; set; }

		[Browsable(false)]
		public string SupplyBrushSerializable
		{
			get { return Serialize.BrushToString(SupplyBrush); }
			set { SupplyBrush = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="DemandBrush", Order=2, GroupName="Parameters")]
		public Brush DemandBrush
		{ get; set; }

		[Browsable(false)]
		public string DemandBrushSerializable
		{
			get { return Serialize.BrushToString(DemandBrush); }
			set { DemandBrush = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Opacity", Order=3, GroupName="Parameters")]
		public int Opacity
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LookbackPeriod", Order=4, GroupName="Parameters")]
		public int LookbackPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ZoneSize", Order=5, GroupName="Parameters")]
		public int ZoneSize
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SupplyDemandZones[] cacheSupplyDemandZones;
		public SupplyDemandZones SupplyDemandZones(Brush supplyBrush, Brush demandBrush, int opacity, int lookbackPeriod, int zoneSize)
		{
			return SupplyDemandZones(Input, supplyBrush, demandBrush, opacity, lookbackPeriod, zoneSize);
		}

		public SupplyDemandZones SupplyDemandZones(ISeries<double> input, Brush supplyBrush, Brush demandBrush, int opacity, int lookbackPeriod, int zoneSize)
		{
			if (cacheSupplyDemandZones != null)
				for (int idx = 0; idx < cacheSupplyDemandZones.Length; idx++)
					if (cacheSupplyDemandZones[idx] != null && cacheSupplyDemandZones[idx].SupplyBrush == supplyBrush && cacheSupplyDemandZones[idx].DemandBrush == demandBrush && cacheSupplyDemandZones[idx].Opacity == opacity && cacheSupplyDemandZones[idx].LookbackPeriod == lookbackPeriod && cacheSupplyDemandZones[idx].ZoneSize == zoneSize && cacheSupplyDemandZones[idx].EqualsInput(input))
						return cacheSupplyDemandZones[idx];
			return CacheIndicator<SupplyDemandZones>(new SupplyDemandZones(){ SupplyBrush = supplyBrush, DemandBrush = demandBrush, Opacity = opacity, LookbackPeriod = lookbackPeriod, ZoneSize = zoneSize }, input, ref cacheSupplyDemandZones);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SupplyDemandZones SupplyDemandZones(Brush supplyBrush, Brush demandBrush, int opacity, int lookbackPeriod, int zoneSize)
		{
			return indicator.SupplyDemandZones(Input, supplyBrush, demandBrush, opacity, lookbackPeriod, zoneSize);
		}

		public Indicators.SupplyDemandZones SupplyDemandZones(ISeries<double> input , Brush supplyBrush, Brush demandBrush, int opacity, int lookbackPeriod, int zoneSize)
		{
			return indicator.SupplyDemandZones(input, supplyBrush, demandBrush, opacity, lookbackPeriod, zoneSize);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SupplyDemandZones SupplyDemandZones(Brush supplyBrush, Brush demandBrush, int opacity, int lookbackPeriod, int zoneSize)
		{
			return indicator.SupplyDemandZones(Input, supplyBrush, demandBrush, opacity, lookbackPeriod, zoneSize);
		}

		public Indicators.SupplyDemandZones SupplyDemandZones(ISeries<double> input , Brush supplyBrush, Brush demandBrush, int opacity, int lookbackPeriod, int zoneSize)
		{
			return indicator.SupplyDemandZones(input, supplyBrush, demandBrush, opacity, lookbackPeriod, zoneSize);
		}
	}
}

#endregion
