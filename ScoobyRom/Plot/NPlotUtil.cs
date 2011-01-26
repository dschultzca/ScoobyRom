// NPlotUtil.cs: Utility classes.

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


using System;

namespace NPlotUtil
{
	public static class ColorExtensions
	{
		public static System.Drawing.Color ToWinformsColor (this Cairo.Color color)
		{
			const double f = 255;
			return System.Drawing.Color.FromArgb ((int)(color.A * f), (int)(color.R * f), (int)(color.G * f), (int)(color.B * f));
		}
	}

	/// <summary>
	/// To use own coloring in an NPlot.ImagePlot
	/// </summary>
	public sealed class NPlotGradient : NPlot.IGradient
	{
		readonly Util.Coloring coloring;

		public NPlotGradient () : this(new Util.Coloring ())
		{
		}

		public NPlotGradient (Util.Coloring coloring)
		{
			this.coloring = coloring;
		}

		// must be able to handle double.NaN !
		public System.Drawing.Color GetColor (double prop)
		{
			// can handle NaN, returns Cairo.Color, must convert to Drawing.Color
			return coloring.GetColor (prop).ToWinformsColor ();
		}
	}
}
