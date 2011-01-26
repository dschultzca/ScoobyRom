// Plot2D.cs: Draw line graph using NPlot interface. Does not depend on UI.

/* Copyright (C) 2011 SubaruDieselCrew
 *
 * This file is part of ScoobyRom.
 *
 * ScoobyRom is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ScoobyRom is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ScoobyRom.  If not, see <http://www.gnu.org/licenses/>.
 */


using NPlot;
using Subaru.Tables;

namespace ScoobyRom
{
	public sealed class Plot2D
	{
		const float PenWidth = 3f;
		const int MarkerSize = 6;

		// Default = no antialiasing!
		const System.Drawing.Drawing2D.SmoothingMode SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

		readonly IPlotSurface2D plotSurface2D;

		// FontFamily.GenericSansSerif -> "Arial" on Linux
		readonly System.Drawing.Font titleFont = new System.Drawing.Font (System.Drawing.FontFamily.GenericSansSerif, 20, System.Drawing.GraphicsUnit.Point);
		readonly System.Drawing.Font labelFont = new System.Drawing.Font (System.Drawing.FontFamily.GenericSansSerif, 16, System.Drawing.GraphicsUnit.Point);
		readonly System.Drawing.Font tickTextFont = new System.Drawing.Font (System.Drawing.FontFamily.GenericSansSerif, 14, System.Drawing.GraphicsUnit.Point);

		readonly NPlot.Marker marker;
		readonly System.Drawing.Pen pen;

		public Plot2D (IPlotSurface2D plotSurface)
		{
			this.plotSurface2D = plotSurface;

			pen = new System.Drawing.Pen (System.Drawing.Color.Red, PenWidth);
			marker = new Marker (Marker.MarkerType.FilledCircle, MarkerSize, System.Drawing.Color.Blue);
		}

		/// <summary>
		/// Might need Refresh () afterwards!
		/// </summary>
		/// <param name="table2D">
		/// A <see cref="Table2D"/>
		/// </param>
		public void Draw (Table2D table2D)
		{
			float[] valuesY = table2D.GetValuesYasFloats ();

			// clear everything. reset fonts. remove plot components etc.
			this.plotSurface2D.Clear ();
			plotSurface2D.Padding = 0;
			plotSurface2D.SmoothingMode = SmoothingMode;

			// y-values, x-values (!)
			LinePlot lp = new LinePlot (valuesY, table2D.ValuesX);
			lp.Pen = pen;

			PointPlot pp = new PointPlot (marker);
			pp.AbscissaData = table2D.ValuesX;
			pp.OrdinateData = valuesY;

			Grid myGrid = new Grid ();
			myGrid.VerticalGridType = Grid.GridType.Coarse;
			myGrid.HorizontalGridType = Grid.GridType.Coarse;

			plotSurface2D.Add (myGrid);
			plotSurface2D.Add (lp);
			plotSurface2D.Add (pp);

			plotSurface2D.TitleFont = titleFont;
			plotSurface2D.Title = table2D.Title;

			plotSurface2D.XAxis1.LabelFont = labelFont;
			plotSurface2D.XAxis1.Label = AxisText (table2D.NameX, table2D.UnitX);
			// could use ex: plotSurface2D.YAxis1.NumberFormat = "0.000";
			plotSurface2D.XAxis1.TickTextFont = tickTextFont;

			plotSurface2D.YAxis1.LabelFont = labelFont;
			plotSurface2D.YAxis1.Label = AxisText (table2D.Title, table2D.UnitY);
			plotSurface2D.YAxis1.TickTextFont = tickTextFont;

			// Refresh () not part of interface!
		}

		// "Axisname [Unit]"
		static string AxisText (string name, string unit)
		{
			if (string.IsNullOrEmpty (name) && string.IsNullOrEmpty (unit))
				return string.Empty;
			else
				return string.Format ("{0} [{1}]", name, unit);
		}
	}
}
