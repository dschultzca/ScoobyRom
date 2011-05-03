
CONTENT
=======
1	License
2	Purpose
3	ScoobyRom Installation
4	Dependencies
5	Files
6	Launch and Command Line Parameters
7	User Interface Hints


1) License
==========
GPLv3. See text file COPYING.txt for license text.
http://fsf.org/
You can also get license text in different formats plus further details there.
http://www.gnu.org/licenses/gpl.html
http://www.gnu.org/licenses/gpl-faq.html


2) Purpose
==========
ScoobyRom is a Subaru ROM specific file content viewer and metadata editor.
Currently it can find and visualize 2D and 3D tables.
Also it has some checksumming calculations built in.
ROM format is supposed to be 32 bit, others may not work.

ROM types tested working:
.) Engine Control Unit (ECU): gasoline and diesel models,
	SH7058 (1.0 MiB) and SH7059 based (1.5 MiB)
.) Transmission Control Unit (TCU): Automatic Transmission
	5AT (1.0 MiB)

However you can try any file safely as it is being opened in read-only mode.
Worst thing that can happen is the app finds nothing at all or false items only.

This application is not a real ROM editor (yet), you cannot change table values!
Remember, in this version the ROM file is only being read.
All additional data is saved into an extra XML file.
However, ScoobyRom has a RomRaider ECU def export feature,
see chapter "RomRaider ECU def export XML".


3) ScoobyRom Installation
=========================

3.1) Binary archive
*******************
Extract the archive (.7z or .zip) into a new (empty) folder.

IMPORTANT:
Because of writing gnuplot temp data file in there, the installation directory should be writeable!
Only matters for gnuplot feature to work.
Windows OS:
  Directories below \Program Files\... are read-only!
  Recommended: Do use any non-Windows folder like \ScoobyRom\ instead.
  Alternative: right-click ScoobyRom.exe and use "Run as Administrator".
Issue will be fixed in future releases.

In addition you need to make sure you've got all required dependencies installed,
Gtk# at least, see below.


3.2) Source Code
****************

.) Getting source files from "git" source control repository

git: (read-only, best protocol)
	git://github.com/SubaruDieselCrew/ScoobyRom.git
	Example, Linux at least:
git clone git://github.com/SubaruDieselCrew/ScoobyRom.git

HTTP: (alternative protocol, workaround for network firewalls etc.)
	http://github.com/SubaruDieselCrew/ScoobyRom.git
git clone http://github.com/SubaruDieselCrew/ScoobyRom.git


.) Getting source files as archive download
Not prepared yet!


.) Compilation
For instructions and hints see file DEVELOPMENT.txt.


4) Dependencies
===============

4.1) .NET 4.0 or compatible runtime
***********************************
REQUIRED!
Tested working:

.) Microsoft .NET 4.0
	Windows only, free update, "client profile" download should be sufficient (server stuff not needed)
	Windows Vista came with .NET 3.5 pre-installed IIRC

	My personal recommendation:
	Microsoft .NET Framework 4 Client Profile (Standalone Installer)
	http://go.microsoft.com/fwlink/?linkid=186919
	dotNetFx40_Client_x86_x64.exe (41.0 MiB, 4/12/2010)

	Or try this .NET specific download page:
	http://msdn.microsoft.com/en-us/netframework/aa569263

	Another link: Microsoft .NET Framework Developer Center
	http://msdn.microsoft.com/en-us/netframework/default.aspx

.) Mono 2.8.x (on Linux)
	free open source, multi-platfrom
	http://www.mono-project.com/


4.2) Gtk#
*********
REQUIRED!
The application won't work without it as it's required for the multi-platform user interface.
Basically it's a .NET wrapper for native Gtk+ binaries.
http://www.gtk.org/

Note: Software like MonoDevelop depends on Gtk#, too.
No need to install this if it's already on the machine of course.

http://mono-project.com/GtkSharp
( Download --> http://www.go-mono.com/mono-downloads/download.html )
It's called "Gtk# for .NET", you may not need the others (Mono).

Windows
-------
Latest version:
gtk-sharp-2.12.10.win32.msi (15.7 MiB)

Experimental Gtk+ x64 versions have not been tested yet!
Use stable 32 bit version (included in above Gtk# installer) and
compile ScoobyRom for 32bit (Platform x86) to make sure.


Unix/Linux
----------
On Linux at least, one does not need to care about 32 or 64 bit, x64 is fine by default.
Generally, UI performance on Unix/Linux is best as Gtk+ libraries are often in use anyway.


OSX
---
Awaiting feedback... not sure if it runs on MacOS. Theoretically it should.


Performance Note:
On Windows and OSX, Gtk+ is somewhat foreign - most apps use OS speficic UI framwork directly,
so required Gtk+ libs (DLLs) must be loaded first (cold startup) - you might notice some delay.


4.3) NPlot
**********
Required. Nothing to do as it's already included pre-compiled (/vendor/NPlot.dll).
Did not change code at all.
Source is pure C#, multi-platform.
Development is rather inactive for years but pretty good and compact. Cannot do 3D (surface) plots!
http://netcontrols.org/nplot/wiki/


4.4) gnuplot
************
OPTIONAL.
gnuplot is being used for EXTERNAL plotting (inside extra windows).
+) It's the only method to get 3D surface plots.
+) Generating SVG files is also done through gnuplot, basically it's like a re-plot into file.
+) interactive features (zoom, scale, stretch... try mouse/keys, see gnuplot documentation)

Homepage: http://www.gnuplot.info/


gnuplot installation on Windows
-------------------------------
1) Windows binaries:
http://sourceforge.net/projects/gnuplot/files/
gp442win32.zip (9.4 MiB)

2) Extract the ZIP file somewhere (ex. "C:\"),
it should place all files under main subdirectory "gnuplot" from there.
(--> "C:\gnuplot\" )

3) You'll need to edit file 'ScoobyRom.exe.config' with a text editor.
Enter the exact full path of 'gnuplot.exe', it should be in subfolder "binary".
Ex: "C:\gnuplot\binary\gnuplot.exe"


gnuplot installation on Linux
-----------------------------
Your distribution repositories might offer a package called "gnuplot" - use your package manager to install it.
(If you want to play with gnuplot yourself I also recommend installing documentation package
like "gnuplot-doc" or similar).

You should be able to run "gnuplot". It may live in /usr/bin/gnuplot for example.
Edit 'ScoobyRom.exe.config' if necessary.


gnuplot installation on other platforms:
MacOS etc. - Not tested yet!


Testing gnuplot
---------------
Launch the binary (Windows: ...\binary\gnuplot.exe, Unix: gnuplot)
(gnuplot command prompt "gnuplot>" should come up.)
plot cos(x)
(graph window should appear)
quit


gnuplot Notes
-------------
.) Currently a temporary binary data file for gnuplot is being created in ScoobyRom app dir!
Make sure there's write access! (directories below \Program Files\... might be read-only be default).

.) gnuplot seems to run much faster on Linux compared to Windows!
Not sure - gnuplot wasn't designed to use 3D acceleration at all.
This is most visible doing high-resolution/full-screen 3D surface plots.
On Linux it feels like it draws roughly 10x more frames per second, not entirely fluid though.


5) Files
========

5.1) ScoobyRom.exe.config
*************************
Contains settings, see comments inside for details.
Since there's no settings user interface yet, you'll need to edit the (XML) file with a text editor.
To be recognized it must live in EXE folder and have exact name.
The app should work without .config file but it might be required for gnuplot features.

.) gnuplot
Especially on Windows the specific entry for gnuplot is important!
If there is not a real gnuplot installer (only ZIP), no path to gnuplot binary is added
to PATH environment variable, so ScoobyRom cannot find required gnuplot.exe on its own.

.) Icons on by default
Applies to icon column on both 2D and 3D list UI.
"True": Increases load wait time as icon creation means work.
or
"False": Icons are generated on first demand. UI row heights are smaller without icons.


5.2) ScoobyRom XML
******************
Simple format to support app specific features, not RomRaider compatible.
Basically it's meant for saving entered metadata (title, category, axis names, units, data type, ...).
Currently the application only writes items into file that got some entered text.
However it does create a bare minimum and valid file on File->Save (Ctrl+S) operation.

Use any text editor to view/edit it.
Recommendation for Windows:
Notepad++ (free OSS, lots of features) 	http://notepad-plus.sourceforge.net/uk/site.htm

The file is not required (like opening a new ROM with no ScoobyRom metadata saved yet).
If it does exist, the app will try to read saved metadata and
combine (merge) it with data (values) got from searching the ROM.


*) FILE PATH
ScoobyRom assumes its XML to be found inside current ROM file directory!!!
Filename will/must be <ROM-filename-without-extension>.xml.
Ex:
"my ROM file.rom" or "my ROM file.bin" whatever...
--> "my ROM file.xml" within same folder is being assumed.
Actual ROM file extension (.rom, .bin, .hex etc.) does not matter!

Pros:
  No additional file dialog for XML to click through opening a ROM file.
  No ambiguity, there's only a single valid XML path for a specific ROM file.
  Easy to find and backup this XML regularly (simple file copy, source control, ...).

Cons:
  ROM folder must be writeable.
  WARNING: In case of existing file, ScoobyRom does not ask for permission,
    it immediatly overwrites existing file!!!!


*) HOW TO IMPROVE ROM LOAD PERFORMANCE:
ROM search range is not required but improves search/load speed tremendously!
Unfortunately, the table position range is very ROM specific.

Without XML or missing XML element (see below), the app will search through the whole ROM file.
This can take several or many seconds.
Using a good search range, load time is usually just a fraction of a second - highly recommended!
See record position column and statistics window to get first/last record positions.
Currently you've got to adjust this manually in the XML.
Ex.:

It's not necessary to copy & paste exact numbers like
<tableSearch start="0x8BE98" end="0x936D8" />
it'll provide maximum load speed, though.

Somewhat larger range will be fast already, might work for similar ROMs:
<tableSearch start="0x80000" end="0x95000" />


Note: Searching through unsuitable (too large, whole file) range might introduce false tables!
If you want to annotate a ROM, I'd recommend specifying a suitable search range soon,
you'll get speed and avoid looking at false data.


*) romid metadata
Borrowed from RomRaider ECU def format, needed for RR-export anyway.
There's no UI yet, you'll have to edit XML manually.
In case you've got similar ROMs I would copy & paste whole romid-segment, then do the required changes.

ScoobyRom itself currently does only use these elements
.) internalidaddress
.) internalidstring
to verify the string from ROM and display it in main window title.
All others are just being read and written.


*) Full example content
Note: Like for RR export the XML comments are auto generated by ScoobyRom as these can be useful.
XML file containing just two annotated tables for brevity:

<?xml version="1.0" encoding="utf-8"?>
<rom>
  <romid>
    <xmlid>JZ2F401A</xmlid>
    <internalidaddress>400C</internalidaddress>
    <internalidstring>JZ2F401A</internalidstring>
    <ecuid>6644D87207</ecuid>
    <year>2009_10</year>
    <market>EDM</market>
    <make>Subaru</make>
    <model>Impreza</model>
    <submodel>2.0 Turbo Diesel 150 HP</submodel>
    <transmission>6MT</transmission>
    <memmodel>SH7058</memmodel>
    <flashmethod>subarucan</flashmethod>
    <filesize>1MB</filesize>
  </romid>
  <tableSearch start="0x89500" end="0x93700" />
  <table2D category="Sensors" name="Coolant Temperature" storageaddress="0x8BE98">
    <!-- 0.248 to 4.836 -->
    <axisX storageaddress="0xB36D8" name="Sensor Voltage" unit="V" />
    <!-- -40 to 135 -->
    <values storageaddress="0xB374C" unit="°C" storagetype="UInt8" />
    <description>NTC thermistor</description>
  </table2D>
  <table3D category="Fueling" name="Injection Quantity Limit (Gear)" storageaddress="0x90518">
    <!-- 0 to 5400 -->
    <axisX storageaddress="0xCD144" name="Engine Speed" unit="1/min" />
    <!-- 0 to 6 -->
    <axisY storageaddress="0xCD1A0" name="Gear" unit="1" />
    <!-- -30 to 100 -->
    <values storageaddress="0xCD1BC" unit="mm³/st" storagetype="UInt8" />
    <description />
  </table3D>
</rom>



5.3) RomRaider ECU def export XML
*********************************
Currently the application only writes annotated tables into file.
There is no UI for custom filtering or "export all" yet.

You can use RomRaider to edit map values and modify the ROM.
http://www.romraider.com/

Important: For signed data types (int16, int8) support,
RomRaider 0.5.3b RC7 or newer is needed!
http://www.romraider.com/forum/viewtopic.php?f=14&t=6801


5.4) SVG export
***************
http://en.wikipedia.org/wiki/SVG
Requires working gnuplot!
Basically the app instructs gnuplot to refresh the current plot (window) into SVG file.

Regarding 3D: SVG is a pure 2D format, you cannot rotate the view inside SVG.
Rotate the plot until satisfied then do the SVG export.
Of course you can create as many exports as you like.

SVGs showing 2D-plots take around 20 KiB, rather fast to view.
A 3D plot SVG containing hundreds of polygons can take 150 KiB for example.
Displaying a complicated file can take seconds depending on viewer program and computer speed.

SVGs should be viewable in modern browsers like
Mozilla Firefox	http://www.mozilla.org/products/firefox/
Opera	http://www.opera.com/
Internet Explorer 9 (currently beta)	http://ie.microsoft.com/testdrive/
...
various graphic viewers and vector graphics editors.

Recommended full featured SVG editor:
Inkscape (free OSS, multi-platform): http://inkscape.org/


5.5) Other export formats
*************************
Not implemented yet.
gnuplot also supports many other export formats like PDF, EPS, PNG and so on.
Would be easy to add support in the same way as SVG.


5.6) Temporary files
********************
Only one: "gnuplot_data.tmp", always created in application folder.
This binary data file is being used for all gnuplot plots - overwritten on each new plot action.
Because of this, the app folder needs to be writeable!
Windows:
To make things easy you might want to avoid using (\Program Files\...) app location, use a non-Windows
related folder instead. Otherwise it might be necessary to set special folder permissions.

We could use a standard temporary file path provided by the OS.
Pros: App folder does not need write permissions.

Cons: Since temp file path is not known upfront, one cannot specify it directly in gnuplot template files.
      This means more things are done in code vs. template.


5.7) gnuplot template files
***************************
Text files "gnuplot_Table2D.plt" and "gnuplot_Table3D.plt" must live in app EXE dir,
can be read-only. Errors can result in empty plots.
These contain most gnuplot commands being executed on plot action.
The latter file (3D) has lots of comments in it - you might be able to learn some gnuplot commands from it.
Feel free to modify those - change colors, layout, default view etc.
Text labeling commands are generated within the app on the fly, you need to change source code.


6) Launch and Command Line Parameters
=====================================
ScoobyRom is designed to only load a single ROM!!!
However you can start the app multiple times - load different(!) ROMs in each instance,
put app windows on different monitors and so on.

One optional argument is supported:

ScoobyRom.exe <ROM_file>

On Windows OS for example:
ScoobyRom.exe "\ROMs\Current Rom\my rom.bin"

Using Mono runtime on Unix systems for example:
mono ScoobyRom.exe ~/ROMs/myROM.hex

If ROM_file cannot be found, ScoobyRom starts in normal mode:
ready to open a ROM via UI command (Ctrl+O).


7) User Interface Hints
=======================
Couple of things that may not be immediatly obvious:

.) Both tab pages have a horizontal splitter between list view and visualization area (bottom).
Click & drag splitter according to your needs.

.) Using check mark columns has no effect at all, selection isn't saved.
You can use it and sort items if you want to. Might have more effect in future versions.

.) Visualization (Ctrl+Space)
Either displays colored table values (3D) or 2D line graph in bottom tab pane.
Any visualization (except icons) does not update on changed metadata (text, data type),
you'll have to trigger updates manually.
Double-clicking or Enter key on a focused read-only row column (icon, numbers)
also triggers internal visualization.

.) Icons update immediatly on table type change as this is a fast operation.
By looking a the icon you can often tell already whether current data type is correct or not ("patterns", bad values).

.) Gray icons are displayed on tables having constant values  (min = max).
Often these are valid tables and are actually used in ROM code.
