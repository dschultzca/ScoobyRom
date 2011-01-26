// StatisticsWindow.cs: Gtk.Window displaying some data properties.

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

namespace ScoobyRom
{
	public partial class StatisticsWindow : Gtk.Window
	{
		const string StringFormatHex = "0x{0:X}";
		const string EmptyString = "-";

		Data data;

		private StatisticsWindow () : base(Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.Icon = MainClass.AppIcon;
			this.Focus = this.buttonRefresh;
		}

		public StatisticsWindow (Data data) : this()
		{
			this.data = data;
			Update ();
		}

		public void Update ()
		{
			if (data == null)
				return;

			label2DCountTotal.Text = data.List2D.Count.ToString ();
			label3DCountTotal.Text = data.List3D.Count.ToString ();

			label2DAnnotated.Text = data.List2DAnnotated.Count.ToString ();
			label3DAnnotated.Text = data.List3DAnnotated.Count.ToString ();

			if (data.List2D.Count > 0) {
				label2DFirstRecord.Text = string.Format (StringFormatHex, data.List2D[0].Location);
				label2DLastRecord.Text = string.Format (StringFormatHex, data.List2D[data.List2D.Count - 1].Location);
			} else {
				label2DFirstRecord.Text = label2DLastRecord.Text = EmptyString;
			}

			if (data.List3D.Count > 0) {
				label3DFirstRecord.Text = string.Format (StringFormatHex, data.List3D[0].Location);
				label3DLastRecord.Text = string.Format (StringFormatHex, data.List3D[data.List3D.Count - 1].Location);
			} else {
				label3DFirstRecord.Text = label3DLastRecord.Text = EmptyString;
			}
		}

		void OnButtonRefreshClicked (object sender, System.EventArgs e)
		{
			Update ();
		}
	}
}
