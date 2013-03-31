ChanSort v2012-12-26

Allgemeines==========================================================

!!! Die Verwendung dieses Programms erfolgt auf eigene Gefahr! Es ist
!!! möglich, dass durch die Verwendung das TV Gerät beschädigt wird

Die Vorliegende Version unterstützt folgende TV-Geräte:
LG-Electronics: Serien CS, LD, LE, LH, LM, LK, LV, LW, PM
Samsung: Serien B, C, D und E

Eine Diskussion zum Thema "ChannelEditor" für LG Fernseher mit näheren
Infos rund um ChanSort befindet sich hier:
http://www.lg-forum.com/lg-led-plasma-lcd-fernseher/5098-channeleditor-45.html
http://www.hifi-forum.de/viewthread-145-5061.html
http://forum.lg.de/viewtopic.php?f=16&t=5097&sid=3dbbc693bbbaef643a624b82206b4ac0&start=10

Systemvoraussetzungen ================================================

- Microsoft .NET Framework 3.5
- für LG TVs der Serien CS, LM und PM ist eine programmierbare Fernbedienung 
  nötig (z.B. Logitech Harmony 300, One-For-All URC3920, ...)


Benutzung ============================================================

LG Fernseher

1) Zuerst muss man am Fernseher das geheime "In-Start" Menü aktivieren
und dort mit dem "TV Link Loader" die Senderliste auf einen USB-Stick
speichern.

Bei der LM-Serie (2012) benötigt man eine programmierbare Fernbedienung, da
weder die Standard-FB noch der Fernseher eine Zugangsmöglichkeit bietet.
Einige Wege um in das Menü zu gelangen, werden auf dieser Seite beschrieben:
http://openlgtv.org.ru/wiki/index.php/Access_hidden_service_menus_/_modes

Gelingt es, den Infrarot-Befehl für "In-Start" an den Fernseher zu senden,
drückt man anschließend auf "Settings" (Original-FB) oder "Menu" (Harmony).
Dann stehen das Hotel-Menü und der TV-Link-Loader zur Auswahl.
  
2) Mit ChanSort die TLL Datei am USB-Stick öffnen. Dies muss immer
  Die Original-Datei des TV sein und nicht eine bereits sortierte.
  Falls eine gleichnamige CSV-Datei mit einer bestehenden Sortierung 
  existiert wird diese automatisch eingelesen.
3) Die gewünschte Senderliste zusammenstellen. Am einfachsten in der
  rechten Ansicht in Spalte "Sendername" den Namen eintippen, dann in
  der Liste doppelclicken.
  In der Mitte kann eingestellt werden, wo in der sortierten Liste der
  neue Sender landet. Dazu einen Programmplatz in der linken Liste 
  auswählen und in der Mitte eventuell "Dahinter".
4) Dateien speichern
  Die TLL-Datei erhält eine 9 als erste Ziffer, um die Originaldatei
  nicht zu überschreiben. 
5) Senderliste auf USB Stick speichern und über den TV Link Loader in
  den Fernseher laden.

Samsung Fernseher

  Im Menü "Sender" / "Kanalliste übertragen" / "Auf USB exportieren"

Kontakt ==============================================================

Benutzer "Pred" auf www.lg-forum.com
Email: horst@beham.biz


Versionen ============================================================

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

Source Code ==========================================================

The user interface (ChanSort.Ui project) is using the commercial "WinForms"
UI component library from Developer Express. You can get a trial version
from http://www.devexpress.com/Subscriptions/DXperience/WhatsNew2012v1/winforms.xml
The trial version will periodically show popup-windows reminding you to
obtain a license.

As long as you dont make any changes to the UI, you can exclude the 
ChanSort.Ui project from the build list and copy all files from the 
"Program" directory to "Source\Debug".

Alternatively you can just compile the other projects and drop the generated
DLL-File into the "Program" folder and start ChanEdit.exe there

Lizenz ===============================================================

This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <http://unlicense.org/>