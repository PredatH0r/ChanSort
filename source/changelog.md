ChanSort Change Log 
===================

2023-01-23
- added support for Hisense HIS_DVB.BIN channel lists

2023-01-18
- added support for Hisense HIS_SVL.BIN / HIS_TSL.BIN / HIS_FAV.BIN channel lists

2023-01-15
- added support for Vision EDGE 4K set-top-box (DVB-S only)
- TCL: separate lists for DVB-C/T/S, each starting at 1
- TCL: cleaned up "Hide" vs "Skip"
- SDX: showing message when an unsupported, encrypted .sdx file is opened (e.g. from Xoro STB)

2023-01-12
- TCL: fixed upload failure due to incorrect checksum when DtvData.db was larger than 307200 bytes

2023-01-10
- TCL: fixed deleting channels

2023-01-08
- TCL/Thomson .tar: custom implementation for reading/writing .tar archives, preserving all 
  unix file metadata (based on "old-GNU" .tar flavor, like the files exported by the TV)

2023-01-06_1450
- .HDB: added support for "Hide"-flag, added Skip/Lock/Fav for TechniSat DVB-C file format

2023-01-06_1420
- fixed TCL .tar files: now saving names as BLOB instead of VARCHAR(64)

2023-01-06
- added support for numerous .DBM file formats used by DVB-C and DVB-S receivers based on 
  MStar chips. Known brands to use this format include Xoro, TechniSat, Strong, Comag and
  many more. (If your file isn't recoginezed, please send it to me to add support for it)

2023-01-04
- TCL/Thomson: improved file detection (.tar file or directory containing DtvData.db, 
  satellite.db, cloneCRC.bin)
- m3u: #EXTINF tag data is displayed in "Short Name" column and can be edited
- m3u: fixed saving #EXTINF lines containing tag data
- m3u: readded "File / Save as" menu item (but not for other types of lists)

2023-01-03
- added support for TCL / Thomson \*.tar channel lists (containing DtvData.db and satellite.db)
- updated hotbird reference list for Italy
- fixed text input for "Favorites" column (where applicable)
- fixed: menu "settings / allow editing predefined lists (DANGEROUS)" unlocked the edit functions
  in the user interface, but changes to those lists were not saved to disk.
- changed build process to use MSBuild project files (to allow future switch to .NET 7 or 8)

2022-12-04
- fixed: various .xml file formats could not be loaded anymore
- channels can also be swapped now be directly selecting two rows and clicking on "swap"
- swapping mark (strike through) is now automatically removed after swapping
- swapping is now recognized as a change and will prompt to save the list on exit

2022-11-30
- fixed Samsung .zip lists changing channel names to "Chinese" characters in the saved file
  (caused by a breaking change in the new version of the Microsoft.Data.Sqlite library)
- changing channel names in Samsung \*\_1242.zip format is disabled due to new Sqlite library

2022-11-29
- fixed saving of modified cmdb_\*.bin channel lists
- removed "Save as" function (which was disabled for most channel list formats)
- added "Convert list" menu item which gives a hint on how to use reference lists instead
- added functions for easy swapping in 1-list-view (mark for swapping, swap with marked)
- LOTS of internal changes

2022-11-22
- fixed "Export to Excel" (copies the list as tab-separated text into clipboard)
- included latest translation to Polish (thanks to J.D.)
- reorganized File menu and tool bar
- allow renaming channels in SatcoDX channel lists (\*.sdx)
- improved support for Panasonic LS 500 / LX 700 series

2022-11-14
- fixed issue loading Loewe servicelist.xml file containing empty service-ids

2022-10-06
- added support for Android based Panasonic LS and LX 500-700 series (lists with a /mnt/.../tv.db file)
- fixed reference list dialog not showing any controls on small screens with and large scaling factor
- fixed "NullReferenceException" while applying a reference list based on a SQLite database file 
  (\*.db or Samsung .zip) which contained NULL values for channel names instead of empty strings.
- function to remove backup files (so that the next "File / Save" operation will create a new backup)
- updated Hotbird 13.0E / Tivùsat reference lists for Italy

2022-07-03
- experimental support for Grundig and ChangHong atv\_cmdb.bin files (analog antenna and cable TV)
- Reference list dialog further optimized to fit 1920x1080 @150% or 1024x768 @100% screens
- re-imported translation file (to fix incorrect letters in language names like "Русский")

2022-04-19
- added support for Orsay .zip files which contain a Clone/map-AirD and other files similar to Samsung .scm
- Reference list dialog is now resizable
- Grundig dvb\*_config.xml lists are now separated into TV and Radio lists with individual ordering
- ability to load Grundig dvb\*\_config.xml files containing invalid XML characters (e.g. 0x10)

2022-04-11
- added ChangHong/Chiq L32H7N dtv_cmdb_2.bin format (4419 KB size)

2022-03-20
- Philips \*Table and \*.dat format: now showing "Encrypted" information
- added ChangHong LED40D3000ISX dtv_cmdb_2.bin format (1489 KB size)
- added experimental support for Loewe servicelist.xml format
- added polarity information to Excel export (and changed column order slightly)
- added missing Romanian translation files to the .zip


2021-10-24
- LG webOS 5 and 6: Improved support for DVB-C lists which changed channel numbers after import
  (Now setting the "userEditChNumber" and "userSelCHNo" flags when channel numbers are changed.
  This can be disabled in ChanSort.Loader.LG.ini, section \[webOS 5\], setting set_userEditChNumber=false)
- Sat>IP/.m3u: Support for files with extra information after the #EXTM3U header
  Also capturing the group-title from #EXTINF, showing the msys value in the Source column (dvbs/dvbt/...), 
  and showing all URL-parameters in the Debug column
- Panasonic .xml: files with "&" characters in channel names can now be loaded 
  (Panasonic does not escape special XML characters and produces invalid XML syntax) 

2021-09-23_1945
- Philips: disabled deleting of channels for ChannelMap\_100 - 115, except for version 100 without any .bin files.
  (Lists with .bin files require that the .xml file contains all the same channels to override all channel numbers)
- Panasonic: improved symbol rate and satellite position detection
- startup window location and size are adjusted to fit on screen

2021-09-23
- improved per-monitor DPI scaling (i.e. moving the window from a Full-HD to a 4K display with 100% and 200% scale)
- improved "column auto width" (right clicking a column header) by forcing line breaks in the column captions
- Philips: improved and re-enabled favorite list editing for FLASH\_\*/\*.db lists
- Panasonic: fixed display of symbol rate and satellite

2021-09-22
- Philips: added support for CM\_*.xml variant that uses a <ECSM> root element around the <ChannelMap>
- Philips: ability to read/write broken CM\_*.xml files that contain channel names with an unescaped & character
- Philips: enabled write mode for Repair\\FLASH\_\*/\*.db file format (one variant was confirmed to work)
  Favorite lists for this format are disabled for now (TV ignored them).
- Panasonic: importing a modified svl.bin file caused the TV to use case-sensitive sorting when using the 
  function to list the names sorted alphabetically. This is now fixed.
   
2021-09-19
- Philips: added support for ChannelMap_115 format
- Philips: ChannelMap formats 100-115 did not always fill "Source" and "Polarity" columns correctly
- Philips: improved experimental support for Philips FLASH\_\*/\*.db file formats
  (read-only by default, can be enabled in Philips.ini for testing)
- added Polish readme and updated translation (by JakubDriver)
- Panasonic: added exerimental support for channel\_list.xml lists (Panasonic Android TVs, 2019 and later)
  Unfortunately the only information included in this format is the channel number and a truncated channel name.
- column order is now preserved between program starts even when lists with different supported columns
  were loaded and columns reordered.
- added option to enabled/disable auto-loading of the last opened list when starting the program
- added UI option "Hide/unhide columns automatically". When turned off, the program will no longer hide columns
  automatically based on the selected list. It is recommended to leave this setting turned on.

2021-09-07
- added turkish readme
- Reference lists can now also be applied to a particular favorites list as the target
  e.g. copy the main program numbers from a Samsung list to the "Fav A" list of a Philips TV

2021-09-06
- Philips: fixes for ChannelMap_100, 105 and 110 formats
- Philips: support for FLASH/*.db DVB-T/C and preset DVB-S lists (mgr_chan_s_pkg.db)
- Toshiba: lists with chmgt_type001\\chmgt.bin can now be opened without zipping them
- Toshiba: selecting the hotelopt_type001.bin will now also load the list (if the type is supported)
- Alden: added support for "Alden" Android SmartTV channel list format (dvr_rtk_tv.db)

2021-09-05_2010
- Philips: fixed wrong .ini settings for formats 100, 105 and 110

2021-09-05
- ChanSort didn't work properly with Windows region settings that don't convert ".BIN" to lowercase ".bin" (e.g. Turkey)
- Philips: added ChanSort.Loader.Philips.ini file to try out different configurations until working ones get confirmed. 
- Philips: disabled deleting channels for most file formats (they require files for menu and tuner to be kept in-sync).
- Philips: identified several variants of ChannelMap_100 format which now have special handling.  
  - one that exports \*cmdb\*.bin files is now fully tested and working.  
  - ones that export only .xml files inside the channellib and s2channellib folders should work too, but not confirmed.  
- Philips: ChannelFormat_105 and 110 specific settings in .ini, currently best-effort without user confirmation.
- Philips: added support for Repair\\mgr_chan_s_fta.db lists. Can be read as a reference list, but editing is
  currently disabled in the .ini file (enabling it is experimental)
- added experimental support for 8 variants of "dtv_cmdb_2.bin" DVB-S channel lists (DVB-C/T not supported yet).  
  Brands known to use this format include Sharp, Toshiba, Dyon, OK.  
  Philips also uses this format as part of ChannelMap_100, but only for the tuner data and uses .xml for the menu.  
  All variants need specific configuration in the ChanSort.Loader.CmdbBin.ini file. If your file is not supported yet,
  please send it to me via github or email.
- Sony: Files with incorrect checksum are no longer rejected, as the TV seems to ignore bad checksums.  
  Information about a bad checksum is visible under File / File Information. 
- Sony: Sky option channels are now in the TV channel list rather than data channel list
- LG: added distinction between analog cable and analog antenna channels for legacy binary files (xxMODEL00001.TLL)
- updated Hungarian translation. Thanks to efi99 on Github!
- pressing the "Del"-key on the keyboard no longer deletes a channel when a text editor is open
- dragging a file (or something else) from outside ChanSort over the ChanSort window no longer creates an error
- opening a file containing a read-only channel list now shows the original numbers

2021-07-27
- Philips ChannelMap\_100: fixed reading/writing favorites
- updated Swiss reference lists with new ONID-TSID-SID for SRF info HD and RSI LA HD
- improved Linux/Wine performance
- "File / File information" now ignores deleted channels when counting duplicates

2021-07-26
- user interface can now be toggled between 
   - **split view**: classic ChanSort UI with new/ordered and old/full list side-by-side
   - **single-table**: simplified and more intuitive UI, but not quite as powerful
  When you choose to "Modify current list", the single-table view is used by default,
  otherwise the split view. But you can always toggle between them.
- added option to select a color theme. The UI now uses the "Office 2019 Colorful" theme by default,
  the old theme was "Office 2010 Blue".
- improved many keyboard shortcuts (open the drop-down menus to see the shortcuts)

2021-07-25
- DVBS_Program.csv, DVBS_CHANNEL_TABLE.csv, MSxxxx_DVBS_CHANNEL_TABLE.csv: 
   Various versions of these files are used by Blaupunkt, Dyon, Hisense, Sharp and many others
   - added support for Hisense .csv files with 6 columns including "channel number" and "LCN"
   - using separate lists for DTV, Radio and Data, which all start with 1
- Panasonic: SAT>IP channels were incorrectly added to the DVB-S channel list
- added [build.md](build.md) with info on how to build ChanSort from source without a DevExpress WinForms license

2021-07-18
- added support for Hisense Vidaa U5.2 format (servicelist.db with different table and column names)
- applying reference lists did not work in many cases (depending on the type of reference list file and channel list)
- support to run as 64bit application on Intel/AMD/ARM CPUs (e.g. Windows 10 in a VM on an Apple M1 CPU)

2021-06-13
- improved High-DPI support

2021-05-01_1615
- fixed issue with high-res displays / Windows display scaling other than 100% aka 96dpi, that caused columns to
  become wider every time the program was started.

2021-05-01
- added "Settings / Reset to defaults and restart" function to delete the stored customized settings in case something
  went wrong (like massively oversized column widths)
- Philips ChannelMap\_30: fixed error when trying to save this type of list
- Upgraded to DevExpress WinForms 20.2.7 user interface library

2021-04-25
- removed dependency on Visual C++ 2010 Redistributable Package (x86):
  Hisense, Panasonic, Philips, Samsung and Toshiba channel list use SQLite database files, which ChanSort accessed
  through a 3rd party ADO.NET data provider, which depended on this package. Now a different ADO.NET provider is used.
- Philips DVB\*.xml: maintain same indentation as original file (can be 0, 2 or 4 spaces) for easier before/after diff
- Philips ChannelMap\_30-45: fixed a bug that caused the list not to load when certain error messages were logged

2021-04-11_1900
- Philips ChannelMap\_30: fixed bug that caused favorite lists to be in wrong order

2021-04-11
- Philips: added support for ChannelMap\_30 format
- LG Web OS 5: fixed a bug that wrote wrong values for "audioPid" to the file (which had no effect on the TV's operation)

2021-04-10
- Samsung .zip: Support for files that contain an empty SRV_EXT_APP table, which caused the whole list to show up empty.

2021-04-02_1734
- Philips: another fix for lists with missing s2channellib\\DVBSall.xml file

2021-04-02
- Philips: skip read-only files when loading the list, so they don't cause errors when the list is saved (e.g. DVBSall.xml)

2021-03-28_1809
- LG webOS 5: fixed handling of channels with an ampersand (&) character showing as "&amp;" and not matching
  a text-only reference list.

2021-03-27
- SatcoDX (.sdx): fixed handling of format version 105, which contains trailing data after the last channel
- SatcoDX: changing the character set in the menu now instantly corrects channel names with non-ASCII characters

2021-03-17
- improved reference list channel matching: if multiple channels match the same name, then the candidates are narrowed
  down by service type (TV/radio/data - if known), case sensitivity (upper/lowercase), encryption (unencrypted first

2021-03-16
- Sharp, Dyon, Blaupunkt, ...: added support for DVBS_Program.csv and \*DVBS_CHANNEL_TABLE.csv files
- Enigma2: added support for Linux based Set-Top-Boxes (Dreambox, VU+, Octagon, ...) using lamedb and bouquets
- Toshiba settingsDB.db: support for lists without analog tuner data (missing TADTunerDataTable)
- Grunding: failed to load lists where the <Digital> element did not contain a <channels> child element
- refrence lists can now be applied to main channel numbers (as before) or to a specific favorite list (new)

2021-02-24
- Philips ChannelMap\_45: TV did not remember last selected favorite list when first fav list was created by ChanSort.
- Philips ChannelMap\_100 and later: "Channel" XML elements inside the DVB\*.xml files are now reordered by program nr.
- Philips ChannelMap\_105 and 110: fixed saving favorite lists (keeping FavoriteNumber="0" in DVB\*.xml and only 
  setting the numbers in Favorites.xml)
- m3u: keep original end-of-line characters (CRLF or LF)
- m3u: detect whether channel names are prefixed with a program number or not, and save the file in the same way.

2021-02-17_2
- Philips ChannelMap\_105 and 110: fixed broken favorites.xml file and DVB\*.xml when channels were renamed

2021-02-17
- Panasonic: fixed error reading lists with channels that refer to non-existing transponders
- Philips ChannelMap\_45: incrementing fav list version number when saving and setting the last\_watched\_channel\_id to
  the first channel in the fav list (ensuring that the channel is actually present in the list)
- UI: added search button (because it is not obvious that the top row of the table is a search/filter row)

2021-02-09
- Sony: fixed incorrect checksum error for Android based TVs which use CR+LF as line separators (normally only LF is used)
- Philips: show info that it may be necessary to unplug and reboot the TV after the import
- Philips ChannelMap\_45: show info when there are checksum errors, which indicate that TV's internal list is broken
  and a rescan is required in order to properly export/import the list.

2021-02-08
- Philips: lists with a chanLst.bin file show information about file format version and TV model under File / Information
- Philips ChannelMap\_45: fixed handling of favorite lists (allow up to 8 lists, empty ones get removed automatically)
- Philips ChannelMap\_45: no longer prompting to reorder channels sequentially (to close gaps). 
  (This feature caused DVB-C/T list to only contain odd numbers and DVB-S to contain only even numbers, when both exist)
- Philips ChannelMap\_45: added display for service type (TV/radio), encryption, sat frequency polarity
- Philips ChannelMap\_45: fixed display of DVB-C/T frequency
- Philips Repair\chanLst.bin (1.x): fixed sat frequency display for transponders with vertical polarity

2021-02-05
- Philips ChannelMap_100 and later: keeping original indentation in XML files 
  and original number of bytes for hex-encoded Unicode names (channel name, fav list names)
- Philips ChannelMap_110: setting the "UserReorderChannel" flag in the file to 1
- Philips ChannelMap\_45: fixed error when channel names did not match between tv.db and Cable/Terrestrial/SatelliteDb.bin

2021-01-31
- Philips ChannelMap\_45: fixed bug writting "channel edited" indicator to the wrong location inside the file
- Philips ChannelMap\_45: fixed display of wrong frequency
- Philips ChannelMap\_45: added support for favorite lists

2021-01-24
- fixed issues with applying reference lists (especially .m3u)

2021-01-23
- Toshiba: added support for settingsDB.db lists
- SatcoDX (*.sdx format used by ITT, Telefunken, Silva-Schneider, ...): minor bug fixes

2021-01-17
- Philips: added support for ChannelMap\_45 format
- Philips: fixed display of symbol rate and frequency (off by factor 1000 depending of list and DVB source)
- Philips: fixed special characters in channel names (e.g. german umlauts)
- Philips: "ServiceType" now only shows "TV" or "Radio". There is no information about HD/SD in the file.

2021-01-02
- Grundig: added support for dvb*_config.xml channel lists

2020-12-29
- update check could not distinguish between 2 program versions from the same day (kept showing "an update is available")

2020-12-26_2
- Panasonic: channel name editing is now supported when all channels implicitly use valid utf-8 encoding

2020-12-26
- LG WebOS 5: added warning that support is only experimental.
- Panasonic: Channel name editing is now supported for svl.bin files (unless there is no indicator what encoding to use)
- Hungarian translation: added missing files to .zip

2020-12-05
- Philips: Fixed error saving the Favorite.xml file (effects Philips "ChannelMap_105" and later file format versions)
- added Hungarian translation (credits to Istvan Krisko)
- Sony XML: fixed display of wrong DVB-C cable channel/transponder number
- LG webOS 5: unfortunately no news yet, but the insight that some "CX" models run on webOS 3.6 and others on 5.1, using
  different file formats. 

2020-11-16
- Philips: TV rejected modified lists because checksums inside chanLst.bin were not updated. This is now fixed.
- LG WebOS 5: fixed handling for deleted satellite radio channels (some TVs expect majorNumber 0, others 16384)
- "Open File Dialog" now works again when double-clicking on a shortcut to a directory (.lnk file).

2020-08-27
- LG WebOS 5: added support for lists with analog cable/antenna channels
- Philips: added support for analog channel lists (Repair/CM_* format)
- GB Freesat reference lists updated

2020-08-10
- Philips: added support for models that export a Repair/ChannelList/channellib/*Table and s2channelLib/*.dat  folder 
  structure, e.g. PFL4317K, PFL5507K, PFL5806K, PFL7656K

2020-08-03
- Philips: older models which export a Repair/*.BIN file can now be loaded, when there is an invisible .xml file in the same
  directory. 
  (Philips exports the .xml file with file attributes "hidden" and "system", which makes them invisible to Windows Explorer)
- upgrade to DevExpress 20.1.6

2020-07-13
- Samsung 1242 format: channel names were displayed as chinese letters instead of latin
  (Names are not stored as characters in this format, but instead 16 bits of UTF16 code points are encoded as "payload"
  inside 3 byte UTF-8 sequences)

2020-07-12
- added UTF-16 Big Endian and Little Endian options to character set menu
- Samsung .zip loader: auto-detect UTF-16 endianness and allow to change encoding after loading to UTF-16 LE/BE
  (some files use Little Endian format and show chinese characters when loaded with the default Big Endian format)
- Customized column order is now preserved across file formats and input sources
- Note about LG WebOS 5 files (e.g. CX series):
  It is still unclear what exact firmware version and conditions are needed to properly import a channel list.
  Users reported about varying success of an import, reaching from not possible at all, only after a factory reset, 
  importing the same list twice or working just fine.
  The problems is not related to ChanSort, as it can be reproduced by exporting a list to USB, swapping channels 
  in the TV's menu and trying to loading the previously exported list back. The TV may keep the swapped channels and 
  show inconsistencies between the channel list in the settings menu and the EPG.

2020-05-15 (inofficial)
- fixed more issues with the LG WebOS 5.0 format

2020-05-11
- improved support for LG OLED series (CX) - now supports lists with multiple sources (DVB-C, DVB-S), but still no favorite lists
- added info screen when opening an empty LG channel list which is most likely caused by selecting a predefined list
  during the TV setup or channel search
- fixed: Sony KDL channel lists were not saved correctly
- upgrade to DevExpres 20.1.3

2020-05-03
- added experimental support for LG WebOS 5.0 (e.g. OLED CX series) - NO FAV LISTS YET

2020-05-02
- added Turkish translation (thanks to Ali Haykir)
- Philips: combined DVB-C and DVB-T into a single list with a common number domain
- added exprimental support for Samsung "iptv" list
- added Suiss reference lists with Astra 19.2E + Hotbird 13.0E channels
- fixed "System.ArgumentOutOfRangeException..." when opening a file which supports mixed-source favorite lists 
  (Sony, Philips, Hisense, ...)
- System requirements changed: .NET Framework 4.8
- added high-DPI support
- added Accessibility menu options to change the UI font size

2020-03-20
- another attempt to get Samsung 1352.0 format working

2020-03-15
- repackaged to include Polish translation files

2020-03-14
- check for updates can now handle multiple updates on a specific day
- fixed applying favorites from a reference list (it showed fav letters on the channels, but the fav lists were empty)
- added Polish translation (thanks to Jakub Driver!)
- potential fix for Samsung 1352.0 format, which can contain channels marked as deleted 

2020-02-11 (re-upload)
- fixed: removing channels from a favorite list caused incorrect reordering

2020-02-11
- Philips: show and edit customized titles of favorite lists
- fixed non-unique numbers in mixed-source favorite lists when using "Add to Fav A" (Panasonic, Hisense, Sony, Philips)
- function to reorder channels from 1-x is now reordering all channels when only a single one was selected
- function to sort channels by name is now reordering all channels when only a single one was selected

2020-02-02
- fixed (hopefully): When channels were deleted from Sony lists, the TV reordered the list randomly after a reboot

2020-01-02
- added support for m3u lists (SAT>IP, VLC, WinAmp, ...)
- added support for Hisense H50B7700UW (and maybe others which use the same favorite list table schema)
- fixed support for Philips lists with format 100
- fixed missing DLLs with spanish translation
- fixed polarity display for Samsung (caused by a stale .ini file in the package)
- disabled "Lock" toggle button when the list does not support parental locks

2020-01-01
- fixed loading of Samsung .scm files (Samsung.ini file was missing in the release package)
- added "polarity" information for Samsung .scm and .zip files

2019-12-31
- fixed error when opening reference list dialog

2019-12-29
- Added Spanish translation (thanks to Marco Sánchez!)
- Added support for Philips "ChannelMap" favorites lists (1-8)

2019-11-24
- LG GlobalClone: Favorites were not loaded correctly into ChanSort
- LG GlobalClone: some changes that might fix problems where the TV didn't work properly
  with an importet list (ChanSort now modifies less data in the file)
- Menu items for hide/unhide, skip/unskip, lock/unlock are now disabled when these features are not supported by the
  channel list file format
- Applying a .txt reference list (which doesn't contain information about skip/lock/hide) will no longer clear these
  flags in the current channel list

2019-11-18
- Philips: fixed file detection in some ChannelMap_xxx folder structures

2019-11-17
- Philips: Improved support for ChannelMap_xxx channel lists directory structure.
  Selecting any .xml or .bin file in the folder will now load all DVB\*.xml files from the 
  channellib and s2channellib sub folders.

2019-11-11
- LG hospitality TVs using files names like xx[modelname].TLL can now be loaded
  (They use the naming pattern of binary TLL files, but contain GlobalClone/XML text data)
- fixed: Philips DVBC.XML files with frequency values in Hz instead of MHz caused an overflow exception

2019-11-10
- Sony: added support for independent favorite list ordering for Android channel lists (n)
- fixed: failed to save Sony lists which contain channel numbers above 8000

2019-11-08
- improved handling for deleting channels across all file formats:
  Depending on what the actual file format supports, one of the following actions will be taken.
   - channels are marked as deleted in the data records (so they will not be auto-added as new channels by the TV) 
   - channels are removed from the file (with the risk of the TV auto-adding them like new channels)
   - appended at the end of the list, when possible marked as "hidden"

2019-08-29
- fixed: some UHD channels did not show up in the list, which caused corrupted Panasonic channel lists
- fixed: Samsung SCM DVB-T lists did not show radio channels
- fixed: print caption of a favorites list was off by a letter (printed "Fav B" for the fav-A list)
- internal restructuring and added automated unit-test for most file formats

2019-08-13
- LG GlobalClone: added support for additional favorites (A-H) and individual fav sorting when supported by the TV
- LG GlobalClone: data/option channels were not listed before and are now shown in the TV channel list
- added function to copy list to clipboard (which can then be pasted into Excel or other programs)

2019-08-11
- Sony: DVB-T and DVB-C lists are now separated into "TV", "Radio" and "Other" lists, each with their own unique numbering
- Samsung ZIP: deleting a channel now really deletes it from the file, instead of marking it as deleted
  and assigning -1 as channel number (which appears as 65535 on some models)

2019-08-05
- added partial support for Philips .xml channel lists
  (There are MANY different file formats, only a few are currently supported)
- fixed "most-recently-used" getting reversed every time the program was started
- added "UTF-8 (Unicode)" character set to menu
- fixed disappearing columns when loading different channel lists without restarting the application

2019-07-25
- fix: Application failed to save config and didn't exit when the folder %LOCALAPPDATA%\ChanSort doesn't exist

2019-07-20
- user settings are now persisted across releases in %LOCALAPPDATA%\ChanSort\config.xml
- fixed Sony sdb.xml DVB-T channel lists
- added support for ITT, Medion, Nabo, ok., PEAQ, Schaub-Lorenz and Telefunken (same .sdx format as Silva-Schneider)
- fixed using wrong loader when the file type was manually selected in the "Open File" dialog

2019-07-18
- fixed support for Sony "FormateVer 1.1.0" DVB-C channel lists

2019-07-16
- added support for various Sony sdb.xml channel list formats
- added option to disable check for program updates
- fixed 200 Mhz offset for DVB-C frequencies (Samsung SCM)

2019-07-14
- added support for Silva-Schneider .sdx channel lists

2019-07-13
- support for channel file name as command line argument
- added explorer integration option in settings menu (to register file types)
- added drag/drop support to drag a file from windows explorer onto chansort to open it
- added reference list for vodafone cable TV in Berlin
- added missing DLL required for printing
- upgrade to DX 19.1

2019-02-12
- fixed "hidden" flag for Samsung C/D/E/F series (.scm lists)

2019-02-10
- fixed "delete channels", which were added at the end regardless of the user selection
- fixed handling of SCM lists where the value of the "hidden" flag is 255 (using best-guess based on another field)
- changed logic how channel lists can control which columns are visible in the UI
- added column for PcrPid
- removed column AudioPid from SCM lists
- fixed saving VDR lists after applying a reference list which contained no longer existing channels (cast exception for the proxy ChannelInfo record)
- no longer load samsung .zip lists with invalid internal folder structure (all files must be in the root folder of the .zip)

2019-02-06
- fixed deployment script to include DevExpress 18.2 DLLs, which are required to run ChanSort

2019-02-05
- upgraded to DevExpress 18.2
- hide VPID and APID columns for Panasonic (no data provided)
- Samsung J lists could have a \0 character at the end of a unicode16 string, which caused "Copy to Excel" to truncate the text
- fixed new-version check to work with github (only supports TLS 1.2 and newer now, not SSL 3.0)
- added pull-request with GB Sky reference channel list

2017-11-30
- fixed: deleting channels and selecting to "Remove unsorted channels"
  when saving could produce problems loading the list on the TV.
  i.e. LG GlobalClone.TLL

2017-11-13
- Samsung .zip: fixed loading/saving of favorites A-E

2017-10-29
- Show popup with download link for MS Visual C++ 2010 x86 Redist
  (this package is needed to open lists with a SQLite file format)
- Show diagnostic information when no plugin was able to load a file
- Samsung .zip: improved detection of transponder data
- Samsung .scm: user defined transponders now have priority
- Samsung .scm: DVB-T and DVB-C lists now support Skip and Hide flags
  (DVB-S lists still don't)

2017-06-08
- added experimental support for Loewe / Hisense 2017 servicelist.db 
  file format
- show error message when trying to open a .zip file that doen't contain
  the expected files of a Samsung J series or Toshiba .zip channel list
- show error message when trying to open a broken .zip file, which is
  most likely caused by exporting to a USB stick formatted with NTFS
- allow changing the "crypt" flag for Samsung .scm lists
- less reliable on file name / extension to detect file format
  (trying all loaders which support the file extension until one can
   successfully read the file)

2017-01-26
- added Czech translation (thanks to Pavel Mizera)
- fixed error when opening latest Hisense channel.db file format

2016-08-10
- fixed saving of LG GlobalClone format (LH series and others)
- fixed wrong .scm format detection when file was renamed by user

2016-05-07
- added support for Hisense channel.db file format
- text reference lists (.txt, .csv, .chl) can now be opened and edited
  just like TV data files.
- added dialog for advanced reference list support to apply partial 
  lists or from different input sources.
- swap 2 channels by selecting one in the left list and double-click
  another one in the right list
- inserting channels now also works with channels that already have a
  new number assigned

2015-11-29
- Samsung E,F,H,J .scm file format: allow independant reordering of each 
  favorites list
- Samsung J .zip file format: predefined lists can be edited again, allow
  independant reordering of each favorites list, allow deleting channels

2015-11-28
- fixed file format detection for Samsung J models with .scm file extension

2015-11-27
- disable editing of predefined channel lists (based on LCN). 
  TVs can show erratic behavior when a predefined list is modified.
  e.g. Samsung J built-in "Astra 19.2E" list, LG "Astra" or "Sky" lists, ...
  (can be overridden in the Settings menu)
- new skin

2015-11-26
- Samsung J series: file detection changed from channel_list_t\*.zip to \*.zip
- Toshiba: file detection changed from \*.zip to Hotel\*.zip
- LG GlobalClone: favorites are now saved to the file
- Added comment to info screen when opening LG LB/UB series GlobalClone list

2015-10-15
- Samsung J series: fixed error when saving certain lists which don't
  contain an "SRV_DVB_EXT" table.
- Panasonic: allow to edit the "Encrypted" flag, which is sometimes
  set incorrectly during the channel search.

2015-09-19
- Samsung J series: fixed deleting of channels
- LG GlobalClone: modified channel names were not written to the file
- LG GlobalClone: ask whether the conflicting xx*.TLL files should be
  renamed so that the TV can import the GlobalClone file.
- LG GlobalClone: improved support for old file format with may 
  have caused errors due to invalid XML characters inside the file.
- Panasonic: re-enabled channel lists with gaps in the numbers
- Update-Check: looking for latest version at github.com
- Detecting corrupted files with 0 size or all bytes with value 0

2015-06-13
- when appending unsorted channels during save, they are now set to
  "hidden" and "skipped/unselectable"
- reference lists: the satellite orbital position is no longer used
  to match channels. (Samsung J series does not provide that info).
- Samsung J series: favorite lists are no longer individually sortable.
  (The same Pr# is used for all favorite lists).
- Samsung J series: deleting channels now physically removes them from
  the file. (The TV might automatically append them again when it finds
  them in the DVB data stream).
- Samsung J series: editing of channel names is now enabled.
- Samsung J series: favorite E is now also available 

2015-06-05
- added support for Samsung J-Series DVB-T and analog channel lists
- fixed reference lists with Samsung J-Series
- web site and source code moved to http://github.com/PredatH0r/ChanSort

2015-04-18
- added support for Samsung J-Series DVB-C and DVB-S channel lists
  (analog channels and DVB-T are not supported yet)

2015-04-17
- added experimental support for Samsung J-Series DVB-C channel lists
  (analog channels, DVB-T and DVB-S are not supported yet)

2015-01-14
- added support for LG xx*.xml file names used by some hospitality TVs
- fixed handling of multiple satellites with LG's GlobalClone format
- setting the locked/skipped/hidden/deleted flags in LG GlobalClone files
- updated information text about required firmware update for LG webOS TVs
- show help text when a Samsung SCM file contains only bytes with value 0

2014-12-21
- added support for Panasonic SAT>IP channel lists
- enforce sequential program numbers for Pansonic lists 
  (TV does not work properly with gaps between the numbers)
- show warning for LG LB webOS TV channel lists, that due to firmware 
  issues any imported list (modified or not) may be unstable

2014-11-04
- fixed handling of favorites for Samsung F and H series
- fixed deleting of channels for older LG models (LD,LE,LH)

2014-11-02
- fixed reading Samsung channel lists containing empty satellite records
- disabled individual sorting of favorite lists for Samsung F and H series.
  It appears that only the E series firmware supports this feature.
- disabled deleting of channels from LG's GlobalClone channel lists because
  the TV does not support this. Instead they are appended at the end of the
  list.
- added support for Samsung "map-AirCableMixedA" and "map-AirCableMixedD"
  channel lists (used by some hospitality TVs)
- disabled editing of channel names for Panasonic lists to prevent side
  effects after saving (e.g. incorrect alphabetical sorting shown on TV)

2014-09-11
- fixed support for LG LV/LW DVB-C/T channel lists
- added support for Samsung map-CyfraPlusD channel list
- added Russian translation (thanks to some anonymous developer!)

2014-07-14
- fixed issue with broken channel lists for LG models LV, 
  LW (except LW4500, LW5400), LK950S and PM670S
- added support for LG's LY series binary TLL file format

2014-07-11
- improved accessibility support (channel list and prog/fav list selection)

2014-07-08
- added Accessibility menu and hotkeys to directly select the input source
  (Alt+1 to Alt+9) and the program/favorite lists (Ctrl+Alt+0, ...)
- addes support for LG LH250C

2014-06-10
- fixed problem with incorrect favorites when applying a reference list
- added function "Edit / Renumber Favorites by Pr#" to use the same numbers
  in all lists

2014-06-08.3
- fixed problem with Toshiba lists that refer to invalid satellites

2014-06-08.2
- added support for LG xxLB580V analog and DVB-C/T channel lists

2014-06-08
- fixed loading of Panasonic svl.db / svl.bin files

2014-05-30
- fixed issues with LG "GlobalClone" XML file format

2014-05-29
- supports LG LB55xx and LB56xx xxLB*.TLL channel lists 
- supports LG LB58xx*.TLL channel lists
  (satellite channels ONLY due to lack of DVB-C/T test files)
- supports LG *Clone*.TLL channel lists (e.g. LA8xxx, LB6xxx, LB7xxx models)
  (deleting channels is not yet implemented)

2014-05-06
- Added support for Canal+ Digital channel list (map-DigitalPlusD)
- Fixed: "Save reference file" ignored the entered filename for .csv lists
- Improved file format detection for Freesat, Tivusat, Canal Digital Sat

2014-05-05
- Added support for VDR *.conf channel list format 
  (Thanks to TCr82 for providing this code patch!) 

2014-01-27
- Added support for Samsung "Channel Digital Sat" channel lists

2014-01-21
- show channel "Provider" information for Samsung C,D and E series DVB-C/T 
  and Satellite channel lists (no data available for Astra HD+)

2013-12-22
- Analog channel names loaded from a reference file (.csv, .csm, .tll, ...)
  are now copied into and saved with the current TV data file .

2013-12-20
- fixed incorrect disabling of "move channel up" button/menu item

2013-12-19
- Support for Samsung Tivusat channel list

2013-12-15
- Support for new channel list file format introduced by LG's firmware
  04.20.29 for LA- and LN-series
- LG DVB-C/T channel numbers are now marked as "moved" so that the TV
  does not change them automatically

2013-11-24
- Load and repair corrupted Panasonic and Toshiba SQLite channel lists

2013-11-23
- Channel lists can now be printed
- Fix: Error when saving Toshiba and Panasonic channel list which contained
  red proxy entries for missing channels after applying a reference list
- Fix: Modified Panasonic channel names were not written to the file

2013-11-20
- Panasonic: Handling of special characters in channel names
- Toshiba: channels didn't change their order for zapping in .zip/chmgt.db 
  channel lists (thanks to Kurt W. for the hint)

2013-11-16
- FIX: changes to Samsung channel lists after the first "save file" 
  operation were lost. Only the first save operation worked as expected.
- FIX: channels in Samsung B-series DVB-C/T channel lists were incorrectly
  identified and marked as deleted/active, resulting in duplicate program
  numbers on the TV.
- FIX: channel names in Samsung lists sometimes showed "..." after the end
- FIX: dragging something (e.g. a file) from outside ChanSort over one of
  its tables causes an error

2013-11-12 (beta)
- Experimental support for modifying LG channel lists with predefined
  channel numbers (LCN)
- Backed-out some changes from 2013-11-09 which may have caused incorrect
  handling of deleted channels in Samsung channel lists

2013-11-09
- File / "File information" now shows information for all TV models
- Disabled "TV-Set" menu items which are not applicable
- Fixed DVB-S transponder/frequency information for LG LN and LA61xx series
- Fixed deleting channels in Samsung B-series Digital Air/Cable lists
- Fixed encryption information in Samsung B-series Digital Air/Cable lists
- Fixed loading of reference lists with non-unique channel identifiers
- Fixed error when saving LG files for models LD-LK after applying a
  reference list which contains channels not present in the TLL file

2013-10-23
- Support for Samsung's Monitor/TV 3-series (LTxxy3)

2013-10-22
- Support for Samsung's 2013 B-Series

2013-10-07
- Added support for Samsung "FreesatD" channel lists
- Added support for LG LP-series sat channel lists (DVB-C/T not supported)
- Added columns for "skip" and "hide" channel flags to left list
- skipped/hidden channels are now display with blue/light grey color
- LG hotel-mode/DTV-update settings editable for all supported LG models

2013-09-15
- Added support for LG PN-series satellite channel lists
- Fixed: use last selected character set when loading LG channel lists
- Fixed missing translations

2013-08-21
- Added support for LG PN-series (tested with PN6500)

2013-08-19
- Fixed loading Samsung B-series channel lists
- Changed file filter for LG to "xx*.TLL" to exclude the GlobalClone*.TLL
  files of LA and LN series.
- Fixed "New version available" info screen
- Improved error handling

2013-07-23
- Added support for LG LT-series (tested with xxLT380H)

2013-07-22
- Display message box to install VC++ 2010 Redist Package (x86) when it is
  missing (required by SQLite to load Panasonic and Toshiba lists)
- FIX: added missing files for Portuguese translation

2013-07-19.3
- FIX: .NET Framework 4.0 exceptions about loading DLLs downloaded from
  the web (since the whole .zip was downloaded from the web)

2013-07-19.2
- FIX: Samsung "CablePrime" channel list was not updated/saved
- FIX: Deleted Samsung analog, DVB-T and DVB-C channels re-appeared in the
  channel list after loading the file again.

2013-07-19
- Supports Panasonic "svl.bin" channel lists for different TV CPUs
  (auto-detecting big-endian or little-endian byte order).
- *.csv reference list is no longer created automatically. (You can always
  use your TV data file as a reference list anyway)
- File / Save reference list... now opens a dialog which allows to save in
  either ChanSort *.csv or SamToolBox *.chl format.
  (The *.chl format only contains the currently selected list, so it can be
  used to duplicate the order from e.g. the "Astra HD+" to the "Satellite"
  list within the same *.scm file)
- Upgraded to .NET Framework 4.0  and DevExpress 13.1 libraries

2013-07-03
- Support for individually sorted favorite lists, if supported by TV
  (e.g. Samsung E and F series, Panasonic Viera)
- FIX: "insert after" using drag and drop from right to left list 
  inserted before instead of after the drop position

2013-07-02
- FIX: wrong version number (caused a popup about a newer version online)

2013-07-01
- Added support for Panasonic channel list (svl.db and svl.bin format)
- Translated UI to Portuguese (Thanks to Vitor Martins Augusto)

2013-06-24
- FIX: Resizing a column caused an exception
- FIX: Deleting satellite channels from an SCM file did not work correctly
- Improved SCM file format detection
- Samsung E/F-series: channels in the favorite lists now use their prog#
  instead of all being put at #1 
  (in a future version the fav lists may be sorted separately)

2013-06-23
- Drag&Drop inside left list and from right to left list 
  (only available when the left list is sorted by "New Pr#")
- Simplified menu/tool bar
- FIX: Moving multiple channels down now works correctly
- FIX: +/- keys no longer open the cell-editor after moving a channel
- Editor for "New Pr#" no longer opens when pressing non-numeric keys
- Move up/down is now disabled when left list is not sorted by "New Pr#"

2013-06-22
- Showing separate DVB-C and DVB-T lists for LG TVs (LA series can have
  both lists while prior models only had one)
- FIX: Lists for LG's LD,LE,LX,PK (except 950), PT, LW4500, LW5400 models
  are now physically reordered
- Empty lists are no longer displayed

2013-05-29
- Added support for Samsung "CablePrime" channel lists
- FIX: error when loading a Samsung files which only contains an 
  AstraHDPlus channel list.
- Channel name editor now limits the input to the maximum number of 
  characters allowed by the file format (e.g. 5 chars for Samsung analog
  channel names)

2013-05-16
- FIX: on LG's LA and LN models the DVB-S symbol rate got corrupted
- disabled editing of LG channel lists whith preset program numbers
- last file is no longer loaded automatically when starting ChanSort

2013-05-11
- TV data files can be used as reference lists (without exporting .csv)
- added "File / Export Excel list" function
- added hotkeys to many menu items
- added most recently used files to menu
- added support to enable Hotel Mode for LH3000 and LN models
- fixed: saved incorrect DVB-S transponder symbol rate for LG's LK950S, LV,
  LW and LN models
- re-added "TV-Set / Clean channel data" function for LG TV's.


2013-05-07
- Added support for LG's LN-series
- Fixed: Saving reordered list for LG xxLH3000.
- Removed "Cleanup TV data file" function which bricked one LG TV.

2013-05-03
- Added Assistants for loading and saving files
- Added support for LG's 2013 LA-series DVB-S channel lists.
  Due to a lack of test files containing analog or DVB-C/T channels, these
  lists are not supported yet. If you have a TLL file with such channels 
  please send it to horst@beham.biz.
- Added support for LG's LH3000 Model
- Improved clean-up of LG channel lists. This should solve problems with
  program numbers changing randomly or inability to change them at all.
- Fixed: Program number and channel name can be edited again by directly 
  typing the number or name on the keyboard.
- Fixed: Sorting and column layout is now preserved when switching lists
- Fixed: Missing channels from a reference list appeared as valid channels
  in the UI after saving a TLL file.

2013-04-21
- Fix: Encryption flag for Samsung analog and DVB-C/T lists now shown 
  correctly
- Added "Remove channels" function to right list. E.g. you can use this to
  search and select encrypted channels in the right list and remove them 
  (from the sorted list).
- Text editor for channel number or name now only opens after holding the
  left mouse button down for at least 0.5sec. This prevents it from opening 
  when you double-click a channel.
- Added "Edit channel name" function to menus (due to the editor no longer
  opening automatically after a short click on the name)
- Warnings and information about TV file content are no longer shown when
  opening the file. It can be viewed by using the 
  "File / Show file information" menu item.
- Added experimental loader for Panasonic TV files. 
  Saving is not supported yet!

2013-04-11
- added support for Toshiba *.zip channel lists (containing chmgt.db list)
- allow Pr #0 for analog channels
- FIX: first channel list only got populated after switching between tabs

2013-04-08
- Added support for Samsung F-series.
- Added online check for updated program version

2013-04-07
- FIX: saving a .TLL file after loading a reference list which contained 
  channels that are no longer inside the .TLL caused an error during saving

2013-04-06
- FIX: When double-clicking a channel in the right list, which was already 
  part of the sorted list, the wrong channel was selected in the left list.
- new application icon which is licensed free-to-use

2013-04-05
- Redesigned user interface
- Editing option to close or keep gaps when moving/deleting channels
- Support for LG LMxxxT models, which export an invalid DVB-S data block
- Opening a file automatically shows the first non-empty channel list
- Reloading a TV-file will show the last opened list
- FIX: Deleting rows caused incorrect selections in the left list. 
  Successive deletes resulted in the loss of the first channel.
- FIX: Duplicate Pr# was assigned to channels when they were added out of
  order

2013-04-04
- Deleting channels for Samsung TVs now stores the files correctly 
  (no longer showing them all on Pr #0 on your TV)

2013-04-03 (major release)
- complete re-write of the code for loading/saving TV-data files (SCM, TLL)
  and reference lists (CSV).
- added support for LG's LX-models
- channel names can now be edited for both LG and Samsung 
- menu item for (un-)setting favorite #5 for Samsung series E
- Samsung channel lists are now loaded/saved correctly 
  (program numbers, favorites, locking, frequencies, ...)
- loading a reference list for a Samsung .SCM file which contains both
  air and cable channels or satellite and AstraHD+ channels caused the 
  items to be mixed up and all shown in the first list (showing not-found
  channels in red)

2013-03-30
- FIX: bei nicht-LM Geräten wurden die DVB-S Programmnummern falsch ausgelesen
- Hotelmodus/DTV-channel-update kann nun bei allen LG serien geändert werden

2013-03-29
- LG: Doppelt vorhandene Sender (die automatisch vom TV bei der Sendersuche
  mit Ländervoreinstellung angelegt werden) werden nun automatisch gelöscht.
  Diese führen am TV häufig dazu, dass Sender zufällig zwischen deren
  Programmplätzen hin- und herspringen. (Bug im TV, unabhängig von ChanSort)
- LG: Sender können nun gelöscht werden ("Nicht explizit sortierte Sender: Löschen")
- LG: Ändern von Sendernamen möglich (um bei DVB-C/T/S die Namen nicht wieder
  zu verlieren, muss Hotel-Mode=ein und DTV channel update="manual" sein)
- LG: Für die LM-Serie kann nun innerhalb von ChanSort der Hotel-Modus bzw
  die Senderlisten-Aktualisierung ein-/ausgeschaltet werden.

2013-02-26
- Neuer Bearbeitungsmodus "Tauschen" eingebaut, mit dem beim Ändern der
  Programmnummer die beiden Sender ihre Nummer tauschen und alle anderen
  Nummern unverändert bleiben.
- Beim Öffnen einer TV-Datei ohne zugehöriger Referenzliste werden nun
  automatisch alle Sender in die "Sortierte Liste" übernommen.
- "Speichern unter" funktioniert nun auch mit Samsung's SCM Dateien
- Wenn beim Speichern ein Fehler auftritt, wird nun auf mögliche Ursachen 
  hingewiesen

2013-02-11
- Mehrfach vorhandene Sender werden nun getrennt angezeigt, wenn sie 
  unterschiedlichen Programmnummern zugewiesen sind (z.B. DVB-T Empfang des 
  gleichen Senders über verschiedene Sendeanlagen).
- Beim Ändern der Programmnr auf eine bisher nicht verwendete Nummer werden
  die Programmnummern hinter der neuen Nummer nicht mehr erhöht.

2013-02-04
- Unterstützung für Samsung "AstraHDPlus" Senderliste hinzugefügt
- Funktion zum Übernehmen der bestehenden Senderliste in die sortierte Liste

2013-01-22
- Fix: Import von .chl Referenzlisten, wenn der gleiche Sendername über
  mehrere Satelliten empfangen wird und dort dem gleichen TSID zugewiesen ist.

2013-01-16
- Import von .chl Referenzlisten von SamToolBox
- Samsung Senderlisten zeigen nun "verschlüsselt"-Info an
- FIX: Mehrfachauswahl in den Senderlisten (mit Strg/Shift + Maus/Cursortasten)
- FIX: Warnung statt Absturz bei mehrfach vergebenen Transponder-IDs (.SCM Datei)

2013-01-15
- Samsung Serie E wird nun unterstützt
- Favoritenliste der Samsung Serie D und E  wird nun erkannt
- Samsung Modellerkennung verbessert

2012-12-26
- Unterstützung für Samsung Fernseher hinzugefügt
- Funktion zum Öffnen einer Referenzliste hinzugefügt
- Funktion zum kompletten Löschen der Senderlisten für LG Fernseher
- Diverse Bugfixes

2012-11-07
- Anzeige von DVB Netzwerk Name und Betreiber
- Anzeige der DVB-S Transpondernummer
- Fix: Direkte Eingabe der Prog# in rechter Liste, wenn sie vorher 0 war

2012-11-06
- Fix: Analoge Senderliste wurde nicht korrekt gespeichert
- Fix: Hinzufügen von Sendern zu einer leeren Liste
- Setzen von Favoriten möglich
- Setzen von Sender sperren / überspringen / verstecken
- Direkte Eingabe von neuer Programmnr in den Tabellen
- Kontextmenü über rechte Maustaste

2012-11-05
- Fix: DVB-C/T Senderliste wurde nicht korrekt gespeichert
- Fix: Favoriten in DVB-C/T werden nun korrekt angezeigt

2012-11-04
- Fix: Transpondernummer und Satelliteninfo wurde falsch ausgelesen
- Mehrfach vorhandene Sender werden nun nicht mehr ausgefiltert

2012-11-01
- UI und internes Datenmodell komplett überarbeitet
- Getrennte Listen für Analog, DVB-C/T, DVB-S in jeweils Radio/TV

2012-10-30
- Unterstützung für LG Serie CS, LD, LE, LH und PM hinzugefügt
- Neue Serien können durch Anpassen einer .INI Datei hinzugefügt werden
- Automatischer Test im Sourcecode, der alle bekannten TLLs lädt
- Hilfe-Menü hinzugefügt

2012-10-29
- Plugin zum direkten Laden/Speichern von TLLs, ohne TLLsort.exe
- Unterstützung für LG Serie LV und LW hinzugefügt
- Sourcen veröffentlicht

2012-10-26
- Programm umbenannt von TLLsortGUI auf ChanSort
- Übersetzt in Englisch und Österreichisch
- Einstellungen werden beim Beenden/Speichern des Programms übernommen
- Ausgewählten Zeilen in der sortierten Liste alphabetisch sortieren
- Dateinamen für Speichern können geändert werden
- Unterstützung für Plugin-DLLs für verschiedene TV-Modelle

2012-10-25
- Neu: Wenn eine Gruppenzeile (Sat, Analog, CableAndTerrestic) ausgewählt
  ist, werden alle darin enthaltene Sender eingefügt bzw. entfernt.
- Bugfix: Eingabefelder für Einfügepositionen erlauben nun keine 
  ungültigen (sprich zu hohe, nicht fortlaufende) Nummern mehr.

2012-10-24 (14:52 MESZ)
- Bugfix: Fehler beim Einlesen der TLL-Datei, wenn der gleiche Sender 
  (sprich Uid) mehrfach enthalten war. Kann vermutlich passieren, wenn 
  ein Sendersuchlauf 2x durchgeführt wurde, ohne die alten Programme zu
  löschen.

2012-10-24 (#1)
- erste Veröffentlichung