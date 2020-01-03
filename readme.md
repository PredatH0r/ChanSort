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
ChanSort supports various file formats from Samsung, LG, Panasonic, Sony, Philips, Hisense, Toshiba, 
ITT/Medion/Nabo/ok./PEAQ/Schaub-Lorenz/Silva-Schneider/Telefunken, Linux VDR, **SAT>IP .m3u** (new).

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
- [Microsoft .NET Framework 4.6 or later](https://dotnet.microsoft.com/download/dotnet-framework)
- [Microsoft Visual C++ 2010 Redistributable Package (x86)](http://www.microsoft.com/en-us/download/details.aspx?id=8328):
  required to edit SQLite lists (Hisense, Panasonic, Toshiba and Samsung .zip format)
- USB stick/SD-card to transfer the channel list between your TV and PC (FAT32 file system recommended)
- Some LG models require a special service remote control to access the import/export functions (see Wiki for details)

Supported TV models 
-------------------

**Samsung**  
.scm files: B (2009)*, B (2013), C, D, E, F, H, J series  
.zip files: H, J, K, M, N and Q, R series  
Lists:  Air analog, Air digital, Cable analog, Cable digital, 
		Cable Prime, Sat digital, Astra HD+, Freesat, TivuSat,
		Canal Digital Sat, Digital+, Cyfra+

\*: The "clone.bin" format is not supported. In the "*.scm" format
the "Air Analog"-list of the 2009 B-series doesn't support all 
editing features due to a lack of test files. If you have such a file,
please send it to me.

Instructions for transferring the channel list can be found on:
https://github.com/PredatH0r/ChanSort/wiki/Samsung

**LG**  
Series: CS, DM, LA, LB\*, LD, LE, LF, LH, LK, LM+, LN, LP#, LS, LT, LV, LW, LX, PM, PN, PT, UB\*  
and all newer models using the GlobalClone.TLL format  
Lists:  Analog TV, DTV (DVB-C, DVB-T), Radio (DVB-C/T), Sat-DTV (DVB-S2), Sat-Radio (DVB-S2)

\*: Some devices behave erroneously due to firmware issues.  
+: See system requirements for LM-series. xxLM640T is not supported due to its firmware limitations.  
\#: Only satellite channels supported.

Other models might also work, but have not been tested. If you have a .TLL file of a series not listed here, please send it to me.

Instructions on how to access the hidden service-menu for transferring
the channel list from/to USB can be found in the Wiki.

**Panasonic**  
Viera models with an svl.bin or svl.db channel list (most models since 2011)

**Sony**  
Android-TVs "sdb.xml" files using formats "FormateVer" 1.1.0 and KDL 2012/2014 files using "FormatVer" 1.0.0, 1.1.0 and 1.2.0 

**Philips**  
Philips uses countless incompatible file formats for various TV models.
ChanSort currently supports 2 different versions of .xml files, other formats are not supported.

**Hisense**  
2016 "Smart" models with a channel.db file, i.e. H65M5500  
2017 models with a servicelist.db file  
Special thanks to Hisense for supporting ChanSort with technical information and a test device!

**Toshiba**  
Models that export a .zip file containing chmgt.db, dvbSysData.db and dvbMainData.db files.  
(e.g. RL, SL, TL, UL, VL, WL, XL, YL models of series 8xx/9xx)

**ITT, Medion, Nabo, ok., PEAQ, Schaub-Lorenz, Silva-Schneider, Telefunken**  
These brands use .sdx files (currently only satellite lists are supported)

**VDR (Linux Video Disk Recorder)**  
Supports the channels.conf file format.  
Implementation for this was provided by TCr82 from the VDR project.

**m3u (SAT>IP)**  
Supports SAT>IP .m3u files with extended information holding channel names and program numbers.

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
