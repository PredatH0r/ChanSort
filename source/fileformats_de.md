Einige TV Modelle und Sat-Empfängerboxen unterschiedlicher Marken verwenden die gleiche Tuner-Hardware (inkl. Firmware 
und Dateiformaten) von Drittherstellern, somit werden auch einige Marken und Modelle unterstützt, die
hier nicht explizit angeführt sind.  
Andererseits kommt es auch vor, dass ein Hersteller für einige Modelle ein Dateiformat verwendet, das von ChanSort
(noch) nicht unterstützt wird.

<a name="samsung"/>Samsung
---
- .scm Dateien: Serien B (2009)*, B (2013), C, D, E, F, H, J  
- .zip Dateien: Serien H, J, K, M, N, Q, R  

\*: Das "clone.bin"-Format ist nicht unterstützt. Im "*.scm"-Format
der 2009 B-series werden in der "Air Analog"-Liste nicht alle Bearbeitungsfunktionen
unterstützt.

Eine Anleitung zum Transfer der Senderliste befindet sich hier:
https://github.com/PredatH0r/ChanSort/wiki/Samsung

<a name="lg"/>LG
---
- Serien basierend auf Netcast OS, die eine xx\*.TLL-Datei exportieren:  
  CS, DM, LA, LB\*, LD, LE, LF, LH, LK, LM+, LN, LP#, LS, LT, LV, LW, LX, PM, PN, PT, UB\*  
- Serien basierend auf webOS 2-5, die eine GlobalClone00001.TLL-Datei exportieren

\*: Einige Geräte verhalten sich fehlerhaft aufgrund Probleme in deren Firmware.  
+: xxLM640T kann aufgrund von Firmwaremängeln nicht unterstützt werden.  
\#: Nur Satellitensender werden unterstützt.

Modelle mit NetCast Betriebssytem beinhalten keine Import/Export Funktion im normalen Menü. Um das Geheimmenü aufzurufen,
halten Sie die Settings Taste auf der Fernbedienung solange gedrückt, bis das Menü wieder verschwindet und dann drücken Sie "1105".
Im "TV Link Loader" Menü befinden sich dann die Import/Export-Funktionen.

WICHTIG: Es ist NOTWENDIG bei der Sendersuche spezielle Optionen auszuwählen. Wenn ein Anbieter / Land / Satellit bei der Suche
ausgewählt wird, erhält man eine vorsortierte Liste und der TV verhält sich nach einem Export+Import fehlerhaft.
Wählen Sie immer "Keiner / Anderer / Alle" aus bzw. "Blindsuche", und nie einen Kabelanbieter oder "Astra 19.2 Liste".

<a name="sony"/>Sony
---
- Android-TVs: "sdb.xml" Dateien mit Version "FormateVer" 1.1.0
- KDL 2012/2014: "sdb.xml" mit "FormatVer" 1.0.0, 1.1.0 and 1.2.0

<a name="hisense"/>Hisense
---
- Einige ältere Modelle wie LTDN40D50TS verwenden die gleichen .csv Dateien wie Sharp. Siehe "Sharp" für eine Anleitung.
- Smart-Modelle (2016) mit channel.db Dateiformat, z.B. H65M5500  
- 2017 Modelle mit einem servicelist.db Dateiformat

Besonderen Dank verdient Hisense für die Bereitstellung von technischen Informationen und einem Testgerät!

<a name="panasonic"/>Panasonic
---
Die meisten Viera-Modelle seit 2011 mit Senderlisten im Format
- svl.bin
- svl.db

<a name="philips"/>Philips
---
Philips verwendet unzählige unterschiedliche Dateiformate für diverse TV-Modelle.
ChanSort unterstützt derzeit folgende Formate:
- PhilipsChannelMaps\ChannelMap_45, 100, 105 und 110
- Repair\ChannelList\channellib\\\*Table and s2channellib\\\*.dat
- Repair\CM_TPM1013E_LA_CK.xml (diese Datei ist oft versteckt und nur eine .bin Datei sichtbar)

<a name="sharp"/>Sharp (and some models from Dyon, Blaupunkt, Hisense, Changhong, alphatronics, JTC Genesis)
---
Einige dieser TVs verwenden ähnliche Hardware und können .csv Dateien exportieren/importieren, um die Reihenfolge zu ändern:  
- DVBS_Program.csv (mit cvt_database.dat)
- DVBS_CHANNEL_TABLE.csv (mit dtv_cmdb\*.bin)
- MS\*_DVBS_CHANNEL_TABLE.csv (mit MS\*_HOTELMODE_TABLE.json)

Blaupunkt B40A148TCSFHD, B32B133T2CSHD, ...  
Changhong LED32E2200ST2, ...  
Dyon Live 22 Pro, Live 24 Pro, ENTER 32 Pro X, ...  
Hisense LTDN40D50TS, ...  
Sharp LC-xxCFE4142E, LC-xxCFF6002E, LC-xxFI5542E, LC-xxFG5242E, LC-xxUI7552E, LC-xxUI7652E, xxBJ1E, xxBJ3E, xxBJ5E, ...  
Grundig 24 GHB 5944  

Abhängig vom konkreten TV-Modell kann die Import/Export-Funktion Teil des normalen Menüs, eines Hotel-Menüs oder Service-Menüs sein
und die exportieren Dateien unterschiedlich sein. Einige Modelle wie Sharp Aquos xxBJ1E haben ein Untermenü für den Export. Hier
sind alle 4 Typen von Dateien für einen Import nötig.  
Um in das geheime Hotel- / Service-Menü zu gelangen, gibt es verschiedene Tastenkombinationen auf der Fernbedienung:  
Hotelmenu: MENU 7906 / MENU 4588  
Servicemenu: MENU 1147 / MENU 11471147 / SOURCE 2580  
Ändern Sie keine Werte im Servicemenü, dies könnte den Fernseher beschädigen. Nutzen Sie nur die Import/Export-Funktionen.

<a name="toshiba"/>Toshiba
---
- Modelle, die eine .zip-Datei mit folgendem Inhalt: chmgt.db, dvbSysData.db und dvbMainData.db.  
(z.B. RL, SL, TL, UL, VL, WL, XL, YL models of series 8xx/9xx)  
- Modelle mit einer settingsDB.db Datei

<a name="grundig"/>Grundig
---
- Modelle die Dateien mit Namen dvb\*_config.xml exportieren.
- Modelle die eine cvt_database.dat Datei exportieren, z.B. 24 GHB 5944: siehe "Sharp"

<a name="tcl"/>TCL, Thomson
---
- Modelle die eine .tar Datei exportieren, in der DtvData.db und satellite.db enthalten sind

<a name="satcodx"/>SatcoDX (Lieferant für ITT, Medion, Nabo, ok., PEAQ, Schaub-Lorenz, Silva-Schneider, Telefunken)
---
Mehrere Marken nutzen die gleiche Hardware für DVB-S und exportieren .sdx Dateien

<a name="vdr"/>VDR (Linux Video Disk Recorder)
---
Unterstützung des channels.conf Dateiformats.  
Die Implementation hierfür wurde vom Mitglied "TCr82" des VDR Projekts beigesteuert.

<a name="m3u"/>m3u (SAT>IP)
---
Unterstützt SAT>IP .m3u Dateien mit erweiterten Informationen zu Sendernamen und Programmnummern.

<a name="enigma2"/>Enigma2 (Dreambox, VU+ und viele andere Linux basierende Empfänger)
---
Erfordert eine lokale Kopie der Dateien "lamedb", "bouquets.\*" and "userbouquet.\*" aus /etc/Enigma2/.  
