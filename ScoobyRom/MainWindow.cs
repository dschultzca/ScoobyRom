// MainWindow.cs: Main application window user interface.

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


// can be useful for testing, may be broken
//#define LOAD_SYNC

using System;
using System.Threading.Tasks;
using Gtk;
using ScoobyRom;
using Subaru.Tables;

public partial class MainWindow : Gtk.Window
{
	enum ActiveUI
	{
		Undefined,
		View2D,
		View3D
	}

	const string appName = "ScoobyRom";

	readonly Data data = new Data ();

	readonly DataView3DModelGtk dataView3DModelGtk;
	readonly DataView3DGtk dataView3DGtk;

	readonly DataView2DModelGtk dataView2DModelGtk;
	readonly DataView2DGtk dataView2DGtk;

	// Gtk# integration: NPlot.Gtk.PlotSurface2D instead of generic NPlot.PlotSurface2D
	readonly NPlot.Gtk.NPlotSurface2D plotSurface = new NPlot.Gtk.NPlotSurface2D ();
	readonly Plot2D plot2D;

	// const so far, so share it
	static readonly Util.Coloring coloring = new Util.Coloring ();

	// measuring load/search performance
	readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch ();

	// Remember output folder for convenience. No need to save it in app.config?
	string svgDirectory = null;

	public MainWindow (string[] args) : base(Gtk.WindowType.Toplevel)
	{
		// Execute Gtk# visual designer generated code (MonoDevelop http://monodevelop.com/ )
		// Obviously, Visual Studio doesn't have a Gtk# designer, you'll have to code all UI stuff by yourself.
		// Compiling existing generated UI code within Visual Studio does work however.
		Build ();

		this.Icon = MainClass.AppIcon;

		dataView3DModelGtk = new DataView3DModelGtk (this.data);
		dataView3DGtk = new DataView3DGtk (dataView3DModelGtk, treeview3D);
		dataView3DGtk.Activated += delegate(object sender, ActionEventArgs e) {
			Table3D table3D = (Table3D)e.Tag;
			if (table3D != null) {
				this.Show3D (table3D);
			}
		};

		dataView2DModelGtk = new DataView2DModelGtk (this.data);
		dataView2DGtk = new DataView2DGtk (dataView2DModelGtk, treeview2D);
		dataView2DGtk.Activated += delegate(object sender, ActionEventArgs e) {
			Table2D table2D = (Table2D)e.Tag;
			if (table2D != null) {
				this.Show2D (table2D);
			}
		};

		plot2D = new Plot2D (plotSurface);
		this.vpaned2D.Add2 (plotSurface);
		global::Gtk.Paned.PanedChild pc = ((global::Gtk.Paned.PanedChild)(this.vpaned2D[plotSurface]));
		// to resize both panes proportionally when parent (main window) resizes
		pc.Resize = false;
		//		pc.Shrink = false;
		this.vpaned2D.ShowAll ();

		this.notebook1.Page = 1;

		if (Config.IconsOnByDefault) {
			iconsAction.Active = true;
			dataView2DGtk.ShowIcons = true;
			dataView3DGtk.ShowIcons = true;
		}

		// program arguments: first argument is ROM path to auto-load
		if (args != null && args.Length > 0 && !string.IsNullOrEmpty (args[0])) {
			OpenRom (args[0]);
		}
	}

	ActiveUI CurrentUI {
		get {
			if (notebook1.CurrentPageWidget == vpaned2D)
				return ActiveUI.View2D; else if (notebook1.CurrentPageWidget == vpaned3D)
				return ActiveUI.View3D;
			else
				return ActiveUI.Undefined;
		}
	}

	void OpenRom (string path)
	{
		this.progressbar1.Adjustment.Lower = 0;
		this.progressbar1.Adjustment.Upper = 100;
		//this.progressbar1.Adjustment.StepIncrement = 5;
		this.progressbar1.Adjustment.Value = 0;

		data.ProgressChanged += delegate(object s, System.ComponentModel.ProgressChangedEventArgs pArgs) { Application.Invoke (delegate { this.progressbar1.Adjustment.Value = pArgs.ProgressPercentage; }); };

		this.statusbar1.Push (0, "Analyzing file " + System.IO.Path.GetFileName (path));

		#if LOAD_SYNC

		LoadRomTask (path);
		//LoadRomDone (new Task((t) => Console.WriteLine ("LoadRomDone")));

		#else

		Task task = new Task (() => LoadRomTask (path));
		// Exceptions must be handled in Task
		task.ContinueWith (t => LoadRomDone (t));

		task.Start ();

		#endif
	}

	void LoadRomTask (string path)
	{
		Application.Invoke (delegate {
			this.openAction.Sensitive = false;
			this.statusbar1.Show ();
		});

		stopwatch.Reset ();
		stopwatch.Start ();
		data.LoadRom (path);
	}

	void LoadRomDone (Task t)
	{
		stopwatch.Stop ();

		Application.Invoke (delegate {
			if (t.Status == TaskStatus.Faulted) {
				Console.Error.WriteLine ("Exception loading ROM:");
				Console.Error.WriteLine (t.Exception.ToString ());
			} else {
				SetWindowTitle ();
				SetActionsSensitiveForRomLoaded (true);

				string txt = "Search took " + stopwatch.ElapsedMilliseconds.ToString () + " ms";
				this.progressbar1.Text = txt;
				this.statusbar1.Push (0, "Updating UI ...");
				DoPendingEvents ();
				Console.WriteLine (txt);

				data.UpdateUI ();
			}

			this.statusbar1.Hide ();
			this.statusbar1.Pop (0);
			this.progressbar1.Text = string.Empty;
		});
	}

	static void DoPendingEvents ()
	{
		while (Application.EventsPending ()) {
			Application.RunIteration ();
		}
	}

	void SetWindowTitle ()
	{
		this.Title = data.RomLoaded ? string.Format ("{0} - {1}", appName, data.CalID) : appName;
	}

	void Show3D (Table3D table)
	{
		if (table == null)
			return;
		var valuesZ = table.GetValuesZasFloats ();
		var tableUI = new GtkWidgets.TableWidget (coloring, table.ValuesX, table.ValuesY, valuesZ, table.Zmin, table.Zmax);
		tableUI.TitleMarkup = GtkWidgets.TableWidget.MakeTitleMarkup (table.Title, table.UnitZ);
		tableUI.AxisMarkupX = GtkWidgets.TableWidget.MakeMarkup (table.NameX, table.UnitX);
		tableUI.AxisMarkupY = GtkWidgets.TableWidget.MakeMarkup (table.NameY, table.UnitY);
		// HACK FormatValues
		tableUI.FormatValues = table.Zmax < 30 ? "0.00" : "0.0";
		if (table.Zmax < 10)
			tableUI.FormatValues = "0.000";

		// Viewport needed for ScrolledWindow to work as generated table widget has no scroll support
		var viewPort = new Gtk.Viewport ();
		viewPort.Add (tableUI.Create ());

		Gtk.Widget previous = this.scrolledwindowTable3D.Child;
		if (previous != null)
			this.scrolledwindowTable3D.Remove (previous);
		// previous.Dispose () or previous.Destroy () cause NullReferenceException!

		this.scrolledwindowTable3D.Add (viewPort);
		this.scrolledwindowTable3D.ShowAll ();
	}

	void Show2D (Table2D table)
	{
		if (table == null)
			return;
		plot2D.Draw (table);
		plotSurface.Refresh ();
	}


	#region UI Events

	void OnNotebook1SwitchPage (object o, Gtk.SwitchPageArgs args)
	{
		bool iconsActive = false;
		switch (CurrentUI) {
		case ActiveUI.View2D:
			iconsActive = dataView2DGtk.ShowIcons;
			break;
		case ActiveUI.View3D:
			iconsActive = dataView3DGtk.ShowIcons;
			break;
		}
		iconsAction.Active = iconsActive;
	}

	void OnVisualizationAction (object sender, System.EventArgs e)
	{
		if (!data.RomLoaded)
			return;

		// Selection can be null - no row selected yet!
		switch (CurrentUI) {
		case ActiveUI.View2D:
			Show2D (dataView2DGtk.Selected);
			break;
		case ActiveUI.View3D:
			Show3D (dataView3DGtk.Selected);
			break;
		}
	}

	void OnAbout (object sender, System.EventArgs e)
	{
		const string LicensePath = "COPYING.txt";

		System.Version version = System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Version;
		string appVersion = string.Format ("{0}.{1}.{2}", version.Major.ToString (), version.Minor.ToString (), version.Build.ToString ());

		AboutDialog about = new AboutDialog { ProgramName = appName, Version = appVersion,
			Copyright = "© 2011 SubaruDieselCrew",
			Authors = new string[] { "subdiesel\thttp://subdiesel.wordpress.com",
				"\nThanks for any feedback!",
				"\nEXTERNAL BINARY DEPENDENCIES:",
				"Gtk#\thttp://mono-project.com/GtkSharp",
				"NPlot\thttp://netcontrols.org/nplot/wiki/",
				"gnuplot\thttp://www.gnuplot.info/",
				},
			WrapLicense = true };
		about.Icon = about.Logo = MainClass.AppIcon;
		about.Comments = "License: GPL v3";

		try {
			about.License = System.IO.File.ReadAllText (LicensePath);
		} catch (System.IO.FileNotFoundException) {
			about.License = "Could not load license file '" + LicensePath + "'.\nGo to http://www.fsf.org";
		}

		about.Run ();
		about.Destroy ();
	}

	void OnOpenActionActivated (object sender, System.EventArgs e)
	{
		Gtk.FileChooserDialog fc = new Gtk.FileChooserDialog ("Choose ROM file to open", this, FileChooserAction.Open, Gtk.Stock.Cancel, ResponseType.Cancel, Gtk.Stock.Open, ResponseType.Accept);

		Gtk.FileFilter ff;
		ff = new FileFilter { Name = "ROM, BIN, HEX files" };
		ff.AddPattern ("*.rom");
		ff.AddPattern ("*.bin");
		ff.AddPattern ("*.hex");
		fc.AddFilter (ff);

		ff = new FileFilter { Name = "All files" };
		ff.AddPattern ("*");
		fc.AddFilter (ff);

		ResponseType response = (ResponseType)fc.Run ();
		string path = fc.Filename;
		fc.Destroy ();

		if (response == ResponseType.Accept) {
			try {
				OpenRom (path);
			} catch (System.Exception ex) {
				ErrorMsg ("Error opening file", ex.Message);
			}
		}
	}

	void OnSaveActionActivated (object sender, System.EventArgs e)
	{
		// TODO consider over-write warning for first time save
		try {
			data.SaveXml ();
		} catch (System.Exception ex) {
			ErrorMsg ("Error saving file", ex.Message);
		}
	}

	void OnExportAsRRActionActivated (object sender, System.EventArgs e)
	{
		string pathSuggested = ScoobyRom.Data.PathWithNewExtension (data.Rom.Path, ".RR.xml");
		var fc = new Gtk.FileChooserDialog ("Export as RomRaider definition file", this, FileChooserAction.Save, Gtk.Stock.Cancel, ResponseType.Cancel, Gtk.Stock.Save, ResponseType.Accept);
		try {
			FileFilter filter = new FileFilter ();
			filter.Name = "XML files";
			filter.AddPattern ("*.xml");
			// would show other XML files like .svg (on Linux at least): filter.AddMimeType ("text/xml");
			fc.AddFilter (filter);

			filter = new FileFilter ();
			filter.Name = "All files";
			filter.AddPattern ("*");
			fc.AddFilter (filter);

			fc.DoOverwriteConfirmation = true;
			fc.CurrentName = pathSuggested;
			if (fc.Run () == (int)ResponseType.Accept) {
				data.SaveAsRomRaiderXml (fc.Filename);
			}
		} catch (Exception ex) {
			ErrorMsg ("Error writing file", ex.Message);
		} finally {
			// Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
			if (fc != null)
				fc.Destroy ();
		}
	}

	// closing main app window
	void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	void OnQuitActionActivated (object sender, System.EventArgs e)
	{
		OnDeleteEvent (this, new DeleteEventArgs ());
	}

	// icons ON/OFF
	void OnIconsActionActivated (object sender, System.EventArgs e)
	{
		bool iconsActive = iconsAction.Active;
		switch (CurrentUI) {
		case ActiveUI.View2D:
			dataView2DGtk.ShowIcons = iconsActive;
			break;
		case ActiveUI.View3D:
			dataView3DGtk.ShowIcons = iconsActive;
			break;
		}
	}

	// create or close gnuplot window
	void OnPlotActionActivated (object sender, System.EventArgs e)
	{
		try {
			// gnuplot process itself can be slow to startup
			// so this does not prevent closing it immediatly when pressed twice
			//plotExternalAction.Sensitive = false;

			switch (CurrentUI) {
			case ActiveUI.View2D:
				Table2D table2D = dataView2DGtk.Selected;
				if (table2D != null) {
					GnuPlot.ToggleGnuPlot (table2D);
				}
				break;
			case ActiveUI.View3D:
				Table3D table3D = dataView3DGtk.Selected;
				if (table3D != null) {
					GnuPlot.ToggleGnuPlot (table3D);
				}
				break;
			}
		} catch (GnuPlotProcessException ex) {
			Console.Error.WriteLine (ex);
			ErrorMsg ("Error launching gnuplot!", ex.Message + "\n\nHave you installed gnuplot?" + "\nYou also may need to edit file '" + appName + ".exe.config'." + "\nCurrent platform-ID is '" + System.Environment.OSVersion.Platform.ToString () + "'." + "\nSee 'README.txt' for details.");
		} catch (GnuPlotException ex) {
			Console.Error.WriteLine (ex);
			ErrorMsg ("Error launching gnuplot!", ex.Message);
		}
	}

	// depends on gnuplot
	void OnCreateSVGFileActionActivated (object sender, System.EventArgs e)
	{
		if (data.RomLoaded == false)
			return;

		Subaru.Tables.Table table = null;
		switch (CurrentUI) {
		case ActiveUI.View2D:
			table = dataView2DGtk.Selected;
			break;
		case ActiveUI.View3D:
			table = dataView3DGtk.Selected;
			break;
		}
		if (table == null)
			return;

		GnuPlot gnuPlot = GnuPlot.GetExistingGnuPlot (table);
		if (gnuPlot == null) {
			ErrorMsg ("Error creating SVG export", "Need existing gnuplot window. Do a normal plot first.");
			return;
		}

		string filenameSuggested = string.IsNullOrEmpty (table.Title) ? "plot" : table.Title;
		filenameSuggested += ".svg";
		if (svgDirectory == null && data.Rom.Path != null)
			svgDirectory = System.IO.Path.GetDirectoryName (data.Rom.Path);

		var fc = new Gtk.FileChooserDialog ("Export plot as SVG file", this, FileChooserAction.Save, Gtk.Stock.Cancel, ResponseType.Cancel, Gtk.Stock.Save, ResponseType.Accept);
		try {
			FileFilter filter = new FileFilter ();
			filter.Name = "SVG files";
			filter.AddPattern ("*.svg");
			fc.AddFilter (filter);

			filter = new FileFilter ();
			filter.Name = "All files";
			filter.AddPattern ("*");
			fc.AddFilter (filter);

			fc.DoOverwriteConfirmation = true;
			fc.SetCurrentFolder (svgDirectory);
			fc.CurrentName = filenameSuggested;
			if (fc.Run () == (int)ResponseType.Accept) {
				GnuPlot.CreateSVG (table, fc.Filename);
			}
			// remember used dir
			svgDirectory = System.IO.Path.GetDirectoryName (fc.Filename);
		} catch (GnuPlotException ex) {
			ErrorMsg ("Error creating SVG file", ex.Message);
		} catch (System.IO.IOException ex) {
			ErrorMsg ("IO Exception", ex.Message);
		} catch (Exception ex) {
			// Access to path denied...
			ErrorMsg ("Error", ex.Message);
		} finally {
			// Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
			if (fc != null)
				fc.Destroy ();
		}
	}

	void OnROMChecksumsActionActivated (object sender, System.EventArgs e)
	{
		// sharing internal FileStream won't work, could use another one though
		if (!data.RomLoaded)
			return;

		// Use a Window (not Dialog) to allow working in main window with new window still open.
		// Also supports multiple monitors.
		var d = new ChecksumWindow ();
		d.SetRom (data.Rom);
		d.Show ();
	}

	void OnStatisticsWindowActionActivated (object sender, System.EventArgs e)
	{
		if (!data.RomLoaded)
			return;

		var d = new StatisticsWindow (data);
		d.Show ();
	}

	void OnExportTableAsCSVActionActivated (object sender, System.EventArgs e)
	{
		if (data.RomLoaded == false)
			return;

		Subaru.Tables.Table table = null;
		switch (CurrentUI) {
		case ActiveUI.View2D:
			table = dataView2DGtk.Selected;
			break;
		case ActiveUI.View3D:
			ErrorMsg ("Error", "Creating CSV for 3D table not implemented yet.");
			return;
			//table = dataView3DGtk.Selected;
			//break;
		}
		if (table == null)
			return;

		string filenameSuggested = string.IsNullOrEmpty (table.Title) ? "table" : table.Title;
		filenameSuggested += ".csv";
		// TODO another var to remember export dir
		if (svgDirectory == null && data.Rom.Path != null)
			svgDirectory = System.IO.Path.GetDirectoryName (data.Rom.Path);

		var fc = new Gtk.FileChooserDialog ("Export data as CSV file", this, FileChooserAction.Save, Gtk.Stock.Cancel, ResponseType.Cancel, Gtk.Stock.Save, ResponseType.Accept);
		try {
			FileFilter filter = new FileFilter ();
			filter.Name = "CSV files";
			filter.AddPattern ("*.csv");
			fc.AddFilter (filter);

			filter = new FileFilter ();
			filter.Name = "All files";
			filter.AddPattern ("*");
			fc.AddFilter (filter);

			fc.DoOverwriteConfirmation = true;
			fc.SetCurrentFolder (svgDirectory);
			fc.CurrentName = filenameSuggested;
			if (fc.Run () == (int)ResponseType.Accept) {
				using (System.IO.StreamWriter sw = new System.IO.StreamWriter (fc.Filename, false, System.Text.Encoding.UTF8)) {
					((Table2D)table).WriteCSV (sw);
				}
			}
			// remember used dir
			svgDirectory = System.IO.Path.GetDirectoryName (fc.Filename);
		} catch (GnuPlotException ex) {
			ErrorMsg ("Error creating CSV file", ex.Message);
		} catch (System.IO.IOException ex) {
			ErrorMsg ("IO Exception", ex.Message);
		} catch (Exception ex) {
			// Access to path denied...
			ErrorMsg ("Error", ex.Message);
		} finally {
			// Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
			if (fc != null)
				fc.Destroy ();
		}
	}

	#endregion UI Events

	void SetActionsSensitiveForRomLoaded (bool sensitive)
	{
		openAction.Sensitive = sensitive;
		saveAction.Sensitive = sensitive;
		exportAsAction.Sensitive = sensitive;
		exportAsRRAction.Sensitive = sensitive;

		visualizationAction.Sensitive = sensitive;
		iconsAction.Sensitive = sensitive;
		checksumWindowAction.Sensitive = sensitive;
		statisticsWindowAction.Sensitive = sensitive;

		plotExternalAction.Sensitive = sensitive;
		createSVGFileAction.Sensitive = sensitive;

		exportTableAsCSVAction.Sensitive = sensitive;
	}

	/// <summary>
	/// Displays a simple MessageDialog with error-icon and close-button.
	/// </summary>
	/// <param name="text">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="title">
	/// A <see cref="System.String"/>
	/// </param>
	void ErrorMsg (string title, string text)
	{
		MessageDialog md = new MessageDialog (this, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, text);
		md.Title = title;
		md.Run ();
		md.Destroy ();
	}
}
