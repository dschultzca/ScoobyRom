// ChecksumWindow.cs: Gtk.Window displaying ROM checksums and CVN.

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
using System.Collections.Generic;
using Gtk;
using Subaru;

namespace ScoobyRom
{
	public partial class ChecksumWindow : Gtk.Window
	{
		enum ColNr
		{
			Index,
			Start,
			End,
			SumTable,
			Icon,
			SumCalc
		}


		RomChecksumming rcs;

		ListStore store;
		readonly Dictionary<TreeViewColumn, ColNr> columnsDict = new Dictionary<TreeViewColumn, ColNr> (5);
		readonly Gdk.Pixbuf[] pixbufs = new Gdk.Pixbuf[2];

		CellRendererText cellRendererText;
		CellRendererPixbuf cellRendererPixbuf;

		public ChecksumWindow () : base(Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.Icon = MainClass.AppIcon;
			Init ();
		}

		void Init ()
		{
			// index, (start, end, tableSum), icon, calcSum
			store = new ListStore (typeof(int), typeof(int), typeof(int), typeof(int), typeof(Gdk.Pixbuf), typeof(int));

			treeviewCSums.RulesHint = true;
			treeviewCSums.Model = store;

			cellRendererText = new CellRendererText ();
			Pango.FontDescription fontDesc = new Pango.FontDescription ();
			fontDesc.Family = System.Environment.OSVersion.Platform == PlatformID.Win32NT ? "Monospace" : "DejaVu Sans Mono";

			cellRendererText.FontDesc = fontDesc;

			cellRendererPixbuf = new CellRendererPixbuf ();

			AddColumn (new TreeViewColumn ("#", cellRendererText, "text", ColNr.Index), ColNr.Index);

			AddColumn (AddHexColumn ("Start", ColNr.Start), ColNr.Start);
			AddColumn (AddHexColumn ("Last", ColNr.End), ColNr.End);
			AddColumn (AddHexColumn ("Checksum", ColNr.SumTable), ColNr.SumTable);

			AddColumn (new TreeViewColumn (null, cellRendererPixbuf, "pixbuf", ColNr.Icon), ColNr.Icon);

			AddColumn (AddHexColumn ("Calculated", ColNr.SumCalc), ColNr.SumCalc);

			InitIcons ();
		}

		void InitIcons ()
		{
			// could use this.RenderIcon(...) but those icons can be less appealing (grey check mark, red cross)
			Gtk.Image image = new Gtk.Image ();
			pixbufs[0] = image.RenderIcon (Gtk.Stock.No, IconSize.SmallToolbar, null);
			pixbufs[1] = image.RenderIcon (Gtk.Stock.Yes, IconSize.SmallToolbar, null);
			image.Destroy ();
		}

		TreeViewColumn AddColumn (TreeViewColumn column, ColNr colNr)
		{
			// Reorderable: default false
			column.Reorderable = true;
			// Resizable: default false
			column.Resizable = true;

			if (colNr != ColNr.Icon)
				column.SortColumnId = (int)colNr;

			treeviewCSums.AppendColumn (column);
			columnsDict.Add (column, colNr);
			return column;
		}

		TreeViewColumn AddHexColumn (string name, ColNr colNr)
		{
			TreeViewColumn col = new TreeViewColumn (name, cellRendererText, "text", (int)colNr);
			col.SetCellDataFunc (cellRendererText, TreeCellDataFuncHex);
			return col;
		}

		public void SetRom (Subaru.File.Rom rom)
		{
			if (rom == null)
				return;

			rcs = rom.RomChecksumming;
			var ilist = rcs.ReadTableRecords ();
			for (int i = 0; i < ilist.Count; i++) {
				var item = ilist[i];
				int sum = rcs.CalcChecksumValue (item);
				int iconIndex = item.Checksum == sum ? 1 : 0;
				store.AppendValues (i, item.StartAddress, item.EndAddress, item.Checksum, pixbufs[iconIndex], sum);
			}

			labelCVN8.Text = RomChecksumming.CVN8Str (rcs.CalcCVN8 ());
		}

		#region Tree Cell Data Functions

		// These should be fast as they are called a lot, even for measuring hidden columns.

		void TreeCellDataFuncHex (TreeViewColumn treeViewColumn, CellRenderer renderer, TreeModel treeModel, TreeIter iter)
		{
			// need col number to get value from store
			ColNr colNr = columnsDict[treeViewColumn];
			int nr = (int)store.GetValue (iter, (int)colNr);

			string formatStr;
			switch (colNr) {
			case ColNr.SumTable:
			case ColNr.SumCalc:
				formatStr = "X8";
				break;
			default:
				formatStr = "X";
				break;
			}
			cellRendererText.Text = nr.ToString (formatStr);
		}

		#endregion Tree Cell Data Functions
	}
}
