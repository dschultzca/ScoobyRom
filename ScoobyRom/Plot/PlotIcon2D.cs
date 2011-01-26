// PlotIcon2D.cs: Create line graph bitmaps using NPlot.

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
	/// Creates NPlot 2D graphs without any annotation, useful for icons.
	/// Methods are not thread safe!
	/// </summary>
	public sealed class PlotIcon2D
	{
		const int MemoryStreamCapacity = 2048;
		const int DefaultWidth = 128;
		const int DefaultHeight = 128;
		// Default = no antialisaing!
		const System.Drawing.Drawing2D.SmoothingMode SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;


		// ImageFormat:
		// Png uses transparent background; Bmp & Gif use black background; ImageFormat.Tiff adds unneeded Exif
		// MemoryBmp makes PNG on Linux!
		static readonly System.Drawing.Imaging.ImageFormat imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;

		readonly System.Drawing.Bitmap bitmap_cache;
		System.IO.MemoryStream memoryStream;
		readonly NPlot.PlotSurface2D plotSurface = new NPlot.PlotSurface2D ();

		readonly System.Drawing.Pen pen;

		int width, height, padding;
		System.Drawing.Rectangle bounds;
		Gdk.Pixbuf missingDataPic;

		public PlotIcon2D () : this(DefaultWidth, DefaultHeight)
		{
		}

		public PlotIcon2D (int width, int height)
		{
			this.width = width;
			this.height = height;
			bounds = new System.Drawing.Rectangle (0, 0, width, height);

			// could also use pre-defined wrapper with internal bitmap: NPlot.Bitmap.PlotSurface2D
			bitmap_cache = new System.Drawing.Bitmap (width, height);

			pen = new System.Drawing.Pen (System.Drawing.Color.Red, width >= 32 ? 2f : 1f);
			// black/transparent (depending on image format) frame
			padding = 2;
		}

		// very useful since many Tables have const values.
		Gdk.Pixbuf GetNoDataPixBuf {
			get {
				if (missingDataPic == null) {
					//					Gtk.Image image = new Gtk.Image ();
					//					missingDataPic = image.RenderIcon (Gtk.Stock.MissingImage, Gtk.IconSize.SmallToolbar, null);
					//					image.Dispose ();

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
			memoryStream.Dispose ();
			memoryStream = null;
		}

		public Gdk.Pixbuf CreateIcon2D (Table2D table)
		{
			if (table.Ymin == table.Ymax)
				return GetNoDataPixBuf;

			plotSurface.Clear ();
			// needs to be set each time after Clear()
			plotSurface.Padding = padding;
			plotSurface.SmoothingMode = SmoothingMode;

			float[] valuesY = table.GetValuesYasFloats ();

			// y-values, x-values (!)
			LinePlot lp = new LinePlot (valuesY, table.ValuesX);
			lp.Pen = pen;

			plotSurface.Add (lp);

			plotSurface.XAxis1.Hidden = true;
			plotSurface.YAxis1.Hidden = true;

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