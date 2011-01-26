// Main.cs: Program entry point.

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

namespace ScoobyRom
{
	static class MainClass
	{
		static Gdk.Pixbuf appIcon;

		// sharing, used on all sub-windows
		static internal Gdk.Pixbuf AppIcon {
			get { return appIcon; }
		}

		public static void Main (string[] args)
		{
			Application.Init ();

			// does not catch Exceptions in MainWindow constructor!
			GLib.ExceptionManager.UnhandledException += OnUnhandledException;

			try {
				// GLib system must have been initialized already for this to work
				appIcon = Gdk.Pixbuf.LoadFromResource ("Images.AppIcon.png");
			} catch (System.ArgumentException) {
				// i.e. resource not found
			}

			// program arguments: if available, first argument is supposed to be ROM path
			MainWindow win = new MainWindow (args);
			win.Show ();

			Application.Run ();
		}

		static void OnUnhandledException (GLib.UnhandledExceptionArgs args)
		{
			var ex = (Exception)args.ExceptionObject;
			string txt = "GLib.UnhandledException:\n" + ex.Message + "\n\n" + ex.ToString ();
			Console.Error.WriteLine (txt);
			ErrorMsg ("Serious Error. Select & copy text.", txt);

			args.ExitApplication = true;
		}

		static void ErrorMsg (string title, string text)
		{
			MessageDialog md = new MessageDialog (null, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, null);
			md.Title = title;
			md.Text = text;
			md.Run ();
			md.Destroy ();
		}
	}
}
