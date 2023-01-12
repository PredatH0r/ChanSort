Links
-----
[![EN](https://chansort.com/img/flag_en_16.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme.md)
[![DE](https://chansort.com/img/flag_de_24.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme_de.md)
[![PL](https://chansort.com/img/flag_pl_16.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme_pl.md)
[![TR](https://chansort.com/img/flag_tr_16.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme_tr-TR.md) |
[Download](https://github.com/PredatH0r/ChanSort/releases) | 
[Dokumentation](https://github.com/PredatH0r/ChanSort/wiki/Home-(de)) |
[Forum](https://github.com/PredatH0r/ChanSort/issues) | 
[E-Mail](mailto:horst@beham.biz)

Über ChanSort
--------------
ChanSort ist eine PC Anwendung, die das Sortieren von Fernsehsenderlisten erlaubt.  
Die meisten modernen Fernseher können Senderlisten auf einen USB-Stick übertragen, den man danach am PC anschließt.  
ChanSort unterstützt [Dateiformate diverser Marken](#unterstützte-tv-modelle) und kann Programmnummern und Favoritenlisten
von einer Senderdatei auf eine andere übertragen, sogar zwischen verschiedenen Modellen und Marken.

![screenshot](http://beham.biz/chansort/ChanSort-de.png)

Funktionen
--------
- Umreihen von Sendern (direkte Nummerneingabe, auf/ab verschieben, drag&drop, Doppelklick)
- Übernahme der Reihenfolge aus einer Vorlagedatei
- Mehrfachauswahl um mehrere Sender gleichzeitig zu bearbeiten
- Einfache Listenansicht (mit eingereihten Sender zuerst und dahinter alle uneingereihten)
- Nebeneinander-Ansicht von umsortierter und ursprünglicher Liste (ähnlich wie Playlist und Medienbibliothek)
- Umbenennen und Löschen von Sendern
- Verwalten von Favoriten, Kindersperre, Überspringen und Verstecken von Sendern
- Benutzeroberfläche in Deutsch, Englisch, Spanisch, Türkisch, Portugiesisch, Russisch und Rumänisch
- Unicode-Zeichensatzunterstützung für Sendernamen (latein, kyrillisch, griechisch, ...)

NICHT unterstützt:
- Hinzufügen von neuen Transpondern oder Sendern
- Ändern von Tuner-Einstellungen von Sendern (ONID, TSID, SID, Frequenz, APID, VPID, ...)

Manche Funktionen sind nicht bei allen TV-Modellen und Empfangsarten verfügbar (analog, digital, Sat, Kabel, ...)

! VERWENDUNG AUF EIGENE GEFAHR !
------------------------
Diese Software wurde großteils ohne Unterstützung durch TV-Hersteller und ohne Zugang zu offiziellen
Unterlagen über die Dateiformate erstellt. Es beruht ausschließlich auf der Analyse von Dateien, Versuchen and Fehlerkorrekturen.
Es besteht die Möglichkeit von unerwarteten Nebeneffekten oder Schaden am Gerät (wie in 2 Fällen berichtet).

Hisense ist der einzige Hersteller, der Informationen und ein Testgerät bereitstellten.

Unterstützte TV-Modelle 
---------------------
ChanSort unterstützt eine große Anzahl an Dateiformaten, aber es ist unmöglich für jede Marke und jedes Modell zu
sagen, welches Format verwendet wird (was sich auch durch Firmware-Updates ändern kann).  
Diese unvollständige Liste führt einige Beispiele an, die unterstützt werden, aber selbst wenn ein Modell oder Marke
hier nicht angeführt ist, könnte es trotzdem funktiontionieren:
- [Samsung](source/fileformats_de.md#samsung)
- [LG](source/fileformats_de.md#lg)
- [Sony](source/fileformats_de.md#sony)
- [Hisense](source/fileformats_de.md#hisense)
- [Panasonic](source/fileformats_de.md#panasonic)
- [Philips](source/fileformats_de.md#philips)
- [Sharp, Dyon, Blaupunkt, Hisense, Changhong, Grundig, alphatronics, JTC Genesis, ...](source/fileformats_de.md#sharp)
- [Toshiba](source/fileformats_de.md#toshiba)
- [Grundig](source/fileformats_de.md#grundig)
- [SatcoDX: ITT, Medion, Nabo, ok., PEAQ, Schaub-Lorenz, Silva-Schneider, Telefunken, ...](source/fileformats_de.md#satcodx)
- [VDR](source/fileformats_de.md#vdr)
- [SAT>IP m3u](source/fileformats_de.md#m3u)
- [Enigma2](source/fileformats_de.md#enigma2)

Systemvoraussetzungen
-------------------
**Windows**:  
- Windows 7 SP1, Windows 8.1, Windows 10 v1606 or later, Windows 11 (mit x86, x64 oder ARM CPU)
- [Microsoft .NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework)  
- Das .NET FW 4.8 funktioniert NICHT unter Windows 7 ohne SP1, Windows 8 oder Windows 10 vor v1606

**Linux**:  
- wine (sudo apt-get install wine)
- winetricks (sudo apt-get install winetricks)
- Starte winetricks, wähle oder erstelle ein wineprefix (32 bit oder 64 bit), wähle
  "Installiere Windows DLL oder Komponente", installiere das "dotnet48" Paket and ignore dutzende Popup-Dialoge
- Rechtsklick auf ChanSort.exe, wähle "Öffnen mit", "Alle Anwendungen", "Eine wine Anwendung"

**Mac**
- macOS wird nicht direkt unterstützt, aber mit Parallels oder UTM kann eine VM mit Windows 10/11 am Mac genutzt werden
- Anleitung für Macs mit m1/ARM CPU: https://history-computer.com/how-to-run-windows-on-m1-macs/

**Hardware**:  
- USB Stick/SD-Karte zur Übertragung der Senderliste zwischen TV und PC (Ein Stick <= 32 GB mit FAT32-Formatierung
ist DRINGEND empfohlen. (Einige TVs schreiben Müll auf NTFS bzw. unterstützen exFAT gar nicht)

Quellcode selbst übersetzen
-----------------
Siehe [build.md](source/build.md)

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
