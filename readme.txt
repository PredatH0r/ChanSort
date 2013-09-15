Version v2013-09-15 ======================================================

Changes:
- Added support for LG PN-series satellite channel lists
- Fixed: use last selected character set when loading LG channel lists
- Fixed missing translations

The complete change log can be found at the end of this document


About ChanSort ============================================================

ChanSort is a program to manage your Samsung, LG, Toshiba or Panasonic TV's
channel list on your PC.

It allows you to change program numbers and channel names, select your 
favorites, set a parental lock and much more. With its multi-selection 
capabilities and the side-by-side view of your sorted list and the available
channels a list can be created in no-time.

You can apply reference lists to your TV data file to restore a previous 
order, e.g. after running a channel scan. You can even apply the same 
reference list to TVs from different manufacturers.

You can get get the latest version and support on  
http://sourceforge.net/p/chansort/discussion/ or by contacting me by email:
mailto:horst@beham.biz


Supported TV models =======================================================

Samsung 
-------
    Series: B*, C, D, E, F
    Lists:  Air analog, Air digital, Cable analog, Cable digital, 
            Cable prime, Sat digital, Astra HD+, 

    * NOTE: the "Air Analog"-list of the B-Series doesn't support all 
    editing features due to a lack of test files. If you have such a file,
    please send it to me.

    Instructions for transferring the channel list can be found on:
    http://www.ullrich.es/job/sendersortierung/senderlisten-samsung-tv-exportieren-importieren/

LG
------
    Series: CS, DM, LA, LD, LE, LH, LK, LM*, LN, LS, LT, LV, LW, LX, PM, PT
    Lists:  Analog TV, DTV (DVB-C, DVB-T), Radio (DVB-C/T), Sat-DTV (DVB-S2),
            Sat-Radio (DVB-S2)
  
    * NOTE: See system requirements for LM-Series.
            Model xxLM640T is not supported due to its broken firmware.
    Other models might also work, but have not been tested. If you have a
    .TLL file of a series not listed here, please send it to me.
    
    Instructions on how to access the hidden service-menu for transferring
    the channel list from/to USB can be found here:
    http://sourceforge.net/p/chansort/wiki/Home/

Panasonic
-------
    Viera models with an svl.bin or svl.db channel list
    (most/all? models since 2011)

Toshiba
-------
    Models that export a .zip file containing chmgt.db, dvbSysData.db and
    dvbMainData.db files.
    (e.g. RL, SL, TL, UL, VL, WL, XL, YL models of series 8xx/9xx)


! USE AT YOUR OWN RISK !
------------------------
This software was written without access to official documentation about 
the file formats involved. Without full knowledge about the specifics of a
format there is a chance of unwanted side-effects or damage to your TV.


System requirements =======================================================

- Microsoft .NET Framework 4.0 (included in Win8, can be installed manually
  on Windows 7, Windows Vista, Windows XP SP3):
  http://www.microsoft.com/en-us/download/details.aspx?id=17851
- Microsoft Visual C++ 2010 Redistributable Package (x86)
  Required to edit Panasonic and Toshiba channel lists (through SQLite)
  http://www.microsoft.com/en-us/download/details.aspx?id=8328
- USB-stick to transfer the channel list between your TV and PC
  (for Panasonic an SD-Card is needed instead of a USB-stick)
- For LG's LM-series you need a programmable remote control to access the 
  service menu for transferring the list to/from USB. 
  (e.g. Logitech Harmony 300, One-For-All URC3920,...)
  Details can be found on the ChanSort wiki and on 
  http://openlgtv.org.ru/wiki/index.php/Access_hidden_service_menus_/_modes


License ===============================================================

GNU General Public Licence, Version 3: http://www.gnu.org/licenses/gpl.html
Source code available on http://sourceforge.net/projects/chansort/

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.


Change log ================================================================

2013-09-15
- Added support for LG PN-series satellite channel lists
- Fixed: use last selected character set when loading LG channel lists
- Fixed missing translations

2013-08-21
- Added support for LG PN Series (tested with PN6500)

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
- Samsung E/F-Series: channels in the favorite lists now use their prog#
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
- Added support for Samsung F-Series.
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