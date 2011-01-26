// TableWidget.cs: Builds a Gtk.Table showing table data values.

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
	// 3D table only so far.
	// TODO Visualize 2D table data.
	public class TableWidget
	{
		const int DataColLeft = 2;
		const int DataRowTop = 3;

		readonly int countX, countY, cols, rows;
		string titleMarkup;
		string axisMarkupX = "X Axis [-]";
		string axisMarkupY = "Y Axis [-]";
		string formatValues = "0.000";
		readonly float[] axisX, axisY, values;
		readonly float valuesMax, valuesMin;

		Gtk.Table table;
		Widget[] axisWidgetsX, axisWidgetsY, valueWidgets;

		readonly Util.Coloring coloring;

		// Use LINQ to calc min/max
//		public TableWidget (Util.Coloring coloring, float[] axisX, float[] axisY, float[] valuesZ) :
//			this(coloring, axisX, axisY, valuesZ, valuesZ.Min (), valuesZ.Max ())
//		{
//		}

		/// <summary>
		///	Create Gtk.Table for 3D table data.
		/// </summary>
		public TableWidget (Util.Coloring coloring, float[] axisX, float[] axisY, float[] valuesZ, float valuesZmin, float valuesZmax)
		{
			this.coloring = coloring;
			this.axisX = axisX;
			this.axisY = axisY;
			this.values = valuesZ;
			this.valuesMin = valuesZmin;
			this.valuesMax = valuesZmax;

			this.countX = this.axisX.Length;
			this.countY = this.axisY.Length;

			if (axisX.Length * axisY.Length != valuesZ.Length)
				throw new ArgumentException ("x.Length * y.Length != z.Length");

			this.cols = this.countX + DataColLeft;
			this.rows = this.countY + DataRowTop;
		}

		public string TitleMarkup {
			get { return this.titleMarkup; }
			set { titleMarkup = value; }
		}

		public string AxisMarkupX {
			get { return this.axisMarkupX; }
			set { axisMarkupX = value; }
		}

		public string AxisMarkupY {
			get { return this.axisMarkupY; }
			set { axisMarkupY = value; }
		}

		public string FormatValues {
			get { return this.formatValues; }
			set { formatValues = value; }
		}

		public Gtk.Widget Create ()
		{
			table = new Gtk.Table ((uint)rows, (uint)cols, false);

			Gtk.Label title = new Label ();
			title.Markup = this.titleMarkup;
			// label starting at left with SetAlignment also needs AttachOptions.Fill for it to work
			title.SetAlignment (0f, 0.5f);
			table.Attach (title, 0, (uint)cols, 0, 1, AttachOptions.Fill, AttachOptions.Shrink, 0, 0);

			// add some spacing so cell content won't touch
			table.ColumnSpacing = table.RowSpacing = 0;

			const uint AxisPadX = 2;
			const uint AxisPadY = 2;

			// x axis
			axisWidgetsX = new Widget[countX];
			for (uint i = 0; i < countX; i++) {
				Gtk.Label label = new Label ();
				label.Text = axisX[i].ToString ();

				axisWidgetsX[i] = label;
				table.Attach (label, DataColLeft + i, DataColLeft + 1 + i, DataRowTop - 1, DataRowTop, AttachOptions.Shrink, AttachOptions.Shrink, AxisPadX, AxisPadY);
			}

			// y axis
			axisWidgetsY = new Widget[countY];
			for (uint i = 0; i < countY; i++) {
				Gtk.Label label = new Label ();
				label.Text = axisY[i].ToString ();
				label.SetAlignment (1f, 0f);

				axisWidgetsY[i] = label;
				table.Attach (label, DataColLeft - 1, DataColLeft, DataRowTop + i, DataRowTop + 1 + i, AttachOptions.Fill, AttachOptions.Shrink, AxisPadX, AxisPadY);
			}

			// values
			int countZ = values.Length;
			valueWidgets = new Widget[countZ];
			for (uint i = 0; i < countZ; i++) {
				float val = values[i];
				Gtk.Widget label = new Label (val.ToString (this.formatValues));
				BorderWidget widget = new BorderWidget ();

				// ShadowType differences might be minimal
				if (val >= this.valuesMax)
					widget.Shadow = ShadowType.EtchedOut; else if (val <= this.valuesMin)
					widget.Shadow = ShadowType.EtchedOut;

				widget.Color = CalcColor (val);
				widget.Add (label);

				valueWidgets[i] = widget;
				uint row = DataRowTop + i / (uint)this.countX;
				uint col = DataColLeft + i % (uint)this.countX;

				table.Attach (widget, col, col + 1, row, row + 1, AttachOptions.Fill, AttachOptions.Fill, 0, 0);
			}

			// x axis name
			Gtk.Label titleX = new Gtk.Label ();
			titleX.Markup = "<b>" + this.axisMarkupX + "</b>";
			//titleX.SetAlignment (0.5f, 0.5f);
			table.Attach (titleX, DataColLeft, (uint)cols, DataRowTop - 2, DataRowTop - 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			// y axis name
			Gtk.Label titleY = new Gtk.Label ();
			// Turning on any wrap property causes 0 angle!
			//titleY.Wrap = true;
			//titleY.LineWrap = true;
			//titleY.LineWrapMode = Pango.WrapMode.WordChar;
			titleY.Angle = 90;
			titleY.Markup = "<b>" + this.axisMarkupY + "</b>";

			//titleY.SetAlignment (0.5f, 0.5f);
			table.Attach (titleY, 0, 1, DataRowTop, (uint)rows, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			//table.Homogeneous = true;
			return table;
		}

		Cairo.Color CalcColor (float val)
		{
			double factor = (val - valuesMin) / (valuesMax - valuesMin);
			// should be able to handle division by zero (NaN)
			return coloring.GetColor (factor);
		}

		public static string MakeMarkup (string name, string unit)
		{
			if (string.IsNullOrEmpty (name) && string.IsNullOrEmpty (unit))
				return string.Empty;
			else
				return string.Format ("<span weight=\"bold\">{0} <tt>[{1}]</tt></span>", name, unit);
		}

		public static string MakeTitleMarkup (string name, string unit)
		{
			if (string.IsNullOrEmpty (name) && string.IsNullOrEmpty (unit))
				return string.Empty;
			else
				return string.Format ("<span size=\"large\" weight=\"bold\">{0} <tt>[{1}]</tt></span>", name, unit);
		}
	}
}

