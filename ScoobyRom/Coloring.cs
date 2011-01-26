// Coloring.cs: Calculate colors for values.

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


// Designed for Cairo graphics (Gtk+ uses Cairo internally)

// APIs taking Gdk.Color are not required here
//#define IncludeGdkColor

using System;

namespace Util
{
	public sealed class Coloring
	{
		static readonly Cairo.Color invalidCol = new Cairo.Color (0.7, 0.7, 0.7);

		double hMin, sMin, vMin;
		double hMax, sMax, vMax;

		#region Constructors

		// Initialize with a good rainbow color range
		public Coloring () : this(new Cairo.Color (0.6, 0.6, 1.0), new Cairo.Color (1.0, 0.4, 0.4))
		{
		}

		public Coloring (Cairo.Color colorMin, Cairo.Color colorMax)
		{
			CairoColorToHSV (colorMin, out hMin, out sMin, out vMin);
			CairoColorToHSV (colorMax, out hMax, out sMax, out vMax);
		}

		#if IncludeGdkColor

		public Coloring (Gdk.Color colorMin, Gdk.Color colorMax)
		{
			GdkColorToHSV (colorMin, out hMin, out sMin, out vMin);
			GdkColorToHSV (colorMax, out hMax, out sMax, out vMax);
		}

		#endif

		#endregion Constructors


		/// <summary>
		/// Get interpolated color for a value.
		/// </summary>
		/// <param name="factor">
		/// A <see cref="System.Double"/> 0.0 ≤ x ≤ 1.0.
		/// </param>
		/// <returns>
		/// A <see cref="Cairo.Color"/>
		/// </returns>
		public Cairo.Color GetColor (double factor)
		{
			if (double.IsNaN (factor))
				return invalidCol;

			#if DEBUG

			if (factor < 0.0 || factor > 1.0)
				throw new ArgumentOutOfRangeException ("factor", factor, "0.0 ≤ x ≤ 1.0");

			#endif

			double h = hMin + factor * (hMax - hMin);
			double s = sMin + factor * (sMax - sMin);
			double v = vMin + factor * (vMax - vMin);

			double r, g, b;
			// There does not seem to exist any other HSVtoRGB function, not in Gtk.Global!
			Gtk.HSV.ToRgb (h, s, v, out r, out g, out b);
			return new Cairo.Color (r, g, b);
		}

		public static void CairoColorToHSV (Cairo.Color color, out double h, out double s, out double v)
		{
			Gtk.Global.RgbToHsv (color.R, color.G, color.B, out h, out s, out v);
		}

		#if IncludeGdkColor

		public static void GdkColorToHSV (Gdk.Color color, out double h, out double s, out double v)
		{
			// Gdk.Color fields are type ushort although its constructor takes bytes!
			const double ScaleTo1 = 1.0 / ushort.MaxValue;
			Gtk.Global.RgbToHsv (color.Red * ScaleTo1, color.Green * ScaleTo1, color.Blue * ScaleTo1, out h, out s, out v);
		}

		#endif
	}
}
