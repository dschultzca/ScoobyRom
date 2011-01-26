// RomXml.cs: Read/write ScoobyRom XML format, merge data.

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


// TODO cleanup, reuse common XML methods
// TODO UI feedback hooks instead of terminal output

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Subaru.Tables;

namespace Subaru.File
{
	public sealed class RomXml
	{
		const string X_table2D = "table2D";
		const string X_table3D = "table3D";
		const string X_rom = "rom";
		const string X_name = "name";
		const string X_category = "category";
		const string X_axisX = "axisX";
		const string X_axisY = "axisY";
		const string X_values = "values";
		const string X_address = "storageaddress";
		const string X_tableType = "storagetype";
		const string X_unit = "unit";
		const string X_description = "description";
		const string X_tableSearch = "tableSearch";
		const string X_tableSearchStart = "start";
		const string X_tableSearchEnd = "end";

		// for parsing integers, cannot use const
		static readonly System.Globalization.NumberFormatInfo NumberFormatInfoInvariant = System.Globalization.NumberFormatInfo.InvariantInfo;

		RomMetadata romMetadata;
		// needed for reloading value data in case of a changed TableType
		System.IO.Stream romStream;

		// table objects in here will only contain parsed metadata for merging into real data objects
		List<Table2D> xml2D = new List<Table2D> (0);
		List<Table3D> xml3D = new List<Table3D> (0);
		int tableSearchStart, tableSearchEnd;

		public RomMetadata RomMetadata {
			get { return romMetadata; }
		}

		/// <summary>
		/// Needed for loading (merging) as changing to a different TableTable needs values reload from ROM.
		/// </summary>
		public System.IO.Stream RomStream {
			get { return this.romStream; }
			set { romStream = value; }
		}

		public int TableSearchEnd {
			get { return tableSearchEnd; }
			set { tableSearchEnd = value; }
		}

		public int TableSearchStart {
			get { return tableSearchStart; }
			set { tableSearchStart = value; }
		}

		public RomXml ()
		{
		}

		public void Load (string path)
		{
			XDocument doc = XDocument.Load (path, LoadOptions.SetLineInfo);
			xml2D = new List<Table2D> (40);
			xml3D = new List<Table3D> (40);
			ParseXml (doc.Root);
		}

		public void TryMergeWith (IList<Table2D> toUpdate)
		{
			// TODO improve merging intelligence, currently uses record location exclusively
			Console.WriteLine ("Merging " + this.xml2D.Count.ToString () + " 2D XML items");
			int count = 0;
			foreach (Table2D table in xml2D) {
				Table2D found = null;
				int v;
				v = table.Location;
				if (v > 0) {
					found = toUpdate.Where (t => t.Location == v).FirstOrDefault ();
				}
				// could add further match checking
				if (found != null) {
					Merge (found, table);
					++count;
				} else
					Console.Error.WriteLine ("Could not find this Table2D from XML: " + table.ToString ());
			}
		}

		public void TryMergeWith (IList<Table3D> toUpdate)
		{
			// TODO improve merging intelligence, currently uses record location exclusively
			Console.WriteLine ("Merging " + this.xml3D.Count.ToString () + " 3D XML items");
			int count = 0;
			foreach (Table3D table in xml3D) {
				Table3D found = null;
				int v;
				v = table.Location;
				if (v > 0) {
					found = toUpdate.Where (t => t.Location == v).FirstOrDefault ();
				}
				if (found != null) {
					Merge (found, table);
					++count;
				} else
					Console.Error.WriteLine ("Could not find this Table3D from XML: " + table.ToString ());
			}
		}

		void MergeCommon (Table original, Table newTable)
		{
			// ref not possible because of properties
			original.Category = UpdateString (original.Category, newTable.Category);
			original.Title = UpdateString (original.Title, newTable.Title);
			original.Description = UpdateString (original.Description, newTable.Description);

			original.NameX = UpdateString (original.NameX, newTable.NameX);

			original.UnitX = UpdateString (original.UnitX, newTable.UnitX);
			original.UnitY = UpdateString (original.UnitY, newTable.UnitY);

			if (original.TableType != newTable.TableType)
				original.ChangeTypeToAndReload (newTable.TableType, romStream);
		}

		void Merge (Table2D original, Table2D newTable)
		{
			MergeCommon (original, newTable);
		}

		void Merge (Table3D original, Table3D newTable)
		{
			MergeCommon (original, newTable);

			original.NameY = UpdateString (original.NameY, newTable.NameY);
			original.UnitZ = UpdateString (original.UnitZ, newTable.UnitZ);
		}

		static string UpdateString (string original, string update)
		{
			if (!string.IsNullOrEmpty (update)) {
				return update;
			}
			return original;
		}

		void ParseXml (XElement root)
		{
			romMetadata = RomMetadata.FromXML (root.Element ("romid"));

			ParseTableSearch (root.Element (X_tableSearch));

			foreach (XElement el in root.Elements ()) {
				if (el.Name == X_table2D)
					xml2D.Add (ParseTable2D (el)); else if (el.Name == X_table3D)
					xml3D.Add (ParseTable3D (el));
			}
		}

		void ParseTableSearch (XElement el)
		{
			if (el == null)
				return;

			XAttribute at = el.Attribute (X_tableSearchStart);
			tableSearchStart = ParseHexInt ((string)at, at);
			at = el.Attribute (X_tableSearchEnd);
			tableSearchEnd = ParseHexInt ((string)at, at);
		}

		XElement TableSearchXElement ()
		{
			return new XElement (X_tableSearch, new XAttribute (X_tableSearchStart, HexNum (TableSearchStart)), new XAttribute (X_tableSearchEnd, HexNum (TableSearchEnd)));
		}

		public void WriteXml (string path, RomMetadata romMetadata, IList<Table2D> list2D, IList<Table3D> list3D)
		{
			XmlTextWriter xw = new XmlTextWriter (path, System.Text.Encoding.UTF8);
			// necessary, otherwise single line
			xw.Formatting = Formatting.Indented;

			var table2DXElements = list2D.Where (t => t.HasMetadata).Select (t => GetXElement (t)).AsParallel ();
			var table3DXElements = list3D.Where (t => t.HasMetadata).Select (t => GetXElement (t)).AsParallel ();

			XElement romEl = new XElement (X_rom, romMetadata.XElement, TableSearchXElement (), table2DXElements, table3DXElements);

			XDocument doc = XDoc (romEl);

			doc.WriteTo (xw);
			xw.Close ();
		}

		static void ParseCommon (XElement el, Table table)
		{
			// allow null values here when attributes don't exist
			table.Category = (string)el.Attribute (X_category);
			table.Title = (string)el.Attribute (X_name);

			XAttribute attr = el.Attribute (X_address);
			if (attr != null)
				table.Location = ParseHexInt ((string)attr, attr);
		}

		static Table2D ParseTable2D (XElement el)
		{
			Table2D table2D = new Table2D ();
			ParseCommon (el, table2D);

			int? address;
			string name, unit;
			XElement subEl;
			subEl = el.Element (X_axisX);
			if (subEl != null) {
				ParseAxis (subEl, out address, out name, out unit);
				table2D.NameX = name;
				table2D.UnitX = unit;
				if (address.HasValue)
					table2D.RangeX = new Util.Range (address.Value, 0);
			}

			subEl = el.Element (X_values);
			if (subEl != null) {
				TableType? tableType;
				ParseValues (subEl, out address, out unit, out tableType);
				table2D.UnitY = unit;
				if (address.HasValue)
					table2D.RangeY = new Util.Range (address.Value, 0);
				if (tableType.HasValue)
					table2D.TableType = tableType.Value;
			}

			table2D.Description = (string)el.Element (X_description);

			return table2D;
		}

		static Table3D ParseTable3D (XElement el)
		{
			Table3D table3D = new Table3D ();
			ParseCommon (el, table3D);

			int? address;
			string name, unit;
			XElement subEl;
			subEl = el.Element (X_axisX);
			if (subEl != null) {
				ParseAxis (subEl, out address, out name, out unit);
				table3D.NameX = name;
				table3D.UnitX = unit;
				if (address.HasValue)
					table3D.RangeX = new Util.Range (address.Value, 0);
			}

			subEl = el.Element (X_axisY);
			if (subEl != null) {
				ParseAxis (subEl, out address, out name, out unit);
				table3D.NameY = name;
				table3D.UnitY = unit;
				if (address.HasValue)
					table3D.RangeY = new Util.Range (address.Value, 0);
			}

			subEl = el.Element (X_values);
			if (subEl != null) {
				TableType? tableType;
				ParseValues (subEl, out address, out unit, out tableType);
				table3D.UnitZ = unit;
				if (address.HasValue)
					table3D.RangeZ = new Util.Range (address.Value, 0);
				if (tableType.HasValue)
					table3D.TableType = tableType.Value;
			}

			table3D.Description = (string)el.Element (X_description);

			return table3D;
		}

		static void ParseAxis (XElement el, out int? address, out string name, out string unit)
		{
			address = null;
			name = null;
			unit = null;

			XAttribute attr = el.Attribute (X_address);
			if (attr != null)
				address = ParseHexInt ((string)attr, attr);
			name = (string)el.Attribute (X_name);
			unit = (string)el.Attribute (X_unit);
		}

		static void ParseValues (XElement el, out int? address, out string unit, out TableType? tableType)
		{
			address = null;
			unit = null;
			tableType = null;

			XAttribute attr = el.Attribute (X_address);
			if (attr != null)
				address = ParseHexInt ((string)attr, attr);

			attr = el.Attribute (X_tableType);
			if (attr != null) {
				TableType parsedType;
				if (TableTypes.TryParse ((string)attr, out parsedType))
					tableType = parsedType;
			}

			unit = (string)el.Attribute (X_unit);
		}

		/// <summary>
		/// Throw XmlException with LineNumber/LinePosition info.
		/// </summary>
		/// <param name="message">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="innerException">
		/// A <see cref="Exception"/>
		/// </param>
		/// <param name="xObj">
		/// A <see cref="XObject"/> to put LineNumber and LinePosition into Exception.
		/// E.g. XElement or XAttribute are derived from XObject.
		/// </param>
		static internal void ThrowXmlExceptionWithLineInfo (string message, Exception innerException, XObject xObj)
		{
			IXmlLineInfo xmlLineInfo = xObj as IXmlLineInfo;
			if (xmlLineInfo != null)
				throw new XmlException (message, innerException, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
			else
				throw new XmlException (message, innerException);
		}

		// Used to parse both element and attribute content.
		// Both XElement and XAttribute derive from XObject but XObject has no content so cannot be used.
		/// <summary>
		/// Parses hex number e.g."0x123af". Prefix "0x" is required.
		/// </summary>
		static internal int ParseHexInt (string strToParse, XObject xObj)
		{
			const string HexPrefix = "0x";

			// Prefix "0x" required in RomRaider-format
			// but not allowed in int.Parse(...) even though using NumberStyles.HexNumber.
			int index0x = strToParse.IndexOf (HexPrefix);
			if (index0x < 0) {
				string message = "Prefix '" + HexPrefix + "' missing in XML item: " + xObj.ToString ();
				ThrowXmlExceptionWithLineInfo (message, null, xObj);
			}

			try {
				return int.Parse (strToParse.Substring (index0x + HexPrefix.Length), System.Globalization.NumberStyles.HexNumber, NumberFormatInfoInvariant);
			} catch (Exception ex) {
				ThrowParse (ex, xObj, "hex integer");
				throw;
			}
		}

		static internal void ThrowParse (Exception ex, XObject xObj, string typeStr)
		{
			if (ex is FormatException || ex is OverflowException) {
				// FormatException also thrown when string is empty
				string message = "Could not parse " + typeStr + " in XML item: " + xObj.ToString ();
				ThrowXmlExceptionWithLineInfo (message, ex, xObj);
			}
		}

		public static XDocument XDoc (params object[] content)
		{
			// XDeclaration: null parameters --> "<?xml version="1.0" encoding="utf-8"?>"
			return new XDocument (new XDeclaration (null, null, null), content);
		}

		static XElement GetXElement (Table2D table2D)
		{
			return new XElement (X_table2D, new XAttribute (X_category, table2D.Category), new XAttribute (X_name, table2D.Title), new XAttribute (X_address, HexNum (table2D.Location)), ValueRangeComment (table2D.Xmin, table2D.Xmax), GetAxisXElement (X_axisX, table2D.RangeX.Pos, table2D.NameX, table2D.UnitX), ValueRangeComment (table2D.Ymin, table2D.Ymax), GetValuesElement (table2D.RangeY.Pos, table2D.UnitY, table2D.TableType), new XElement (X_description, table2D.Description));
		}

		static XElement GetXElement (Table3D table3D)
		{
			return new XElement (X_table3D, new XAttribute (X_category, table3D.Category), new XAttribute (X_name, table3D.Title), new XAttribute (X_address, HexNum (table3D.Location)), ValueRangeComment (table3D.Xmin, table3D.Xmax), GetAxisXElement (X_axisX, table3D.RangeX.Pos, table3D.NameX, table3D.UnitX), ValueRangeComment (table3D.Ymin, table3D.Ymax), GetAxisXElement (X_axisY, table3D.RangeY.Pos, table3D.NameY, table3D.UnitY), ValueRangeComment (table3D.Zmin, table3D.Zmax), GetValuesElement (table3D.RangeZ.Pos, table3D.UnitZ, table3D.TableType),
			new XElement (X_description, table3D.Description));
		}

		static XElement GetAxisXElement (string axisType, int address, string name, string unit)
		{
			return new XElement (axisType, new XAttribute (X_address, HexNum (address)), new XAttribute (X_name, name), new XAttribute (X_unit, unit));
		}

		static XElement GetValuesElement (int address, string unit, TableType tableType)
		{
			return new XElement (X_values, new XAttribute (X_address, HexNum (address)), new XAttribute (X_unit, unit), new XAttribute (X_tableType, tableType.ToStr ()));
		}

		static XComment ValueRangeComment (float min, float max)
		{
			return new XComment (string.Format (" {0} to {1} ", min.ToString (), max.ToString ()));
		}

		static string HexNum (int num)
		{
			return "0x" + num.ToString ("X");
		}
	}
}
