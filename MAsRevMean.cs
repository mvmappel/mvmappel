#region Using declarations
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public class MAs10EMA : Indicator
	{
		// Button Management
		private System.Windows.Controls.Grid		buttonsGrid;
		private System.Windows.Controls.Button		pauseButton;
		private bool Pause = false;
		private double TargetDownOffset = .75;
		private double TargetUpOffset = 1.25;		

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Signal 10EMA possible trades";
				Name										= "MAs10EMA";
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
				MeanBrush					= Brushes.Black;
				TargetDownBrush		 		= Brushes.Green;
				TargetUpBrush			 	= Brushes.Green;				
				Opacity						= 20;
				LookbackPeriod				= 90;
				ZoneSize					= 10;
				EMAPeriod					= 10;
				AddPlot(Brushes.Red, "EmaPlot1");
			}
			else if (State == State.Configure)
			{
				Pause = false;
			}
			else if (State == State.Historical)
			{			
				if (ChartControl != null)
					CreateWPFControls();
			}
			else if (State == State.Terminated)
			{
				if (ChartControl != null)
					RemoveWPFControls();
			}
		}

		protected override void OnBarUpdate()
		{
			try {
				//if (CurrentBars[0] < 600) return;
				double ema10 = EMA(Close, EMAPeriod)[0];
				EmaPlot1[0] = ema10;
				
				int highestBarsAgo = HighestBar(High, LookbackPeriod);
				double zoneHighTop = High[highestBarsAgo];
				double zoneHighBottom = zoneHighTop - ZoneSize;
				
				int lowestBarsAgo = LowestBar(Low, LookbackPeriod);
				double zoneLowTop = Low[lowestBarsAgo];
				double zoneLowBottom = zoneLowTop + ZoneSize;
				
				double zoneMeanTop = ((zoneHighTop + zoneLowTop)/2);
				double zoneMeanBottom = ((zoneHighTop + zoneLowTop)/2);
				
				double zoneTargetDownTop = zoneHighBottom - ((zoneHighBottom - zoneMeanTop) * TargetDownOffset);
				double zoneTargetDownBottom =  zoneHighBottom - ((zoneHighBottom - zoneMeanTop) * TargetDownOffset);
				
				double zoneTargetUpTop = zoneHighBottom - ((zoneHighBottom - zoneMeanTop) * TargetUpOffset);//zoneLowTop + ((zoneMeanTop - zoneLowTop));
				double zoneTargetUpBottom = zoneHighBottom - ((zoneHighBottom - zoneMeanTop) * TargetUpOffset);//zoneLowTop + ((zoneMeanTop - zoneLowTop));
				
				Draw.RegionHighlightY(this, "Supply", true, zoneHighTop, zoneHighBottom, SupplyBrush, SupplyBrush, Opacity);
				Draw.RegionHighlightY(this, "Demand", true, zoneLowTop, zoneLowBottom, DemandBrush, DemandBrush, Opacity);
				Draw.RegionHighlightY(this, "Middle", true, zoneMeanTop, zoneMeanBottom, MeanBrush, MeanBrush, Opacity);
				Draw.RegionHighlightY(this, "PercDownTarg", true, zoneTargetDownTop, zoneTargetDownBottom, TargetDownBrush, TargetDownBrush, Opacity);
				Draw.RegionHighlightY(this, "PercUpTarg", true, zoneTargetUpTop, zoneTargetUpBottom, TargetUpBrush, TargetUpBrush, Opacity);				
				
				if (!Pause) {
					if (Close[0] > Open[0] && Close[0] < ema10) {
						Draw.TriangleDown (this, "DwnArrow"+CurrentBar, true, 0, (DrawOnPricePanel ? High[0] + 18 * TickSize: ema10), Brushes.Red);					
						Alert("downAlert", Priority.High, "Short Trigger", NinjaTrader.Core.Globals.InstallDir+@"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);								
					}
					
					if (Close[0] < Open[0] && Close[0] > ema10) {
						Draw.TriangleUp (this, "UpArrow"+CurrentBar, true, 0, (DrawOnPricePanel ? Low[0] - 18 * TickSize: ema10), Brushes.Green);					
						Alert("downAlert", Priority.High, "Short Trigger", NinjaTrader.Core.Globals.InstallDir+@"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);													
					}
				}
			} catch(Exception e) {
				// Catching Exception and moving on
			}
		}
		
#region Button Management
		private void OnButtonClick(object sender, RoutedEventArgs rea)
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			
			if (button == pauseButton)
			{
				string pauseChk = button.Content.ToString();
				
				if (pauseChk.Equals("Pause")) {
					Pause = true;
					button.Content = "Un-Pause";
				} else {
					Pause = false;
					button.Content = "Pause";
				}
			}			
		}
		
		private void CreateWPFControls()
		{
			// if the script has already added the controls, do not add a second time.
//			if (UserControlCollection.Contains(buttonsGrid))
//				return;
			
			// when making WPF changes to the UI, run the code on the UI thread of the chart
			ChartControl.Dispatcher.InvokeAsync((() =>
			{
				// this buttonGrid will contain the buttons
				buttonsGrid = new System.Windows.Controls.Grid
				{
					Background			= Brushes.White,
					Name				= "ButtonsGrid",
					HorizontalAlignment	= HorizontalAlignment.Right,
					VerticalAlignment	= VerticalAlignment.Top
				};

				for (int i = 0; i < 1; i++)
					buttonsGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
				
				for (int i = 0; i < 1; i++)
					buttonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
			
				pauseButton = new System.Windows.Controls.Button
				{
					Name		= "PauseButton",
					Content		= "Pause",
					Foreground	= Brushes.White,
					Background	= Brushes.CornflowerBlue
				};
				pauseButton.Click += OnButtonClick;				
				buttonsGrid.Children.Add(pauseButton);
				System.Windows.Controls.Grid.SetRow(pauseButton, 0);
				System.Windows.Controls.Grid.SetColumn(pauseButton, 0);
				buttonsGrid.Visibility = Visibility.Visible;
				pauseButton.Visibility = Visibility.Visible;
			}));
		}
		
		private void RemoveWPFControls()
		{
			ChartControl.Dispatcher.InvokeAsync((() =>
			{
				if (pauseButton != null)
				{
					pauseButton.Click -= OnButtonClick;
					pauseButton = null;
				}
			}));
		}
#endregion
			
#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(Name = "EMAPeriod", Order = 0)]
		public int EMAPeriod
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EmaPlot1
		{
			get { return Values[0]; }
		}		
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="SupplyBrush", Order=1)]
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
		[Display(Name="DemandBrush", Order=2)]
		public Brush DemandBrush
		{ get; set; }

		[Browsable(false)]
		public string DemandBrushSerializable
		{
			get { return Serialize.BrushToString(DemandBrush); }
			set { DemandBrush = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="MeanBrush", Order=3)]
		public Brush MeanBrush
		{ get; set; }

		[Browsable(false)]
		public string MeanBrushSerializable
		{
			get { return Serialize.BrushToString(MeanBrush); }
			set { MeanBrush = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="TargetDownBrush", Order=4)]
		public Brush TargetDownBrush
		{ get; set; }

		[Browsable(false)]
		public string TargetDownBrushSerializable
		{
			get { return Serialize.BrushToString(TargetDownBrush); }
			set { TargetDownBrush = Serialize.StringToBrush(value); }
		}		

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="TargetUpBrush", Order=5)]
		public Brush TargetUpBrush
		{ get; set; }

		[Browsable(false)]
		public string TargetUpBrushSerializable
		{
			get { return Serialize.BrushToString(TargetUpBrush); }
			set { TargetUpBrush = Serialize.StringToBrush(value); }
		}			
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Opacity", Order=6)]
		public int Opacity
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LookbackPeriod", Order=7)]
		public int LookbackPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ZoneSize", Order=8)]
		public int ZoneSize
		{ get; set; }
	}
}		
#endregion

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MAs10EMA[] cacheMAs10EMA;
		public MAs10EMA MAs10EMA(int eMAPeriod, Brush supplyBrush, Brush demandBrush, Brush meanBrush, Brush targetDownBrush, Brush targetUpBrush, int opacity, int lookbackPeriod, int zoneSize)
		{
			return MAs10EMA(Input, eMAPeriod, supplyBrush, demandBrush, meanBrush, targetDownBrush, targetUpBrush, opacity, lookbackPeriod, zoneSize);
		}

		public MAs10EMA MAs10EMA(ISeries<double> input, int eMAPeriod, Brush supplyBrush, Brush demandBrush, Brush meanBrush, Brush targetDownBrush, Brush targetUpBrush, int opacity, int lookbackPeriod, int zoneSize)
		{
			if (cacheMAs10EMA != null)
				for (int idx = 0; idx < cacheMAs10EMA.Length; idx++)
					if (cacheMAs10EMA[idx] != null && cacheMAs10EMA[idx].EMAPeriod == eMAPeriod && cacheMAs10EMA[idx].SupplyBrush == supplyBrush && cacheMAs10EMA[idx].DemandBrush == demandBrush && cacheMAs10EMA[idx].MeanBrush == meanBrush && cacheMAs10EMA[idx].TargetDownBrush == targetDownBrush && cacheMAs10EMA[idx].TargetUpBrush == targetUpBrush && cacheMAs10EMA[idx].Opacity == opacity && cacheMAs10EMA[idx].LookbackPeriod == lookbackPeriod && cacheMAs10EMA[idx].ZoneSize == zoneSize && cacheMAs10EMA[idx].EqualsInput(input))
						return cacheMAs10EMA[idx];
			return CacheIndicator<MAs10EMA>(new MAs10EMA(){ EMAPeriod = eMAPeriod, SupplyBrush = supplyBrush, DemandBrush = demandBrush, MeanBrush = meanBrush, TargetDownBrush = targetDownBrush, TargetUpBrush = targetUpBrush, Opacity = opacity, LookbackPeriod = lookbackPeriod, ZoneSize = zoneSize }, input, ref cacheMAs10EMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MAs10EMA MAs10EMA(int eMAPeriod, Brush supplyBrush, Brush demandBrush, Brush meanBrush, Brush targetDownBrush, Brush targetUpBrush, int opacity, int lookbackPeriod, int zoneSize)
		{
			return indicator.MAs10EMA(Input, eMAPeriod, supplyBrush, demandBrush, meanBrush, targetDownBrush, targetUpBrush, opacity, lookbackPeriod, zoneSize);
		}

		public Indicators.MAs10EMA MAs10EMA(ISeries<double> input , int eMAPeriod, Brush supplyBrush, Brush demandBrush, Brush meanBrush, Brush targetDownBrush, Brush targetUpBrush, int opacity, int lookbackPeriod, int zoneSize)
		{
			return indicator.MAs10EMA(input, eMAPeriod, supplyBrush, demandBrush, meanBrush, targetDownBrush, targetUpBrush, opacity, lookbackPeriod, zoneSize);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MAs10EMA MAs10EMA(int eMAPeriod, Brush supplyBrush, Brush demandBrush, Brush meanBrush, Brush targetDownBrush, Brush targetUpBrush, int opacity, int lookbackPeriod, int zoneSize)
		{
			return indicator.MAs10EMA(Input, eMAPeriod, supplyBrush, demandBrush, meanBrush, targetDownBrush, targetUpBrush, opacity, lookbackPeriod, zoneSize);
		}

		public Indicators.MAs10EMA MAs10EMA(ISeries<double> input , int eMAPeriod, Brush supplyBrush, Brush demandBrush, Brush meanBrush, Brush targetDownBrush, Brush targetUpBrush, int opacity, int lookbackPeriod, int zoneSize)
		{
			return indicator.MAs10EMA(input, eMAPeriod, supplyBrush, demandBrush, meanBrush, targetDownBrush, targetUpBrush, opacity, lookbackPeriod, zoneSize);
		}
	}
}

#endregion
