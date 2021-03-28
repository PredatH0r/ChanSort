Links
-----
[![EN](http://beham.biz/chansort/flag_en.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme.md)
[![DE](http://beham.biz/chansort/flag_de.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme_de.md) |
[Download](https://github.com/PredatH0r/ChanSort/releases) | 
[Documentation](https://github.com/PredatH0r/ChanSort/wiki) |
[Forum](https://github.com/PredatH0r/ChanSort/issues) | 
[E-Mail](mailto:horst@beham.biz)

About ChanSort
--------------
ChanSort is a Windows application that allows you to reorder your TV's channel list.  
Most modern TVs can transfer channel lists via USB stick, which you can plug into your PC.  
ChanSort supports various file formats from Samsung, LG, Panasonic, Sony, Philips, Hisense, Toshiba, Grundig,  
Sharp, Dyon, Blaupunkt, SatcoDX (used by ITT, Medion, Nabo, ok., PEAQ, Schaub-Lorenz, Silva-Schneider, Telefunken),  
Linux VDR, SAT>IP .m3u and Enimgma2 based Linux boxes.

![screenshot](http://beham.biz/chansort/ChanSort-en.png)

Features
--------
- Reorder channels (change numbers directly, move up/down, drag&drop, double-click)
- Use another channel list as a reference to apply the same ordering
- Multi-selection for editing multiple channels at once
- Side-by-side view of new/sorted list and original/complete list (similar to playlist and library)
- Rename or delete channels
- Manage favorites, parental lock, channel skipping (when zapping), channel hiding
- User interface in English, German, Spanish, and partially in Turkish, Portuguese and Russian
- Unicode character support for channel names (Latin, Cyrillic, Greek, ...)

Some features may not be available on all TV models and channel types (analog, digital, sat, cable, ...)

! USE AT YOUR OWN RISK !
------------------------
Most of this software was written without support from TV manufacturers or access to any official 
documentation about the file formats. It is solely based on analysing existing data files, trial and error.
There is a chance of unwanted side-effects or even damage to your TV, as reported in 2 cases.

Hisense is the only manufacturer who provided technical information and a test device.


System requirements
-------------------
- [Microsoft .NET Framework 4.8 or later](https://dotnet.microsoft.com/download/dotnet-framework)
  (When running under Linux, you need Winetricks, select a 32bit wineprefix and install the "dotnet48" package)
- [Microsoft Visual C++ 2010 Redistributable Package (x86)](http://www.microsoft.com/en-us/download/details.aspx?id=8328):
  required to edit SQLite lists (Hisense, Panasonic, Toshiba and Samsung .zip format)
- USB stick/SD-card to transfer the channel list between your TV and PC (FAT32 file system recommended)
- Some LG models require a special service remote control to access the import/export functions (see Wiki for details)


Supported TV models 
-------------------

**Samsung**  
.scm files: B (2009)*, B (2013), C, D, E, F, H, J series  
.zip files (Tizen OS): H, J, K, M, N and Q, R series  

\*: The "clone.bin" format is not supported. In the "*.scm" format
the "Air Analog"-list of the 2009 B-series doesn't support all 
editing features due to a lack of test files.

Instructions for transferring the channel list can be found on:
https://github.com/PredatH0r/ChanSort/wiki/Samsung

**LG**  
Series based on NetCast OS exporting a xx\*.TLL file:  
  CS, DM, LA, LB\*, LD, LE, LF, LH, LK, LM+, LN, LP#, LS, LT, LV, LW, LX, PM, PN, PT, UB\*  
Series based on webOS 2-5 exporting a GlobalClone00001.TLL file

\*: Some devices behave erroneously due to firmware issues.  
+: See system requirements for LM-series. xxLM640T is not supported due to its firmware limitations.  
\#: Only satellite channels supported.

Models with the NetCast operating system don't have the export/import function in the menu. Instead you need to press+hold the 
settings button on the remote control until the settings disappear again, then enter 1105 and select the "TV Link Loader" menu.
With the latest firmware for the LM series the special "service remote control" is no longer needed, "1105" works now too.

IMPORTANT: It is REQUIRED to select special options during the channel search on the TV. If you select a provider / country / 
satellite specific list, it will be pre-ordered and your TV will behave erratic after an export+import.
Always select "none / other / all" and never your cable TV provider or something like "Astra 19.2E list" and blindscan.

**Panasonic**  
Viera models with an svl.bin or svl.db channel list (most models since 2011)

**Sony**  
Android-TVs "sdb.xml" files using formats "FormateVer" 1.1.0 and KDL 2012/2014 files using "FormatVer" 1.0.0, 1.1.0 and 1.2.0 

**Philips**  
Philips uses countless incompatible file formats for various TV models.
ChanSort currently supports the formats:  
- PhilipsChannelMaps\ChannelMap_45, 100, 105 and 110
- Repair\ChannelList\channellib\\*Table and s2channellib\\*.dat
- Repair\CM_TPM1013E_LA_CK.xml (sometimes that file is hidden and only a .bin file is visible)

**Hisense**  
Some older models like LTDN40D50TS use the same .csv files as Sharp. See "Sharp" for instructions.
2016 "Smart" models with a channel.db file, i.e. H65M5500  
2017 models with a servicelist.db file  
Special thanks to Hisense for supporting ChanSort with technical information and a test device!

**Toshiba**  
Models that export a .zip file containing chmgt.db, dvbSysData.db and dvbMainData.db files.  
(e.g. RL, SL, TL, UL, VL, WL, XL, YL models of series 8xx/9xx)  
Models with a settingsDB.db file

**Grundig**
Models that export files named dvb\*_config.xml.

**SatcoDX (OEM for ITT, Medion, Nabo, ok., PEAQ, Schaub-Lorenz, Silva-Schneider, Telefunken)**  
Various brands use the same hardware for DVB-S, which exports .sdx files 

**Sharp (and some models from Dyon, Blaupunkt, Hisense, Changhong, alphatronics, JTC Genesis)**
Some of these TV share similar hardware and can export/import a .csv file allowing to change the channel order.  
Blaupunkt B40A148TCSFHD, B32B133T2CSHD, ...  
Changhong LED32E2200ST2, ...  
Dyon Live 22 Pro, Live 24 Pro, ENTER 32 Pro X, ...  
Hisense LTDN40D50TS, ...  
Sharp LC-xxCFE4142E, LC-xxCFF6002E, LC-xxFI5542E, LC-xxFG5242E, LC-xxUI7552E, LC-xxUI7652E, xxBJ1E, xxBJ3E, xxBJ5E, ...  

Depending on the actual TV the import/export function can be part of the regular user menu, a hotel menu or a service menu
and the exported files may be different. Some models like Sharp Aquos xxBJ1E have a sub menu for exporting. You need to
export all 4 type of files for the import to work.  
To access the secret hotel / service menus, there are several possible key combinations on the remote control:  
hotel menu: MENU 7906 / MENU 4588  
service menu: MENU 1147 / MENU 11471147 / SOURCE 2580  
Do not make any changes in the service menu, as this could damage your TV. Only use the import/export functions.

**VDR (Linux Video Disk Recorder)**  
Supports the channels.conf file format.  
Implementation for this was provided by TCr82 from the VDR project.

**m3u (SAT>IP)**  
Supports SAT>IP .m3u files with extended information holding channel names and program numbers.

**Enigma2 (Dreambox, VU+ and many other Linux based receivers)**  
Requires a local copy of the files "lamedb", "bouquets.\*" and "userbouquet.\*" from /etc/Enigma2/.  


License (GPLv3)
---------------
GNU General Public Licence, Version 3: http://www.gnu.org/licenses/gpl.html  
Source code is available on https://github.com/PredatH0r/ChanSort

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.

IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
