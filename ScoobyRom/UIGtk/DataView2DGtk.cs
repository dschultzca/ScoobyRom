// DataView2DGtk.cs: Gtk.TreeView based UI.

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
using Subaru.Tables;

namespace ScoobyRom
{
	// TODO cleanup, sharing more code and objects
	// Code file for 3D is similar and has more comments.
	public sealed class DataView2DGtk : DataViewBaseGtk
	{
		public event EventHandler<ActionEventArgs> Activated;

		DataView2DModelGtk viewModel;


		private DataView2DGtk ()
		{
		}

		public DataView2DGtk (DataView2DModelGtk viewModel, TreeView treeView)
		{
			this.viewModel = viewModel;
			this.treeModel = viewModel.TreeModel;
			this.treeView = treeView;

			InitTreeView ();
		}

		public bool ShowIcons {
			get { return this.showIcons; }
			set {
				showIcons = value;
				GetColumn ((int)ColumnNr2D.Icon).Visible = value;
				if (value) {
					viewModel.RequestIcons ();
				}
			}
		}

		public Table2D Selected {
			get {
				Table2D table2D = null;
				TreeSelection selection = treeView.Selection;
				TreeModel model;
				TreeIter iter;

				if (selection.GetSelected (out model, out iter)) {
					table2D = (Table2D)model.GetValue (iter, (int)ColumnNr2D.Obj);
				}
				return table2D;
			}
		}

		void InitTreeView ()
		{
			InitCellRenderers ();

			#region Columns

			// TODO avoid reflection
			ColumnNr2D[] columns = (ColumnNr2D[])Enum.GetValues (typeof(ColumnNr2D));

			// must be appended/inserted in correct order
			foreach (ColumnNr2D colNr in columns) {
				TreeViewColumn column = CreateColumn (colNr);
				// null means column is not being used on view
				if (column == null)
					continue;

				columnsDict.Add (column, (int)colNr);

				if (column.SortColumnId < 0 && colNr != ColumnNr2D.Icon)
					column.SortColumnId = (int)colNr;

				column.Reorderable = true;
				column.Resizable = true;
			}

			#endregion Columns


			#region TreeView

			treeView.Selection.Mode = SelectionMode.Browse;
			treeView.RulesHint = true;
			treeView.EnableSearch = true;
			treeView.SearchColumn = (int)ColumnNr2D.Title;
			treeView.SearchEqualFunc = TreeViewSearchFunc;
			treeView.CursorChanged += OnCursorChanged;
			treeView.RowActivated += HandleTreeViewRowActivated;
			treeView.Model = treeModel;

			#endregion TreeView

		}

		void InitCellRenderers ()
		{
			cellRendererText = new CellRendererText ();

			cellRendererTextEditable = new CellRendererText ();
			cellRendererTextEditable.Editable = true;
			cellRendererTextEditable.Edited += HandleCellRendererTextEditableEdited;

			cellRendererToggle = new CellRendererToggle ();
			cellRendererToggle.Toggled += CellRendererToggled;

			cellRendererPixbuf = new CellRendererPixbuf ();

			cellRendererCombo = new CellRendererCombo ();
			cellRendererCombo.HasEntry = false;
			cellRendererCombo.Editable = true;
			cellRendererCombo.Model = tableTypesModel;
			cellRendererCombo.TextColumn = 0;
			cellRendererCombo.Edited += HandleCellRendererComboEdited;
		}

		TreeViewColumn CreateColumn (ColumnNr2D colNr)
		{
			TreeViewColumn col = null;
			switch (colNr) {
			case ColumnNr2D.Category:
				col = new TreeViewColumn ("Category", cellRendererTextEditable, "text", colNr);
				break;
			case ColumnNr2D.Toggle:
				col = new TreeViewColumn (null, cellRendererToggle, "active", colNr);
				break;
			case ColumnNr2D.Icon:
				col = new TreeViewColumn ("Icon", cellRendererPixbuf, "pixbuf", colNr);
				col.Visible = false;
				break;
			case ColumnNr2D.Title:
				col = new TreeViewColumn ("Title", cellRendererTextEditable, "text", colNr);
				break;
			case ColumnNr2D.Type:
				col = new TreeViewColumn ("Type", cellRendererCombo, "text", colNr);
				col.SetCellDataFunc (cellRendererCombo, TreeCellDataFuncTableType);
				break;
			case ColumnNr2D.NameX:
				col = new TreeViewColumn ("NameX", cellRendererTextEditable, "text", colNr);
				break;
			case ColumnNr2D.UnitX:
				col = new TreeViewColumn ("UnitX", cellRendererTextEditable, "text", colNr);
				break;
			case ColumnNr2D.UnitY:
				col = new TreeViewColumn ("UnitY", cellRendererTextEditable, "text", colNr);
				break;
			case ColumnNr2D.CountX:
				col = new TreeViewColumn ("Count", cellRendererText, "text", colNr);
				break;
			case ColumnNr2D.Xmin:
				col = new TreeViewColumn ("Xmin", cellRendererText, "text", colNr);
				col.SetCellDataFunc (cellRendererText, TreeCellDataFuncFloat);
				break;
			case ColumnNr2D.Xmax:
				col = new TreeViewColumn ("Xmax", cellRendererText, "text", colNr);
				col.SetCellDataFunc (cellRendererText, TreeCellDataFuncFloat);
				break;
			case ColumnNr2D.Ymin:
				col = new TreeViewColumn ("Ymin", cellRendererText, "text", colNr);
				col.SetCellDataFunc (cellRendererText, TreeCellDataFuncFloat);
				break;
			case ColumnNr2D.Yavg:
				col = new TreeViewColumn ("Yavg", cellRendererText, "text", colNr);
				col.SetCellDataFunc (cellRendererText, TreeCellDataFuncFloat);
				break;
			case ColumnNr2D.Ymax:
				col = new TreeViewColumn ("Ymax", cellRendererText, "text", colNr);
				col.SetCellDataFunc (cellRendererText, TreeCellDataFuncFloat);
				break;
			case ColumnNr2D.Location:
				col = new TreeViewColumn ("Record", cellRendererText, "text", colNr);
				col.SetCellDataFunc (cellRendererText, TreeCellDataFuncHex);
				break;
			case ColumnNr2D.YPos:
				col = new TreeViewColumn ("YPos", cellRendererText, "text", colNr);
				col.SetCellDataFunc (cellRendererText, TreeCellDataFuncHex);
				break;
			case ColumnNr2D.Description:
				col = new TreeViewColumn ("Description", cellRendererTextEditable, "text", colNr);
				break;
			}
			if (col != null)
				treeView.AppendColumn (col);
			return col;
		}


		// It seems if using own TreeViewSearchEqualFunc one cannot use default function anymore.
		// Function getter returns null and null is not allowed.
		// Workaround: define own functions for all needed column types.

		// FALSE if the row does MATCH, otherwise true !!!
		bool TreeViewSearchFunc (TreeModel model, int column, string key, TreeIter iter)
		{
			object content = model.GetValue (iter, column);

			GLib.GType gt = model.GetColumnType (column);
			if (gt == GLib.GType.Float)
				return EqualFuncFloat (key, (float)content); else if (gt == GLib.GType.String)
				return EqualFuncString (key, (string)content);

			// type int needs further info!
			switch ((ColumnNr2D)column) {
			case ColumnNr2D.Location:
			case ColumnNr2D.YPos:
				return EqualFuncHex (key, (int)content);
			case ColumnNr2D.Type:
				return EqualFuncTableType (key, (TableType)content);
			case ColumnNr2D.CountX:
				return EqualFuncInt (key, (int)content);
			default:
				// cannot search on icon column, must signal true = no match.
				return true;
			}
		}

		#region event handlers


		#region TreeView event handlers

		// double click or Enter key
		void HandleTreeViewRowActivated (object o, RowActivatedArgs args)
		{
			Table2D table2D = Selected;
			if (table2D != null && Activated != null) {
				Activated (this, new ActionEventArgs (table2D));
			}
		}

		#endregion TreeView event handlers


		#endregion event handlers


		protected override void OnTableTypeChanged (TreeIter iter, TableType newTableType)
		{
			Table2D table2D = (Table2D)treeModel.GetValue (iter, (int)ColumnNr2D.Obj);
			viewModel.ChangeTableType (table2D, newTableType);
			viewModel.SetNodeContentTypeChanged (iter, table2D);
		}
	}
}
