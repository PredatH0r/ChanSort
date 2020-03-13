Links
-----
[![EN](http://beham.biz/chansort/flag_en.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme.md)
[![DE](http://beham.biz/chansort/flag_de.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme_de.md) |
[Download](https://github.com/PredatH0r/ChanSort/releases) | 
[Dokumentation](https://github.com/PredatH0r/ChanSort/wiki/Home-(de)) |
[Forum](https://github.com/PredatH0r/ChanSort/issues) | 
[E-Mail](mailto:horst@beham.biz)

�ber ChanSort
--------------
ChanSort ist eine Windows-Anwendung, die das Sortieren von Fernsehsenderlisten erlaubt.  
Die meisten modernen Fernseher k�nnen Senderlisten auf einen USB-Stick �bertragen, den man danach am PC anschlie�t.
ChanSort unterst�tzt diverse Dateiformate von Samsung, LG, Panasonic, Sony, Philips, Hisense, Toshiba,
Medion/Nabo/ok./PEAQ/Schaub-Lorenz/Silva-Schneider/Telefunken, Linux VDR und SAT>IP .m3u.

![screenshot](http://beham.biz/chansort/ChanSort-de.png)

Funktionen
--------
- Umreihen von Sendern (direkte Nummerneingabe, auf/ab verschieben, drag&drop, Doppelklick)
- �bernahme der Reihenfolge aus einer Vorlagedatei
- Mehrfachauswahl um mehrere Sender gleichzeitig zu bearbeiten
- Nebeneinander-Ansicht von umsortierter und urspr�nglicher Liste (�hnlich wie Playlist und Medienbibliothek)
- Umbenennen und L�schen von Sendern
- Verwalten von Favoriten, Kindersperre, �berspringen und Verstecken von Sendern
- Benutzeroberfl�che in Deutsch, Englisch, Spanisch und teilweise in T�rkisch, Portugiesisch und Russisch
- Unicode-Zeichensatzunterst�tzung f�r Sendernamen (latein, kyrillisch, griechisch, ...)

Manche Funktionen sind nicht bei allen TV-Modellen und Empfangsarten verf�gbar (analog, digital, Sat, Kabel, ...)

! VERWENDUNG AUF EIGENE GEFAHR !
------------------------
Diese Software wurde gro�teils ohne Unterst�tzung durch TV-Hersteller und ohne Zugang zu offiziellen
Unterlagen �ber die Dateiformate erstellt. Es beruht ausschlie�lich auf der Analyse von Dateien, Versuchen and Fehlerkorrekturen.
Es besteht die M�glichkeit von unerwarteten Nebeneffekten oder Schaden am Ger�t (wie in 2 F�llen berichtet).

Hisense ist der einzige Hersteller, der Informationen und ein Testger�t bereitstellten.

Systemvoraussetzungen
-------------------
- [Microsoft .NET Framework 4.6 (oder neuer)](https://dotnet.microsoft.com/download/dotnet-framework)
- [Microsoft Visual C++ 2010 Redistributable Package (x86)](http://www.microsoft.com/en-us/download/details.aspx?id=8328):
  Wird ben�tigt um SQLite-Senderlisten zu bearbeiten (Hisense, Panasonic, Toshiba und Samsung J-Serie)
- USB Stick/SD-Karte zur �bertragung der Senderliste zwischen TV und PC (FAT32-Formatierung empfohlen)
- Einige TV-Modelle von LG erfordern eine spezielle Service-Fernbedienung zum Aufruf der Export/Import-Funktionen (Details in der Wiki)

Unterst�tzte TV-Modelle 
---------------------

**Samsung**  
.scm Dateien: Serien B (2009)*, B (2013), C, D, E, F, H, J  
.zip Dateien: Serien H, J, K, M, N, Q, R  
Listen:  Air analog, Air digital, Cable analog, Cable digital, 
		Cable Prime, Sat digital, Astra HD+, Freesat, TivuSat,
		Canal Digital Sat, Digital+, Cyfra+

\*: Das "clone.bin"-Format ist nicht unterst�tzt. Im "*.scm"-Format
der 2009 B-series werden in der "Air Analog"-Liste nicht alle Bearbeitungsfunktionen
unterst�tzt, da keine entsprechenden Testdateien vorhanden ist.

Eine Anleitung zum �bertragen der Liste auf/von USB befindet sich auf:
http://www.ullrich.es/job/sendersortierung/senderlisten-samsung-tv-exportieren-importieren/

**LG**  
Serien: CS, DM, LA, LB\*, LD, LE, LF, LH, LK, LM+, LN, LP#, LS, LT, LV, LW, LX, PM, PN, PT, UB\*  
Listen:  Analog TV, DTV (DVB-C, DVB-T), Radio (DVB-C/T), Sat-DTV (DVB-S2), Sat-Radio (DVB-S2)

\*: Einige Ger�te verhalten sich fehlerhaft aufgrund Probleme in deren Firmware.  
+: Siehe Systemanforderungen f�r die LM-Serie. xxLM640T kann aufgrund von Firmwarem�ngeln nicht unterst�tzt werden.  
\#: Nur Satellitensender werden unterst�tzt.

Andere Modelle k�nnen ebenfalls funktionieren, wurden aber nicht getestet. Erfahrungsberichte im Forum sind jederzeit willkommen.

Eine Anleitung zum Aufruf des geheimen Service-Men�s zur Senderlisten�bertragung befindet sich in der Wiki.

**Panasonic**  
Viera-Modelle mit svl.bin oder svl.db Dateien (die meisten Modelle seit 2011)

**Sony**  
Android-TV "sdb.xml" Dateien mit Versionen "FormateVer" 1.1.0 und KDL 2012/2014 mit "FormatVer" 1.0.0, 1.1.0 and 1.2.0

**Philips**  
Philips verwendet unz�hlige unterschiedliche Dateiformate f�r diverse TV-Modelle.
ChanSort unterst�tzt derzeit 2 Varianten von .xml-Dateien. Andere Formate werden nicht unterst�tzt.

**Hisense**  
Smart-Modelle (2016) mit channel.db Dateiformat, z.B. H65M5500  
2017 Modelle mit einem servicelist.db Dateiformat  
Besonderen Dank an Hisense f�r die Bereitstellung von technischen Informationen und einem Testger�t!

**Toshiba**  
Modelle, die eine .zip-Datei mit folgendem Inhalt: chmgt.db, dvbSysData.db und dvbMainData.db.  
(z.B. RL, SL, TL, UL, VL, WL, XL, YL models of series 8xx/9xx)

**ITT, Medion, Nabo, ok., PEAQ, Schaub-Lorenz, Silva-Schneider, Telefunken**  
Die Marken nutzen .sdx Dateien (derzeit wird nur Satellitenempfang unterst�tzt)

**VDR (Linux Video Disk Recorder)**  
Unterst�tzung des channels.conf Dateiformats.  
Die Implementation hierf�r wurde vom Mitglied "TCr82" des VDR Projekts beigesteuert.

**m3u (SAT>IP)**  
Unterst�tzt SAT>IP .m3u Dateien mit erweiterten Informationen zu Sendernamen und Programmnummern.

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
