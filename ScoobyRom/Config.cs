// Config.cs: Settings, parses app.config.

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
using System.Collections.Specialized;

namespace ScoobyRom
{
	public static class Config
	{
		const string key_iconsOnByDefault = "iconsOnByDefault";
		// works on Linux at least, use this default name in case config entry cannot be found
		const string gnuplotDefaultPath = "gnuplot";

		static string platformStr = Environment.OSVersion.Platform.ToString ();
		static string gnuplotPath;
		static bool iconsOnByDefault;

		/// <summary>
		/// Null if key not found!
		/// </summary>
		public static string GnuplotPath {
			get { return gnuplotPath; }
		}

		public static string PlatformStr {
			get { return platformStr; }
		}

		public static bool IconsOnByDefault {
			get { return iconsOnByDefault; }
		}

		// should work even if .config file is missing
		static Config ()
		{
			// Get the AppSettings collection.
			// ConfigurationManager requires reference to System.Configuration.dll !
			NameValueCollection appSettings = System.Configuration.ConfigurationManager.AppSettings;
			// Value is null when key not found!

			gnuplotPath = appSettings["gnuplot_" + Environment.OSVersion.Platform.ToString ()];
			if (gnuplotPath == null)
				gnuplotPath = gnuplotDefaultPath;

			string val = appSettings[key_iconsOnByDefault];
			if (val != null)
				bool.TryParse (val, out iconsOnByDefault);
		}
	}
}
