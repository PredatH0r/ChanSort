Links
-----
[![EN](http://beham.biz/chansort/flag_en.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme.md)
[![DE](http://beham.biz/chansort/flag_de.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme_de.md) |
[Download](https://github.com/PredatH0r/ChanSort/releases) | 
[Dokumentation](https://github.com/PredatH0r/ChanSort/wiki/Home-(de)) |
[Forum](https://github.com/PredatH0r/ChanSort/issues) | 
[E-Mail](mailto:horst@beham.biz)

Über ChanSort
--------------
ChanSort ist eine Windows-Anwendung, die das Sortieren von Fernsehsenderlisten erlaubt.  
Die meisten modernen Fernseher können Senderlisten auf einen USB-Stick übertragen, den man danach am PC anschließt.  
ChanSort unterstützt diverse Dateiformate von Samsung, LG, Panasonic, Sony, Philips, Hisense, Toshiba, Grundig,
Sharp, Dyon, Blaupunkt, SatcoDX (verwendet von Medion, Nabo, ok., PEAQ, Schaub-Lorenz, Silva-Schneider, Telefunken),
Linux VDR, SAT>IP .m3u und Enigma2 basierender Linux TV-Boxen.

![screenshot](http://beham.biz/chansort/ChanSort-de.png)

Funktionen
--------
- Umreihen von Sendern (direkte Nummerneingabe, auf/ab verschieben, drag&drop, Doppelklick)
- Übernahme der Reihenfolge aus einer Vorlagedatei
- Mehrfachauswahl um mehrere Sender gleichzeitig zu bearbeiten
- Nebeneinander-Ansicht von umsortierter und ursprünglicher Liste (ähnlich wie Playlist und Medienbibliothek)
- Umbenennen und Löschen von Sendern
- Verwalten von Favoriten, Kindersperre, Überspringen und Verstecken von Sendern
- Benutzeroberfläche in Deutsch, Englisch, Spanisch und teilweise in Türkisch, Portugiesisch und Russisch
- Unicode-Zeichensatzunterstützung für Sendernamen (latein, kyrillisch, griechisch, ...)

Manche Funktionen sind nicht bei allen TV-Modellen und Empfangsarten verfügbar (analog, digital, Sat, Kabel, ...)

! VERWENDUNG AUF EIGENE GEFAHR !
------------------------
Diese Software wurde großteils ohne Unterstützung durch TV-Hersteller und ohne Zugang zu offiziellen
Unterlagen über die Dateiformate erstellt. Es beruht ausschließlich auf der Analyse von Dateien, Versuchen and Fehlerkorrekturen.
Es besteht die Möglichkeit von unerwarteten Nebeneffekten oder Schaden am Gerät (wie in 2 Fällen berichtet).

Hisense ist der einzige Hersteller, der Informationen und ein Testgerät bereitstellten.


Systemvoraussetzungen
-------------------
- [Microsoft .NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework)  
  (Unter Linux wird Winetricks mit einem 32bit wineprefix benötigt, wo das "dotnet48" Paket installiert ist)
- [Microsoft Visual C++ 2010 Redistributable Package (x86)](http://www.microsoft.com/en-us/download/details.aspx?id=8328)  
  Wird benötigt um SQLite-Senderlisten zu bearbeiten (Hisense, Panasonic, Toshiba und Samsung J-Serie)
- USB Stick/SD-Karte zur Übertragung der Senderliste zwischen TV und PC (FAT32-Formatierung empfohlen)


Unterstützte TV-Modelle 
---------------------

**Samsung**  
- .scm Dateien: Serien B (2009)*, B (2013), C, D, E, F, H, J  
- .zip Dateien: Serien H, J, K, M, N, Q, R  

\*: Das "clone.bin"-Format ist nicht unterstützt. Im "*.scm"-Format
der 2009 B-series werden in der "Air Analog"-Liste nicht alle Bearbeitungsfunktionen
unterstützt.

Eine Anleitung zum Transfer der Senderliste befindet sich hier:
https://github.com/PredatH0r/ChanSort/wiki/Samsung

**LG**  
- Serien basierend auf Netcast OS, die eine xx\*.TLL-Datei exportieren:  
  CS, DM, LA, LB\*, LD, LE, LF, LH, LK, LM+, LN, LP#, LS, LT, LV, LW, LX, PM, PN, PT, UB\*  
- Serien basierend auf webOS 2-5, die eine GlobalClone00001.TLL-Datei exportieren

\*: Einige Geräte verhalten sich fehlerhaft aufgrund Probleme in deren Firmware.  
+: Siehe Systemanforderungen für die LM-Serie. xxLM640T kann aufgrund von Firmwaremängeln nicht unterstützt werden.  
\#: Nur Satellitensender werden unterstützt.

Modelle mit NetCast Betriebssytem beinhalten keine Import/Export Funktion im normalen Menü. Um das Geheimmenü aufzurufen,
halten Sie die Settings Taste auf der Fernbedienung solange gedrückt, bis das Menü wieder verschwindet und dann drücken Sie "1105".
Im "TV Link Loader" Menü befinden sich dann die Import/Export-Funktionen.

WICHTIG: Es ist NOTWENDIG bei der Sendersuche spezielle Optionen auszuwählen. Wenn ein Anbieter / Land / Satellit bei der Suche
ausgewählt wird, erhält man eine vorsortierte Liste und der TV verhält sich nach einem Export+Import fehlerhaft.
Wählen Sie immer "Keiner / Anderer / Alle" aus bzw. "Blindsuche", und nie einen Kabelanbieter oder "Astra 19.2 Liste".

**Panasonic**  
Die meisten Viera-Modelle seit 2011 mit Senderlisten im Format
- svl.bin
- svl.db

**Sony**  
- Android-TV "sdb.xml" Dateien mit Version "FormateVer" 1.1.0
- KDL 2012/2014 "sdb.xml" mit "FormatVer" 1.0.0, 1.1.0 and 1.2.0

**Philips**  
Philips verwendet unzählige unterschiedliche Dateiformate für diverse TV-Modelle.
ChanSort unterstützt derzeit folgende Formate:
- PhilipsChannelMaps\ChannelMap_45, 100, 105 und 110
- Repair\ChannelList\channellib\\*Table and s2channellib\\*.dat
- Repair\CM_TPM1013E_LA_CK.xml (diese Datei ist oft verstekt und nur eine .bin Datei sichtbar)

**Hisense**  
- Einige ältere Modelle wie LTDN40D50TS verwenden die gleichen .csv Dateien wie Sharp. Siehe "Sharp" für eine Anleitung.
- Smart-Modelle (2016) mit channel.db Dateiformat, z.B. H65M5500  
- 2017 Modelle mit einem servicelist.db Dateiformat  
Besonderen Dank an Hisense für die Bereitstellung von technischen Informationen und einem Testgerät!

**Toshiba**  
- Modelle, die eine .zip-Datei mit folgendem Inhalt: chmgt.db, dvbSysData.db und dvbMainData.db.  
(z.B. RL, SL, TL, UL, VL, WL, XL, YL models of series 8xx/9xx)  
- Modelle mit einer settingsDB.db Datei

**Grundig**  
Modelle die Dateien mit Namen dvb\*_config.xml exportieren.

**SatcoDX (OEM für ITT, Medion, Nabo, ok., PEAQ, Schaub-Lorenz, Silva-Schneider, Telefunken)**  
Mehrere Marken nutzen die gleiche Hardware für DVB-S und exportieren .sdx Dateien

**Sharp (and some models from Dyon, Blaupunkt, Hisense, Changhong, alphatronics, JTC Genesis)**
Einige dieser TVs verwenden ähnliche Hardware und können .csv Dateien exportieren/importieren, um die Reihenfolge zu ändern.  
Blaupunkt B40A148TCSFHD, B32B133T2CSHD, ...  
Changhong LED32E2200ST2, ...  
Dyon Live 22 Pro, Live 24 Pro, ENTER 32 Pro X, ...  
Hisense LTDN40D50TS, ...  
Sharp LC-xxCFE4142E, LC-xxCFF6002E, LC-xxFI5542E, LC-xxFG5242E, LC-xxUI7552E, LC-xxUI7652E, xxBJ1E, xxBJ3E, xxBJ5E, ...  

Abhängig vom konkreten TV-Modell kann die Import/Export-Funktion Teil des normalen Menüs, eines Hotel-Menüs oder Service-Menüs sein
und die exportieren Dateien unterschiedlich sein. Einige Modelle wie Sharp Aquos xxBJ1E haben ein Untermenü für den Export. Hier
sind alle 4 Typen von Dateien für einen Import nötig.  
Um in das geheime Hotel- / Service-Menü zu gelangen, gibt es verschiedene Tastenkombinationen auf der Fernbedienung:  
Hotelmenu: MENU 7906 / MENU 4588  
Servicemenu: MENU 1147 / MENU 11471147 / SOURCE 2580  
Ändern Sie keine Werte im Servicemenü, dies könnte den Fernseher beschädigen. Nutzen Sie nur die Import/Export-Funktionen.

**VDR (Linux Video Disk Recorder)**  
Unterstützung des channels.conf Dateiformats.  
Die Implementation hierfür wurde vom Mitglied "TCr82" des VDR Projekts beigesteuert.

**m3u (SAT>IP)**  
Unterstützt SAT>IP .m3u Dateien mit erweiterten Informationen zu Sendernamen und Programmnummern.

**Enigma2 (Dreambox, VU+ und viele andere Linux basierende Empfänger)**  
Erfordert eine lokale Kopie der Dateien "lamedb", "bouquets.\*" and "userbouquet.\*" aus /etc/Enigma2/.  


Lizenz (GPLv3)
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
