// RomMedadata.cs: "romid" metadata, compatible to RomRaider ECU def format.

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


// TODO cleanup, reuse generic XML code

using System;
using System.Xml;
using System.Xml.Linq;

namespace Subaru.File
{

	/// <summary>
	/// ROM description data, mostly for RR export.
	/// Should be compatible to RomRaider ECU def format.
	/// </summary>
	public sealed class RomMetadata
	{
		const string RRX_romid = "romid";
		const string RRX_xmlid = "xmlid";
		const string RRX_internalidaddress = "internalidaddress";
		const string RRX_internalidstring = "internalidstring";
		const string RRX_ecuid = "ecuid";
		const string RRX_year = "year";
		const string RRX_market = "market";
		const string RRX_make = "make";
		const string RRX_model = "model";
		const string RRX_submodel = "submodel";
		const string RRX_transmission = "transmission";
		const string RRX_memmodel = "memmodel";
		const string RRX_flashmethod = "flashmethod";
		const string RRX_filesize = "filesize";

		// = calibration ID in most cases
		string xmlid;
		// file pos to read calibration ID, RR jargon: "internalidaddress"
		int calibrationIDPos;
		// RR jargon: "internalidstring"
		string calibrationID;
		// 5 bytes = 10 hex chars, RR jargon: "ecuid"
		long romid;
		string year;
		string market;
		string make;
		string model;
		string submodel;
		string transmission;
		string memmodel;
		string flashmethod;

		// int.Max is 2 GiB, more than enough
		int filesize;

		public RomMetadata ()
		{
		}

		public int Filesize {
			get { return this.filesize; }
			set { filesize = value; }
		}

		public string Flashmethod {
			get { return this.flashmethod; }
			set { flashmethod = value; }
		}

		public int CalibrationIDPos {
			get { return this.calibrationIDPos; }
			set { calibrationIDPos = value; }
		}

		public string CalibrationID {
			get { return this.calibrationID; }
			set { calibrationID = value; }
		}

		public string Make {
			get { return this.make; }
			set { make = value; }
		}

		public string Market {
			get { return this.market; }
			set { market = value; }
		}

		public string Memmodel {
			get { return this.memmodel; }
			set { memmodel = value; }
		}

		public string Model {
			get { return this.model; }
			set { model = value; }
		}

		/// <summary>
		/// Should be displayed as 10 hex characters.
		/// RomRaider jargon: "ecuid"
		/// </summary>
		public long Romid {
			get { return this.romid; }
			set { romid = value; }
		}

		/// <summary>
		/// ROMID as string, 10 hex chars.
		/// </summary>
		public string RomIdStr {
			get { return RomIdToStr (this.romid); }
		}

		public string Submodel {
			get { return this.submodel; }
			set { submodel = value; }
		}

		public string Transmission {
			get { return this.transmission; }
			set { transmission = value; }
		}

		public string Xmlid {
			get { return this.xmlid; }
			set { xmlid = value; }
		}

		public string Year {
			get { return this.year; }
			set { year = value; }
		}

		public XElement XElement {
			get { return new XElement (RRX_romid, new XElement (RRX_xmlid, xmlid), new XElement (RRX_internalidaddress, calibrationIDPos.ToString ("X")), new XElement (RRX_internalidstring, calibrationID), new XElement (RRX_ecuid, RomIdStr), new XElement (RRX_year, year), new XElement (RRX_market, market), new XElement (RRX_make, make), new XElement (RRX_model, model), new XElement (RRX_submodel, submodel),new XElement (RRX_transmission, transmission), new XElement (RRX_memmodel, memmodel), new XElement (RRX_flashmethod, flashmethod), new XElement (RRX_filesize, FileSizeToStr (filesize))); }
		}

		public static RomMetadata FromXML (XElement romidElement)
		{
			RomMetadata d = new RomMetadata ();
			d.Xmlid = (string)romidElement.Element (RRX_xmlid);

			XElement el;
			el = romidElement.Element (RRX_internalidaddress);
			if (el != null)
				d.CalibrationIDPos = ParseHexInt ((string)el, el);
			d.CalibrationID = (string)romidElement.Element (RRX_internalidstring);

			el = romidElement.Element (RRX_ecuid);
			if (el != null)
				d.Romid = ParseHexLong ((string)el, el);

			d.Year = (string)romidElement.Element (RRX_year);
			d.Market = (string)romidElement.Element (RRX_market);
			d.Make = (string)romidElement.Element (RRX_make);
			d.Model = (string)romidElement.Element (RRX_model);
			d.Submodel = (string)romidElement.Element (RRX_submodel);
			d.Transmission = (string)romidElement.Element (RRX_transmission);
			d.Memmodel = (string)romidElement.Element (RRX_memmodel);
			d.Flashmethod = (string)romidElement.Element (RRX_flashmethod);
			return d;
		}

		#region static helper methods

		/// <summary>
		/// Parses hex number e.g."0x123af". Prefix "0x" is optional.
		/// </summary>
		static internal int ParseHexInt (string strToParse, XObject xObj)
		{
			const string HexPrefix = "0x";

			int index0x = strToParse.IndexOf (HexPrefix);
			if (index0x >= 0) {
				strToParse = strToParse.Substring (index0x + HexPrefix.Length);
			}

			try {
				return int.Parse (strToParse, System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.InvariantInfo);
			} catch (Exception ex) {
				ThrowParse (ex, xObj, "hex integer");
				throw;
			}
		}

		/// <summary>
		/// Parses hex number e.g."0x123af". Prefix "0x" is optional.
		/// </summary>
		static internal long ParseHexLong (string strToParse, XObject xObj)
		{
			const string HexPrefix = "0x";

			int index0x = strToParse.IndexOf (HexPrefix);
			if (index0x >= 0) {
				strToParse = strToParse.Substring (index0x + HexPrefix.Length);
			}

			try {
				return long.Parse (strToParse, System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.InvariantInfo);
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


		/// <summary>
		/// ROMID as string, 10 hex characters.
		/// </summary>
		/// <param name="romid">
		/// A <see cref="System.Int64"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string RomIdToStr (long romid)
		{
			return romid.ToString ("X10");
		}

		public static string FileSizeToStr (int fSize)
		{
			string postfix = null;
			if (fSize % 1024 == 0) {
				fSize = fSize / 1024;
				postfix = "KB";
			}
			if (fSize % 1024 == 0) {
				fSize = fSize / 1024;
				postfix = "MB";
			}
			return postfix != null ? fSize.ToString () + postfix : fSize.ToString ();
		}

		#endregion static helper methods
	}
}

