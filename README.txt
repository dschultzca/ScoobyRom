
CONTENT
=======
1	License
2	Purpose
3	Further Details


1) License
==========
GPLv3. See text file COPYING.txt in project subfolders for license text.
You can also get license text in different formats plus further details here:
http://www.gnu.org/licenses/gpl.html
http://www.gnu.org/licenses/gpl-faq.html


2) Purpose
==========
ScoobyRom is a Subaru ROM specific file content viewer and metadata editor.
Currently it can find and visualize 2D and 3D tables.
Also it has some checksumming calculations built in.
ROM format is supposed to be 32 bit, others may not work.

ROM types tested working, all 1 MiB size type so far:
.) Engine Control Unit (ECU): gasoline and diesel models
.) Transmission Control Unit (TCU): Automatic Transmission (AT)

However you can try any file safely as it is being opened in read-only mode.
Worst thing that can happen is the app finds nothing at all or false items only.

This application is not a real ROM editor (yet), you cannot change table values!
Remember, in this version the ROM file is only being read.
All additional data is saved into an extra XML file.
However, ScoobyRom has a RomRaider ECU def export feature,
see chapter "RomRaider ECU def export XML".


3) Further Details
==================
See README.txt files in project subfolders.
