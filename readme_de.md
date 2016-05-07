Links
-----
[![EN](http://beham.biz/chansort/flag_en.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme.md)
[![DE](http://beham.biz/chansort/flag_de.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme_de.md) |
[Download](https://github.com/PredatH0r/ChanSort/releases) | 
[Dokumentation](https://github.com/PredatH0r/ChanSort/wiki) |
[Forum](https://github.com/PredatH0r/ChanSort/issues) | 
[E-Mail](mailto:horst@beham.biz)

Über ChanSort
--------------
ChanSort ist eine PC/Windows-Anwendung, die das Sortieren von Fernsehsenderlisten erlaubt.  
Die meisten modernen Fernseher können Senderlisten auf einen USB-Stick übertragen, den man danach am PC anschließt.
ChanSort unterstützt diverse Modelle von **Hisense** (NEU), LG, Panasonic, Samsung, Toshiba und das Linux VDR Projekt.

![screenshot](http://beham.biz/chansort/ChanSort-de.png)

Funktionen
--------
- Umreihen von Sendern (direkte Nummerneingabe, auf/ab verschieben, drag&drop, Doppelklick)
- Übernahme der Reihenfolge aus einer Vorlagedatei
- Mehrfachauswahl um mehrere Sender gleichzeitig zu bearbeiten
- Nebeneinander-Ansicht von umsortierter und ursprünglicher Liste (ähnlich wie Playlist und Medienbibliothek)
- Umbenennen und Löschen von Sendern
- Verwalten von Favoriten, Kindersperre, Überspringen und Verstecken von Sendern
- Benutzeroberfläche in Deutsch, Englisch und teilweise in Türkisch und Portugiesisch
- Unicode-Zeichensatzunterstützung für Sendernamen (latein, kyrillisch, griechisch, ...)

Manche Funktionen sind nicht bei allen TV-Modellen und Empfangsarten verfügbar (analog, digital, Sat, Kabel, ...)

! VERWENDUNG AUF EIGENE GEFAHR !
------------------------
Diese Software wurde ohne Unterstützung durch TV-Hersteller und ohne Zugang zu offiziellen
Unterlagen über die Dateiformate erstellt. Es beruht ausschließlich auf der Analyse von Dateien, Versuchen and Fehlerkorrekturen.
Es besteht die Möglichkeit von unerwarteten Nebeneffekten oder Schaden am Gerät (wie in 2 Fällen berichtet).

Hisense ist der einzige Hersteller, der Informationen und ein Testgerät bereitstellten.

Systemvoraussetzungen
-------------------
- [Microsoft .NET Framework 4.0 (Full)](http://www.microsoft.com/en-us/download/details.aspx?id=17851): 
  In Win8 enthalten, Nachinstallation bei Win7, Vista and XP SP3 erforderlich
- [Microsoft Visual C++ 2010 Redistributable Package (x86)](http://www.microsoft.com/en-us/download/details.aspx?id=8328):
  Wird benötigt um SQLite-Senderlisten zu bearbeiten (Hisense, Panasonic, Toshiba und Samsung J-Serie)
- USB Stick/SD-Karte zur Übertragung der Senderliste zwischen TV und PC (FAT32-Formatierung empfohlen)
- Einige TV-Modelle von LG erfordern eine spezielle Service-Fernbedienung zum Aufruf der Export/Import-Funktionen (Details in der Wiki)

Unterstützte TV-Modelle 
---------------------

**Hisense**  
Smart-Modelle (2016) mit channel.db Dateiformat, z.B. H65M5500  
Besonderen Dank an Hisense für die Bereitstellung von technischen Informationen und einem Testgerät!

**Samsung**  
Serie: B (2009)*, B (2013), C, D, E, F, H, J  
Listen:  Air analog, Air digital, Cable analog, Cable digital, 
		Cable Prime, Sat digital, Astra HD+, Freesat, TivuSat,
		Canal Digital Sat, Digital+, Cyfra+

\*: Das "clone.bin"-Format ist nicht unterstützt. Im "*.scm"-Format
der 2009 B-series werden in der "Air Analog"-Liste nicht alle Bearbeitungsfunktionen
unterstützt, da keine entsprechenden Testdateien vorhanden ist.

Eine Anleitung zum Übertragen der Liste auf/von USB befindet sich auf:
http://www.ullrich.es/job/sendersortierung/senderlisten-samsung-tv-exportieren-importieren/

**LG**  
Serien: CS, DM, LA, LB\*, LD, LE, LF, LH, LK, LM+, LN, LP#, LS, LT, LV, LW, LX, PM, PN, PT, UB\*  
Listen:  Analog TV, DTV (DVB-C, DVB-T), Radio (DVB-C/T), Sat-DTV (DVB-S2), Sat-Radio (DVB-S2)

\*: Einige Geräte verhalten sich fehlerhaft aufgrund Probleme in deren Firmware.  
+: Siehe Systemanforderungen für die LM-Serie. xxLM640T kann aufgrund von Firmwaremängeln nicht unterstützt werden.  
\#: Nur Satellitensender werden unterstützt.

Andere Modelle können ebenfalls funktionieren, wurden aber nicht getestet. Erfahrungsberichte im Forum sind jederzeit willkommen.

Eine Anleitung zum Aufruf des geheimen Service-Menüs zur Senderlistenübertragung befindet sich in der Wiki.

**Panasonic**  
Viera-Modelle mit svl.bin oder svl.db Dateien (die meisten Modelle seit 2011)

**Toshiba**  
Modelle, die eine .zip-Datei mit folgendem Inhalt: chmgt.db, dvbSysData.db und dvbMainData.db.  
(z.B. RL, SL, TL, UL, VL, WL, XL, YL models of series 8xx/9xx)

**VDR (Linux Video Disk Recorder)**  
Unterstützung des channels.conf Dateiformats.  
Die Implementation hierfür wurde vom Mitglied "TCr82" des VDR Projekts beigesteuert.

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
