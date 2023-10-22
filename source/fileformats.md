Some TV models and DVB-S receivers from different brands share the same 3rd party DVB-S tuner hardware, firmware 
and file format, so even if your TV brand and model is not in the following list, there is a chance that it is 
supported by ChanSort anyway.  
On the other hand it's possible that a manufacturer uses a file format for certain models that's not (yet) supported
by ChanSort.

<a name="samsung"/>Samsung
---
- .scm files: B (2009)*, B (2013), C, D, E, F, H, J series  
- .zip files (Tizen OS): H, J, K, M, N and Q, R series  

\*: The "clone.bin" format is not supported. In the "*.scm" format
the "Air Analog"-list of the 2009 B-series doesn't support all 
editing features due to a lack of test files.

Instructions for transferring the channel list can be found on:
https://github.com/PredatH0r/ChanSort/wiki/Samsung

<a name="lg"/>LG
---
- Series based on NetCast OS exporting a xx\*.TLL file:  
  CS, DM, LA, LB\*, LD, LE, LF, LH, LK, LM+, LN, LP#, LS, LT, LV, LW, LX, PM, PN, PT, UB\*  
- Series based on webOS 2-5 exporting a GlobalClone00001.TLL file

\*: Some devices behave erroneously due to firmware issues.  
+: xxLM640T is not supported due to its firmware limitations.  
\#: Only satellite channels supported.

Models with the NetCast operating system don't have the export/import function in the menu. Instead you need to press+hold the 
settings button on the remote control until the settings disappear again, then enter 1105 and select the "TV Link Loader" menu.
With the latest firmware for the LM series the special "service remote control" is no longer needed, "1105" works now too.

IMPORTANT: It is REQUIRED to select special options during the channel search on the TV. If you select a provider / country / 
satellite specific list, it will be pre-ordered and your TV will behave erratic after an export+import.
Always select "none / other / all" and never your cable TV provider or something like "Astra 19.2E list" and blindscan.

<a name="sony"/>Sony
---
- Android-TVs: "sdb.xml" files using format "FormateVer" 1.1.0
- KDL 2012/2014: "sdb.xml" files using "FormatVer" 1.0.0, 1.1.0 and 1.2.0 

<a name="hisense"/>Hisense
---
- Some older models like LTDN40D50TS use the same .csv files as Sharp. See [Sharp](#sharp) for instructions.
- 2016 "Smart" models with a channel.db file, i.e. H65M5500  
- 2017 models with a servicelist.db file
- models exporting a HIS_DVB.BIN file
- models exporting a set of HIS\_FAV.BIN, HIS\_SVL.BIN and HIS\_TSL.BIN files

Special thanks to Hisense for supporting ChanSort with technical information and a test device!

<a name="panasonic"/>Panasonic
---
**Android-TVs** come with different internal hardware, firmware and file formats, so support depends on the model.
- mnt/vendor/tvdata/database/tv.db file (LSW500 and LXW700 series)
- channellist.txt (MX700, MZ800)
- channels.sdx ("fire-OS" MXW834)
- NOT supported: CLONE00001/settingsDB\_enc.db (JXW600)
- NOT supported: hotel\_setup/Channel\_list/channel\_list.bin (JXW800)
On some models you can export/import the list by selecting "Input: Cable" (or any other TV source), then
open the menu, Channels, Export/Import.
Other models require to use the secret hotel menu: Menu / Picture / Picture Mode / User Defined / Contrast / 6x ok.

**Viera** models since 2011 with channel list formats
- svl.bin 
- svl.db (JZT1500, ...)
To export/import files on Viera models, format the USB stick with FAT32 and create an empty file or directory 
named hotel.pwd. After plugging in the stick on the TV enter 4850 for TV->USB or 4851 for USB->TV

<a name="tcl"/>TCL / Thomson
---
TVs exporting a .tar archive containing a database/cloneCRC.bin and database/usedata/DtvData.db file  
(Various TCL Android / Google TV models)

<a name="philips"/>Philips
---
Philips uses countless incompatible file formats for various TV models.
ChanSort currently supports the formats:  
- PhilipsChannelMaps\ChannelMap_45, 100, 105 and 110
- Repair\ChannelList\channellib\\\*Table and s2channellib\\\*.dat
- Repair\CM_TPM1013E_LA_CK.xml (sometimes that file is hidden and only a .bin file is visible)

<a name="sharp"/>Sharp (and some models from Dyon, Blaupunkt, Hisense, Changhong, Grundig, alphatronics, JTC Genesis)
---
Some of these TVs share similar hardware and can export/import a .csv file allowing to change the channel order:  
- DVBS_Program.csv (alongside cvt_database.dat)
- DVBS_CHANNEL_TABLE.csv (alongside dtv_cmdb\*.bin)
- MS\*_DVBS_CHANNEL_TABLE.csv (alongside MS\*_HOTELMODE_TABLE.json)
  
Blaupunkt B40A148TCSFHD, B32B133T2CSHD, ...  
Changhong LED32E2200ST2, ...  
Dyon Live 22 Pro, Live 24 Pro, ENTER 32 Pro X, ...  
Hisense LTDN40D50TS, ...  
Sharp LC-xxCFE4142E, LC-xxCFF6002E, LC-xxFI5542E, LC-xxFG5242E, LC-xxUI7552E, LC-xxUI7652E, xxBJ1E, xxBJ3E, xxBJ5E, ...  
Grundig 24 GHB 5944  

Depending on the actual TV the import/export function can be part of the regular user menu, a hotel menu or a service menu
and the exported files may be different. Some models like Sharp Aquos xxBJ1E have a sub menu for exporting. You need to
export all 4 type of files for the import to work.  
To access the secret hotel / service menus, there are several possible key combinations on the remote control:  
hotel menu: MENU 7906 / MENU 4588  
service menu: MENU 1147 / MENU 11471147 / SOURCE 2580  
Do not make any changes in the service menu, as this could damage your TV. Only use the import/export functions.

<a name="toshiba"/>Toshiba
---
- Models that export a .zip file containing chmgt.db, dvbSysData.db and dvbMainData.db files.  
(e.g. RL, SL, TL, UL, VL, WL, XL, YL models of series 8xx/9xx)  
- Models with a settingsDB.db file

<a name="grundig"/>Grundig
---
- Models that export files named dvb\*_config.xml.
- Models that export a cvt_database.dat file, e.g. 24 GHB 5944: see [Sharp](#Sharp)
- Models that export a set of \*\_cmdb\_\*.bin files

<a name="medion"/>
---
- Android Smart TVs exporting a "senderliste.txt" (containing JSON data lines), e.g. X15567 (MD 31555)
- Models with .sdx lists (see SatcoDX)

<a name="satcodx"/>SatcoDX (supplier for ITT, Medion, Nabo, ok., PEAQ, Schaub-Lorenz, Silva-Schneider, Telefunken)
---
Various brands use the same hardware for DVB-S, which exports .sdx files 

<a name="dbm"/>Xoro, TechniSat, Strong, ...
---
Various brands use the same hardware for DVB-S and DVB-C receivers, which export variations of .DBM-files

<a name="visionedge"/>VISION EDGE 4K
---
database.db file exported by this DVB-S/C/T set-top-box

<a name="vdr"/>VDR (Linux Video Disk Recorder)
---
Supports the channels.conf file format.  
Implementation for this was provided by TCr82 from the VDR project.

<a name="m3u"/>m3u (SAT>IP)
---
Supports SAT>IP .m3u files with extended information holding channel names and program numbers.

<a name="enigma2"/>Enigma2 (Dreambox, VU+ and many other Linux based receivers)
---
Requires a local copy of the files "lamedb", "bouquets.\*" and "userbouquet.\*" from /etc/Enigma2/.  

