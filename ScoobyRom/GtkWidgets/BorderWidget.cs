// BorderWidget.cs: Gtk# widget that draws a background color and hosts a child widget.

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
using Gtk;

namespace GtkWidgets
{
	public static class Extensions
	{
		public static bool CairoColorEquals (this Cairo.Color color, Cairo.Color other)
		{
			return color.R == other.R && color.G == other.G && color.B == other.B && color.A == other.A;
		}
	}

	// Deriving from Gtk.Frame seems easiest solution: just have to paint background.
	// Disadvantage: Frame might not look good on all platforms.
	// Can put any widget inside - Label, Entry, ...
	[System.ComponentModel.ToolboxItem(true)]
	public sealed class BorderWidget : Gtk.Frame
	{
		Cairo.Color color;

		public Cairo.Color Color {
			get { return this.color; }
			set {
				if (!this.color.CairoColorEquals (value)) {
					color = value;
					QueueDraw ();
				}
			}
		}

		public BorderWidget ()
		{
		}

		// GLib.Object subclass GtkWidgets.BorderWidget must provide a protected or public
		// IntPtr ctor to support wrapping of native object handles.
		public BorderWidget (IntPtr raw) : base(raw)
		{
		}


		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			// draw background fill before child widget so child appears on top
			// use Cairo drawing API (Gtk+ uses it internally as well)
			using (Cairo.Context cr = Gdk.CairoHelper.Create (evnt.Window)) {
				cr.Color = this.color;
				cr.Rectangle (this.Allocation.Left, this.Allocation.Top, this.Allocation.Width, this.Allocation.Height);
				cr.Fill ();
			}

			// display child
			return base.OnExposeEvent (evnt);
		}
	}
}
