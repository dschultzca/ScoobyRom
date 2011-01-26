// Plot3D.cs: Draw NPlot.ImagePlot via interface. Does not depend on UI.

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
	public static class Plot3D
	{
		static readonly NPlotUtil.NPlotGradient gradient = new NPlotUtil.NPlotGradient ();

		/// <summary>
		/// Draws NPlot.ImagePlot.
		/// Does Clear(), axis are hidden.
		/// Can be used for icons as well.
		/// </summary>
		/// <param name="plotSurface2D">
		/// A <see cref="IPlotSurface2D"/>
		/// </param>
		/// <param name="table">
		/// A <see cref="Table3D"/>
		/// </param>
		public static void Draw (IPlotSurface2D plotSurface2D, Table3D table)
		{
			float[] valuesZ = table.GetValuesZasFloats ();

			// NPlot ImagePlot, needs 2D-array of type double
			int cyi = table.CountY - 1;
			int cx = table.CountX;
			double[,] data = new double[table.CountY, cx];
			for (int i = 0; i < valuesZ.Length; i++) {
				// [row, col], include y-reordering, same effect as plotSurface.YAxis1.Reversed
				// not using using YAxis1.Reversed seems to avoid a display bug (white row sometimes included)
				data[cyi - i / cx, i % cx] = valuesZ[i];
			}

			var ip = new ImagePlot (data);
			ip.Gradient = gradient;

			plotSurface2D.Clear ();
			plotSurface2D.Add (ip);

			plotSurface2D.XAxis1.Hidden = true;
			plotSurface2D.YAxis1.Hidden = true;
		}
	}
}