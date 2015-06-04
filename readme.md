Links
-----
[Download](https://github.com/PredatH0r/ChanSort/releases) | 
[Documentation](https://github.com/PredatH0r/ChanSort/wiki) |
[Forum](https://github.com/PredatH0r/ChanSort/issues) | 
[E-mail](mailto:horst@beham.biz)

About ChanSort
--------------

ChanSort is a PC/Windows application that allows you to reorder your TV's channel list.  
Most modern TVs have functions to export/import a channel list to/from USB stick or SD-card, 
which you can plug into your PC.  
ChanSort currently supports various models from Samsung, LG, Panasonic, Toshiba and the Linux VDR project.

![screenshot](http://i.imgur.com/QEvqcbl.png)

Features
--------

- Reorder channels (change numbers directly, move up/down, drag&drop, double-click)
- Use another channel list as a reference to apply the same ordering
- Multi-selection for editing multiple channels at once
- Side-by-side view of new/sorted list and original/complete list (similar to playlist and library)
- Rename or delete channels
- Manage favorites, parental lock, channel skipping (when zapping), channel hiding
- User interface in English, German and partially in Turkish and Portuguese
- Unicode character support for channel names (Latin, Cyrillic, Greek, ...)

Some features may not be available on all TV models and channel types (analog, digital, sat, cable, ...)

! USE AT YOUR OWN RISK !
------------------------
This software was written without support from any TV manufacturer nor access to any official 
documentation about the file formats. It is solely based on analysing existing data files, trial and error.
There is a chance of unwanted side-effects or even damage to your TV, as reported in 2 cases.

System requirements
-------------------

- [Microsoft .NET Framework 4.0 (Full)](http://www.microsoft.com/en-us/download/details.aspx?id=17851): 
  included in Win8, required for Win7, Vista and XP SP3
- [Microsoft Visual C++ 2010 Redistributable Package (x86)](http://www.microsoft.com/en-us/download/details.aspx?id=8328):
  required to edit SQLite lists (Panasonic, Toshiba and Samsung J-series)
- USB stick/SD-card to transfer the channel list between your TV and PC
- Some LG models require a special service remote control to access the import/export functions (see Wiki for details)

Supported TV models 
===================

Samsung 
-------
Series: B (2009)*, B (2013), C, D, E, F, H, J+  
Lists:  Air analog, Air digital, Cable analog, Cable digital, 
		Cable Prime, Sat digital, Astra HD+, Freesat, TivuSat,
		Canal Digital Sat, Digital+, Cyfra+

\*: The "clone.bin" format is not supported. In the "*.scm" format
the "Air Analog"-list of the 2009 B-series doesn't support all 
editing features due to a lack of test files. If you have such a file,
please send it to me.

+: currently only DVB-C and DVB-S are supported (not DVB-T and analog channels)

Instructions for transferring the channel list can be found on:
http://www.ullrich.es/job/sendersortierung/senderlisten-samsung-tv-exportieren-importieren/

LG
---

Series: CS, DM, LA, LB\*, LD, LE, LH, LK, LM+, LN, LP#, LS, LT, LV, LW, LX, PM, PN, PT, UB\*  
Lists:  Analog TV, DTV (DVB-C, DVB-T), Radio (DVB-C/T), Sat-DTV (DVB-S2), Sat-Radio (DVB-S2)

\*: Some devices behave erroneously due to firmware issues.  
+: See system requirements for LM-series. xxLM640T is not supported due to firmware limitations.  
\#: Only satellite channels supported.

Other models might also work, but have not been tested. If you have a .TLL file of a series not listed here, please send it to me.

Instructions on how to access the hidden service-menu for transferring
the channel list from/to USB can be found in the Wiki.

Panasonic
-------
Viera models with an svl.bin or svl.db channel list (most models since 2011)

Toshiba
-------
Models that export a .zip file containing chmgt.db, dvbSysData.db and dvbMainData.db files.  
(e.g. RL, SL, TL, UL, VL, WL, XL, YL models of series 8xx/9xx)

VDR (Linux Video Disk Recorder)
-------
Supports the channels.conf file format.  
Implementation for this was provided by TCr82 from the VDR project.

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
