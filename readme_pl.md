Linki
-----
[![EN](https://chansort.com/img/flag_en_16.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme.md)
[![DE](https://chansort.com/img/flag_de_16.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme_de.md)
[![PL](https://chansort.com/img/flag_pl_24.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme_pl.md)
[![TR](https://chansort.com/img/flag_tr_16.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme_tr-TR.md) |
[Pobierz](https://github.com/PredatH0r/ChanSort/releases) | 
[Dokumentacja](https://github.com/PredatH0r/ChanSort/wiki) |
[Forum](https://github.com/PredatH0r/ChanSort/issues) | 
[E-Mail](mailto:horst@beham.biz)

O ChanSort
--------------
ChanSort to aplikacja na komputery PC, która umożliwia zmianę kolejności listy kanałów w telewizorze.
Większość nowoczesnych telewizorów może przesyłać listy kanałów za pomocą pamięci USB, którą można podłączyć do komputera.
ChanSort obsługuje [formaty plików wielu marek] (#obsługiwane modele telewizorów) i może kopiować numery programów i
ulubione z jednego pliku do drugiego, nawet między różnymi modelami i markami.

![screenshot](http://beham.biz/chansort/ChanSort-en.png)

Cechy
--------
- Zmiana kolejności kanałów (bezpośrednia zmiana numerów, przesuwanie w górę/w dół, przeciąganie i upuszczanie, podwójne kliknięcie)
- Użyj innej listy kanałów jako odniesienia, aby zastosować tę samą kolejność
- Wielokrotny wybór do edycji wielu kanałów jednocześnie
- Widok pojedynczej listy (pokazuje najpierw przypisane kanały, a następnie wszystkie nieprzypisane kanały)
- Widok obok siebie nowej/posortowanej listy i oryginalnej/pełnej listy (podobny do listy odtwarzania i biblioteki)
- Zmień nazwę lub usuń kanały
- Zarządzaj ulubionymi, blokadą rodzicielską, pomijaniem kanałów (podczas zappingu), ukrywaniem kanałów
- Interfejs użytkownika w języku angielskim, niemieckim, hiszpańskim, tureckim, portugalskim, rosyjskim i rumuńskim
- Obsługa znaków Unicode dla nazw kanałów (łaciński, cyrylica, grecki, ...)

Nieobsługiwany:
- dodawanie nowych transponderów lub kanałów
- zmiana właściwości kanałów związanych z tunerem (ONID, TSID, SID, częstotliwość, APID, VPID, ...)

Niektóre funkcje mogą nie być dostępne we wszystkich modelach telewizorów i typach kanałów (analogowe, cyfrowe, satelitarne, kablowe, ...)

! UŻYWASZ NA WŁASNE RYZYKO!
------------------------
Większość tego oprogramowania została napisana bez wsparcia producentów telewizorów lub dostępu do jakiegokolwiek urzędnika
dokumentacja dotycząca formatów plików. Opiera się wyłącznie na analizie istniejących plików danych, próbach i błędach.
Istnieje prawdopodobieństwo wystąpienia niepożądanych skutków ubocznych lub nawet uszkodzenia telewizora, co odnotowano w 2 przypadkach.

Hisense jest jedynym producentem, który dostarczył informacje techniczne i urządzenie testowe.

Obsługiwane modele telewizorów
-------------------
ChanSort obsługuje dużą liczbę formatów plików, ale nie można określić dla każdej marki i modelu telewizora
jaki format pliku używa (może się nawet zmienić wraz z aktualizacjami oprogramowania układowego).
Ta lista zawiera kilka przykładów tego, co powinno być obsługiwane, ale nawet jeśli Twojego modelu lub marki nie ma na tej liście,
to może i tak zadziałać:
- [Samsung](source/fileformats.md#samsung)
- [LG](source/fileformats.md#lg)
- [Sony](source/fileformats.md#sony)
- [Hisense](source/fileformats.md#hisense)
- [Panasonic](source/fileformats.md#panasonic)
- [Philips](source/fileformats.md#philips)
- [Sharp, Dyon, Blaupunkt, Hisense, Changhong, Grundig, alphatronics, JTC Genesis, ...](source/fileformats.md#sharp)
- [Toshiba](source/fileformats.md#toshiba)
- [Grundig](source/fileformats.md#grundig)
- [SatcoDX: ITT, Medion, Nabo, ok., PEAQ, Schaub-Lorenz, Silva-Schneider, Telefunken, ...](source/fileformats.md#satcodx)
- [VDR](source/fileformats.md#vdr)
- [SAT>IP m3u](source/fileformats.md#m3u)
- [Enigma2](source/fileformats.md#enigma2)

Wymagania systemowe
-------------------
**Windows**:  
- Windows 7 SP1, Windows 8.1, Windows 10 v1606 or later, Windows 11 (with x86, x64 or ARM CPU)
- [Microsoft .NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework)
- The .NET FW 4.8 does NOT work with Windows 7 without SP1, Windows 8 or Windows 10 prior to v1606

**Linux**:  
- wino (sudo apt-get install wine)
- winetricki (sudo apt-get install winetrics)
- uruchom winetricki, wybierz lub utwórz prefiks wina (32-bitowy lub 64-bitowy), wybierz
  "Zainstaluj Windows DLL lub komponent" i zainstaluj pakiet "dotnet48" i zignoruj dziesiątki okienek komunikatów- right-click on ChanSort.exe and select "open with", "all applications", "A wine application"

**Sprzęt komputerowy**:
- Pamięć USB/karta SD do przesyłania listy kanałów między telewizorem a komputerem. Pendrive <= 32 GB z systemem plików FAT32
  jest ZDECYDOWANIE zalecane. (Niektóre telewizory zapisują śmieci do NTFS i w ogóle nie obsługują exFAT)

Zbudowany na źródle
-----------------
See [build.md](source/build.md)

Licencja (GPLv3)
---------------
Powszechna Licencja Publiczna GNU, wersja 3: http://www.gnu.org/licenses/gpl.html  
Kod źródłowy jest dostępny na https://github.com/PredatH0r/ChanSort

OPROGRAMOWANIE JEST DOSTARCZANE „TAK JAK JEST”, BEZ JAKICHKOLWIEK GWARANCJI,
WYRAŹNE LUB DOROZUMIANE, W TYM MIĘDZY INNYMI GWARANCJE
SPRZEDAWALNOŚĆ, PRZYDATNOŚĆ DO OKREŚLONEGO CELU I NIENARUSZALNOŚCI.

W ŻADNYM WYPADKU AUTORZY NIE PONOSZĄ ODPOWIEDZIALNOŚCI ZA JAKIEKOLWIEK ROSZCZENIA, SZKODY LUB
INNA ODPOWIEDZIALNOŚĆ UMOWA, CZYNOWA LUB W INNY SPOSÓB,
WYNIKAJĄCE Z, Z LUB W ZWIĄZKU Z OPROGRAMOWANIEM LUB UŻYTKOWANIEM LUB
INNE CZYNNOŚCI W OPROGRAMOWANIU.
