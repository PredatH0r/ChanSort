Links
-----
[![EN](https://chansort.com/img/flag_en.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme.md)
[![DE](https://chansort.com/img/flag_de.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme_de.md) |
[![TR](https://chansort.com/img/flag_tr.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme_tr-TR.md) |
[Download](https://github.com/PredatH0r/ChanSort/releases) | 
[Documentation](https://github.com/PredatH0r/ChanSort/wiki) |
[Forum](https://github.com/PredatH0r/ChanSort/issues) | 
[E-Mail](mailto:horst@beham.biz)

About ChanSort
--------------
ChanSort is a PC application that allows you to reorder your TV's channel list.  
Most modern TVs can transfer channel lists via USB stick, which you can plug into your PC.  
ChanSort supports [file formats from numerious brands](#supported-tv-models) and can copy program numbers and
favorites from one file to another, even between different models and brands.

![screenshot](http://beham.biz/chansort/ChanSort-en.png)

Features
--------
- Reorder channels (change numbers directly, move up/down, drag&drop, double-click)
- Use another channel list as a reference to apply the same ordering
- Multi-selection for editing multiple channels at once
- Single-list view (showing assigned channels first and then all unassigned channels)
- Side-by-side view of new/sorted list and original/complete list (similar to playlist and library)
- Rename or delete channels
- Manage favorites, parental lock, channel skipping (when zapping), channel hiding
- User interface in English, German, Spanish, Turkish, Portuguese, Russian and Romanian
- Unicode character support for channel names (Latin, Cyrillic, Greek, ...)

NOT supported:
- adding new transponders or channels
- changing tuner related properties of channels (ONID, TSID, SID, frequency, APID, VPID, ...)

Some features may not be available on all TV models and channel types (analog, digital, sat, cable, ...)

! USE AT YOUR OWN RISK !
------------------------
Most of this software was written without support from TV manufacturers or access to any official 
documentation about the file formats. It is solely based on analysing existing data files, trial and error.
There is a chance of unwanted side-effects or even damage to your TV, as reported in 2 cases.

Hisense is the only manufacturer who provided technical information and a test device.

Supported TV models 
-------------------
ChanSort supports a large number of file formats, but it's impossible to tell for every brand and TV model 
what file format it uses (with may even change with firmware updates).  
This list gives some examples of what should be supported, but even if your model or brand is not in this list, 
it may work anyway:
- [Samsung](source/fileformats.md#samsung)
- [LG](source/fileformats.md#lg)
- [Sony](source/fileformats.md#sony)
- [Hisense](source/fileformats.md#hisense)
- [Panasonic](source/fileformats.md#panasonic)
- [Philips](source/fileformats.md#philips)
- [Sharp, Dyon, Blaupunkt, Hisense, Changhong, Grundig, alphatronics, JTC Genesis, ...](source/fileformats.md#sharp)
- [Toshiba](source/fileformats.md#toshiba)
- [Grundig](source/fileformats.md#grundig)
- [SatcoDX: ITT, Medion, Nabo, ok., PEAQ, Schaub-Lorenz, Silva-Schneider, Telefunken, ...](source/fileformats.md#satcodx)
- [VDR](source/fileformats.md#vdr)
- [SAT>IP m3u](source/fileformats.md#m3u)
- [Enigma2](source/fileformats.md#enigma2)

System requirements
-------------------
**Windows**:  
- Windows 7 SP1, Windows 8.1, Windows 10 v1606 or later, Windows 11 (with x86, x64 or ARM CPU)
- [Microsoft .NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework)
- The .NET FW 4.8 does NOT work with Windows 7 without SP1, Windows 8 or Windows 10 prior to v1606

**Linux**:  
- wine (sudo apt-get install wine)
- winetricks (sudo apt-get install winetricks)
- start winetricks, select or create a wineprefix (32 bit or 64 bit), select
  "Install Windows DLL or component" and install the "dotnet48" package and ignore dozens of message boxes
- right-click on ChanSort.exe and select "open with", "all applications", "A wine application"

**Hardware**:
- USB stick/SD-card to transfer the channel list between your TV and PC. A stick <= 32 GB with FAT32 file system 
is STRONGLY recommended. (Some TVs write garbage to NTFS and don't support exFAT at all)

Build from source
-----------------
See [build.md](source/build.md)

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
