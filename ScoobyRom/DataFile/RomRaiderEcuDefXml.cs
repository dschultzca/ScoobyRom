// RomRaiderEcuDefXml.cs: Export data in RomRaider ECu definition format.

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


using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Subaru.Tables;

namespace Subaru.File
{
	public static class RomRaiderEcuDefXml
	{
		public static void WriteRRXmlFile (string path, XElement romid, IList<Table2D> list2D, IList<Table3D> list3D)
		{
			XmlTextWriter xw = new XmlTextWriter (path, System.Text.Encoding.UTF8);
			// necessary, otherwise single line
			xw.Formatting = Formatting.Indented;

			var l2D = list2D == null ? null : list2D.Select (t => t.RRXml ());
			var l3D = list3D == null ? null : list3D.Select (t => t.RRXml ());

			XDocument doc = RRXmlDocument (new XElement ("rom", romid, l2D, l3D));

			doc.WriteTo (xw);
			xw.Close ();
		}

		public static XDocument RRXmlDocument (params object[] content)
		{
			// XDeclaration: null parameters --> "<?xml version="1.0" encoding="utf-8"?>"
			return new XDocument (new XDeclaration (null, null, null), new XComment ("RomRaider ECU definition file"), new XElement ("roms", content));
		}

		public static XElement RomID (string xmlid, int internalidaddress, string internalidstring, string ecuid, string year, string market, string make, string model, string submodel, string transmission,
		string memmodel, string flashmethod, int filesize)
		{
			// TODO check if RR supports unambiguous designations "KiB", "MiB"
			int fSize = filesize;
			string postfix = null;
			if (fSize % 1024 == 0) {
				fSize = fSize / 1024;
				postfix = "KB";
			}
			if (fSize % 1024 == 0) {
				fSize = fSize / 1024;

				postfix = "MB";
			}
			return new XElement ("romid", new XElement ("xmlid", xmlid), new XElement ("internalidaddress", internalidaddress.ToString ("X")), new XElement ("internalidstring", internalidstring), new XElement ("ecuid", ecuid), new XElement ("year", year), new XElement ("market", market), new XElement ("make", make), new XElement ("model", model), new XElement ("submodel", submodel),
			new XElement ("transmission", transmission), new XElement ("memmodel", memmodel), new XElement ("flashmethod", flashmethod), new XElement ("filesize", postfix != null ? fSize.ToString () + postfix : fSize.ToString ()));
		}
	}
}
