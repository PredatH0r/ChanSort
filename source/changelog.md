ChanSort Change Log 
===================

2020-05-15
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