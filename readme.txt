Version v2013-04-11 ========================================================

This is a maintenance release based on version v2013-04-05, which brought a 
refurbished user interface and fixes for various usability issues.

New:
- Added support for Toshiba *.zip channel lists (containing chmgt.db list)
- Allow Pr #0 for analog channels

Fixed:
- first channel list only got populated after switching between tabs

The complete change log can be found at the end of this document


About ChanSort =============================================================

ChanSort is a program to manage your Samsung, LG or Toshiba TV's channel list
on your PC.

It allows you to change program numbers and channel names, select your favorites, 
set a parental lock and much more. With its multi-selection capabilities and the
side-by-side view of your sorted list and the available channels a list can be
created in no-time.

You can apply reference lists to your TV data file to restore a previous order, 
e.g. after running a channel scan. You can even apply the same reference list
to your LG and Samsung TV.

You can get get the latest version and support on  
https://sourceforge.net/projects/chansort/?source=navbar


Supported models ===========================================================

LG
------
    Series: CS, DM, LD, LE, LH, LK, LM*, LS, LV, LW, LX, PM, PT
    Lists: Analog TV, DTV (DVB-C/T), Radio (DVB-C/T), Sat-DTV (DVB-S2), Sat-Radio (DVB-S2)
  
    * NOTE: See system requirements for LM-Series.
    Other models might also work, but have not been tested. 
    If you have a .TLL file of a series not listed here, please send it to me.

    Instructions on how to access the hidden service-menu for transferring the 
    channel list from/to USB can be found here:
    https://sourceforge.net/p/chansort/wiki/Home/
    http://www.ullrich.es/job/service-menue/lg-tlledit-lg-sendersortierung/


Samsung 
-------
    Series: B*, C, D, E
    Lists: Air analog, Air digital, Cable analog, Cable digital, Sat digital, Astra HD+

    * NOTE: the "Air Analog"-List of the B-Series doesn't support all editing features
    due to a lack of test files. If you have such a file, please send it to me.

    Instructions for transferring the channel list can be found on:
    http://www.ullrich.es/job/sendersortierung/senderlisten-samsung-tv-exportieren-importieren/

Toshiba
-------
    Models that export a .zip file containing chmgt.db, dvbSysData.db and dvbMainData.db files.
    (e.g. RL, SL, TL, UL, VL, WL, XL, YL models of series 8xx/9xx)


! USE AT YOUR OWN RISK !
------------------------
This software was written without access to any official documentation from either
Samsung or LG about the file formats involved. Without full knowledge about there 
specifics there is a chance of unwanted side-effects or damage to your TV.


System requirements =========================================================

- USB-Stick to transfer the channel list between your TV and PC
- For LG's LM-series you need a programmable remote control to access the service menu
  for transferring the list to/from USB. (e.g. Logitech Harmony 300, One-For-All URC3920,...)
  Details can be found on the ChanSort wiki and on 
  http://openlgtv.org.ru/wiki/index.php/Access_hidden_service_menus_/_modes
- Microsoft .NET Framework 3.5 (included in WinXP SP3, Vista, Win7, Win8)


License ===============================================================

GNU General Public Licence, Version 3: http://www.gnu.org/licenses/gpl.html
Source code available on https://sourceforge.net/projects/chansort/

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.


Change log ====================================================================

2013-04-11
- added support for Toshiba *.zip channel lists (containing chmgt.db list)
- allow Pr #0 for analog channels
- FIX: first channel list only got populated after switching between tabs

2013-04-08
- Added support for Samsung F-Series.
- Added online check for updated program version

2013-04-07
- FIX: saving a .TLL file after loading a reference list which contained channels
  that are no longer inside the .TLL caused an error during saving.

2013-04-06
- FIX: When double-clicking a channel in the right list, which was already part of the
  sorted list, the wrong channel was selected in the left list.
- new application icon which is licensed free-to-use

2013-04-05
- Redesigned user interface
- Editing option to close or keep gaps when moving/deleting channels
- Support for LG LMxxxT models, which export an invalid DVB-S data block
- Opening a file automatically shows the first non-empty channel list
- Reloading a TV-file will show the last opened list
- FIX: Deleting rows caused incorrect selections in the left list. Successive
  deletes resulted in the loss of the first channel.
- FIX: Duplicate Pr# was assigned to channels when they were added out of order

2013-04-04
- Deleting channels for Samsung TVs now stores the files correctly (no longer
  showing them all on Pr #0 on your TV)

2013-04-03 (major release)
- complete re-write of the code for loading/saving TV-data files (SCM, TLL) 
  and reference lists (CSV).
- added support for LG's LX-models
- channel names can now be edited for both LG and Samsung 
- menu item for (un-)setting favorite #5 for Samsung series E
- Samsung channel lists are now loaded/saved correctly 
  (program numbers, favorites, locking, frequencies, ...)
- loading a reference list for a Samsung .SCM file which contains both
  air and cable channels or satellite and AstraHD+ channels caused the items
  to be mixed up and all shown in the first list (showing not-found channels in red)

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