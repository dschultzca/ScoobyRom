// DataViewBaseGtk.cs: Base class for Gtk.TreeView UI.

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
using Subaru.Tables;

namespace ScoobyRom
{
	public abstract class DataViewBaseGtk
	{
		// needed model for ComboBox cells, sharable and const
		protected static readonly ListStore tableTypesModel = new ListStore (typeof(string));

		protected static readonly string[] AllowedHexPrefixes = new string[] { "0x", "$" };


		protected Gtk.TreeModel treeModel;
		protected Gtk.TreeView treeView;

		protected CellRendererText cellRendererText, cellRendererTextEditable;
		//, cellRendererTextMono;
		protected CellRendererToggle cellRendererToggle;
		protected CellRendererCombo cellRendererCombo;
		protected CellRendererPixbuf cellRendererPixbuf;

		// not worth taking enum, Gtk methods need int anyway
		protected readonly Dictionary<TreeViewColumn, int> columnsDict = new Dictionary<TreeViewColumn, int> (10);
		protected bool showIcons;

		static DataViewBaseGtk ()
		{
			// populate strings to be shown in ComboBox
			foreach (string item in TableTypes.GetStrings ()) {
				tableTypesModel.AppendValues (item);
			}
		}

		public DataViewBaseGtk ()
		{
		}

		#region Tree Cell Data Functions

		// These should be fast as they are called a lot, even for measuring hidden columns.

		protected void TreeCellDataFuncHex (TreeViewColumn treeViewColumn, CellRenderer renderer, TreeModel treeModel, TreeIter iter)
		{
			int nr = (int)treeModel.GetValue (iter, columnsDict[treeViewColumn]);
			cellRendererText.Text = nr.ToString ("X");
		}

		// Without own data function floats would be rendered like "100.000000"
		// ToString() only adds decimals where necessary - much better.
		protected void TreeCellDataFuncFloat (TreeViewColumn treeViewColumn, CellRenderer renderer, TreeModel treeModel, TreeIter iter)
		{
			float nr = (float)treeModel.GetValue (iter, columnsDict[treeViewColumn]);
			//((CellRendererText)renderer).Text = nr.ToString ();
			cellRendererText.Text = nr.ToString ();
		}

		protected void TreeCellDataFuncTableType (TreeViewColumn treeViewColumn, CellRenderer renderer, TreeModel treeModel, TreeIter iter)
		{
			TableType tt = (TableType)treeModel.GetValue (iter, columnsDict[treeViewColumn]);
			cellRendererCombo.Text = tt.ToStr ();
		}

		#endregion Tree Cell Data Functions


		#region CellRenderer event handlers


		protected void HandleCellRendererTextEditableEdited (object o, EditedArgs args)
		{
			TreeIter iter;
			if (treeModel.GetIter (out iter, new TreePath (args.Path))) {
				treeModel.SetValue (iter, CursorColNr, args.NewText);
				ScrollTo (iter);
			}
		}

		protected void CellRendererToggled (object o, ToggledArgs args)
		{
			int colNr = CursorColNr;
			TreeIter iter;
			if (treeModel.GetIter (out iter, new TreePath (args.Path))) {
				bool toggleOld = (bool)treeModel.GetValue (iter, colNr);
				treeModel.SetValue (iter, colNr, !toggleOld);
			}
		}

		protected void HandleCellRendererComboEdited (object o, EditedArgs args)
		{
			TreeIter iter;
			if (!treeModel.GetIter (out iter, new TreePath (args.Path)))
				return;
			TableType ttNew;
			if (!TableTypes.TryParse (args.NewText, out ttNew))
				return;
			int colNr = CursorColNr;

			// so far there's only ComboBox for TableType column
			TableType ttOld = (TableType)treeModel.GetValue (iter, colNr);
			if (ttOld != ttNew) {
				treeModel.SetValue (iter, colNr, (int)ttNew);
				OnTableTypeChanged (iter, ttNew);
				// follow it in case this column is being sorted
				ScrollTo (iter);
			}
		}

		protected abstract void OnTableTypeChanged (TreeIter iter, TableType newTableType);


		#endregion CellRenderer event handlers


		#region TreeView Search Functions

		// key = entered text in search (entry) widget

		// FALSE if the row matches, TRUE otherwise !!!
		protected static bool EqualFuncHex (string key, int content)
		{
			const System.Globalization.NumberStyles numberStyles = System.Globalization.NumberStyles.HexNumber;

			foreach (string prefix in AllowedHexPrefixes) {
				int index = key.IndexOf (prefix, StringComparison.InvariantCulture);
				if (index >= 0) {
					key = key.Substring (index + prefix.Length);
					break;
				}
			}
			int parsed;
			if (int.TryParse (key, numberStyles, System.Globalization.NumberFormatInfo.InvariantInfo, out parsed))
				return parsed != content;
			else
				return true;
		}

		// FALSE if the row matches, TRUE otherwise !!!
		protected static bool EqualFuncInt (string key, int content)
		{
			int searchNr;
			if (int.TryParse (key, out searchNr))
				return searchNr != content;
			else
				return true;
		}

		// FALSE if the row matches, TRUE otherwise !!!
		protected static bool EqualFuncFloat (string key, float content)
		{
			float searchNr;
			if (float.TryParse (key, out searchNr))
				return searchNr != content;
			else
				return true;
		}

		// like default behavior
		protected static bool EqualFuncString (string key, string content)
		{
			return !content.StartsWith (key, StringComparison.CurrentCultureIgnoreCase);
		}

		// FALSE if the row matches, TRUE otherwise !!!
		protected static bool EqualFuncTableType (string key, TableType content)
		{
			TableType parsed;
			if (TableTypes.TryParse (key, out parsed))
				return parsed != content;
			else
				return true;
		}

		#endregion TreeView Search Functions


		#region TreeView event handlers


		protected void OnCursorChanged (object obj, EventArgs e)
		{
			treeView.SearchColumn = CursorColNr;
		}

//		void HandleTreeViewKeyPressEvent (object o, KeyPressEventArgs args)
//		{
//			Gdk.Key key = args.Event.Key;
//			Console.WriteLine (key.ToString());
//			if (key == (Gdk.Key.p | Gdk.Key.Control_L))
//				Console.WriteLine ("p");
//		}

		#endregion TreeView event handlers


		protected TreeViewColumn GetColumn (int col)
		{
			foreach (var kvp in columnsDict) {
				if (kvp.Value == col)
					return kvp.Key;
			}
			return null;
			// LinQ would be overkill
			//var pair = columnsDict.Where (kvp => kvp.Value == col).First();
		}

		/// <summary>
		/// Scroll vertically to keep row in view when sorting is active and sorted column data changes.
		/// Otherwise would need to manually scroll in order bring it back into view.
		/// (TreePath usually changes, TreeIter does not.)
		/// </summary>
		/// <param name="iter">
		/// A <see cref="TreeIter"/>
		/// </param>
		protected void ScrollTo (TreeIter iter)
		{
			// ScrollToCell needs TreePath
			// If column is null, then no horizontal scrolling occurs.
			treeView.ScrollToCell (treeModel.GetPath (iter), null, false, 0, 0);
		}

		protected TreeViewColumn CursorColumn {
			get {
				TreePath path;
				TreeViewColumn column;
				treeView.GetCursor (out path, out column);
				return column;
			}
		}

		protected int CursorColNr {
			get { return columnsDict[CursorColumn]; }
		}

	}
}

