// PlotIcon3D.cs: Create color bitmaps using NPlot.

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


using System.Drawing;
using NPlot;
using Subaru.Tables;

namespace ScoobyRom
{
	/// <summary>
	/// Creates NPlot ImagePlots without any annotation, useful for icons.
	/// Methods are not thread safe!
	/// </summary>
	public sealed class PlotIcon3D
	{
		const int MemoryStreamCapacity = 2048;
		const int DefaultWidth = 128;
		const int DefaultHeight = 128;

		// ImageFormat
		// Png uses transparent background; Bmp & Gif use black background; ImageFormat.Tiff adds unneeded Exif
		// MemoryBmp makes PNG on Linux!
		static readonly System.Drawing.Imaging.ImageFormat imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;

		// reuse objects where possible to improve performance
		readonly NPlot.PlotSurface2D plotSurface = new NPlot.PlotSurface2D ();
		readonly System.Drawing.Bitmap bitmap_cache;
		System.IO.MemoryStream memoryStream;

		int width, height, padding;
		System.Drawing.Rectangle bounds;
		Gdk.Pixbuf missingDataPic;

		public PlotIcon3D () : this(DefaultWidth, DefaultHeight)
		{
		}

		public PlotIcon3D (int width, int height)
		{
			this.width = width;
			this.height = height;
			// TODO improve (black) frame
			bounds = new System.Drawing.Rectangle (0, 0, width, height);
			padding = 2;

			// could also use pre-defined wrapper with internal bitmap: NPlot.Bitmap.PlotSurface2D
			bitmap_cache = new System.Drawing.Bitmap (width, height);
		}

		// Saves time since many tables have const values.
		Gdk.Pixbuf GetNoDataPixBuf {
			get {
				if (missingDataPic == null) {
					// bits per sample must be 8!
					missingDataPic = new Gdk.Pixbuf (Gdk.Colorspace.Rgb, false, 8, width, height);
					// RGBA
					missingDataPic.Fill (0xAAAAAAFF);
				}
				return missingDataPic;
			}
		}

		public void CleanupTemp ()
		{
			// free some KiB depending on icon size and image format
			memoryStream.Dispose ();
			memoryStream = null;
		}

		public Gdk.Pixbuf CreateIcon3D (Table3D table)
		{
			if (table.Zmin == table.Zmax)
				return GetNoDataPixBuf;

			Plot3D.Draw (plotSurface, table);

			// Things like Padding needs to be set each time after Clear()
			plotSurface.Padding = padding;

			using (System.Drawing.Graphics g = Graphics.FromImage (bitmap_cache)) {
				plotSurface.Draw (g, bounds);
			}

			if (memoryStream == null)
				memoryStream = new System.IO.MemoryStream (MemoryStreamCapacity);
			memoryStream.Position = 0;
			bitmap_cache.Save (memoryStream, imageFormat);
			memoryStream.Position = 0;
			// TODO create Pixbuf directly from bitmap if possible, avoiding MemoryStream
			return new Gdk.Pixbuf (memoryStream);
		}
	}
}