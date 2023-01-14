Bağlantılar
-----
[![EN](https://chansort.com/img/flag_en_16.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme.md)
[![DE](https://chansort.com/img/flag_de_16.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme_de.md)
[![PL](https://chansort.com/img/flag_pl_16.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme_pl.md)
[![TR](https://chansort.com/img/flag_tr_24.png)](https://github.com/PredatH0r/ChanSort/blob/master/readme_tr-TR.md) |
[İndir](https://github.com/PredatH0r/ChanSort/releases) | 
[Dökümantasyon](https://github.com/PredatH0r/ChanSort/wiki) |
[Forum](https://github.com/PredatH0r/ChanSort/issues) | 
[E-Posta](mailto:horst@beham.biz)

ChanSort Hakkında
--------------
ChanSort, TV'nizin kanal listesini yeniden sıralamanıza imkân veren bir PC uygulamasıdır.  
Çoğu modern TV, daha sonra PC'nize takmakta kullanabileceğiniz USB bellekler aracılığıyla kanal listelerini dışarı aktarabilir.  
ChanSort [çok sayıda markanın dosya formatını](#supported-tv-models) destekler ve program sıralarını ve favorileri bir dosyadan bir diğerine kopyalayabilir, hatta farklı marka ve modeller arasında bile.

![ekran-görüntüsü](http://beham.biz/chansort/ChanSort-en.png)

Özellikler
--------
- Kannalların yeniden sıralanması (numaraları doğrudan, aşağı/yukarı taşıyarak, sürükle-bırakarak, çift tıklayarak değiştirin)
- Aynı sıralamayı uygulamak için başka bir kanal listesini referans olarak kullanın
- Birden fazla kanalı aynı anda düzenleyebilmek için çoklu-seçim
- Tek-liste görünümü (atanmış kanalların önce ve tüm atanmamış kanalları sonda göstermi)
- Yeni/hizalanmış listenin ve orjinal/bütün listenin yan-yana görünümü (şimdi oynatılıyor ve kütüphane gibi)
- Kanalları yeniden isimlendirin ya da silin
- Favorileri, ebevey kilidini, kanal atlamayı (kanal değiştirirken) ve kanal gizlemeyi yönetin
- Türkçe, İngilizce, Almanca, İspanyolca, Portekizce, Rusça ve Romanca kullanıcı arayüzü
- Kanal isimleri için Unicode karakter desteği (Latin, Kiril, Yunanca, ...)

Bu özellikler DESTEKLENMEZ:
- Yeni rransponder ya da kanal eklenmesi
- Kanalların uydu alıcısı ile alakalı niteliklerinin değiştirilmesi (ONID, TSID, SID, frekans, APID, VPID, ...)

Bazı özellikler tüm TV modellerinde ve kanal türlerinde geçerli olmayabilir (analog, dijital, uydu, kablo, ...)

! KULLANIM SORUMLULUĞU SİZE AİTTİR !
------------------------
Bu yazılımın büyük bir kısmı, TV üreticilerinin desteği olmadan veya dosya formatlarıyla ilgili herhangi bir resmi belgeye erişim olmadan yazılmıştır. Yalnızca eldeki veri dosyalarının, deneme yanılma analizine dayanır.
İstenmeyen yan etkiler ortaya çıkabilir ve hatta TV'nize zarar verme olasılığı vardır, bununla ilgili şu ana kadar 2 vakâ bildirilmiştir.

Hisense, teknik bilgi ve test cihazı sağlamış olan tek firmadır.

Desteklenen TV modelleri 
-------------------
ChanSort çok sayıda dosya formatını destekler, ancak her marka ve model TV için hangi dosya formatının kullanıldığını söylemek imkansızdır (-ki bu dosya formatlarının televizyonun yazılım güncellemeleriyle değişmiş olması da mümkündür).  
Bu liste nelerin desteklendiğine dair bir örnek teşkil eder, ancak yine de bu listede bulunmayan marka veya modeliniz destekleniyor olabilir:
- [Samsung](source/fileformats.md#samsung)
- [LG](source/fileformats.md#lg)
- [Sony](source/fileformats.md#sony)
- [Hisense](source/fileformats.md#hisense)
- [Panasonic](source/fileformats.md#panasonic)
- [TCL](source/fileformats.md#tcl)
- [Philips](source/fileformats.md#philips)
- [Sharp, Dyon, Blaupunkt, Hisense, Changhong, Grundig, alphatronics, JTC Genesis, ...](source/fileformats.md#sharp)
- [Toshiba](source/fileformats.md#toshiba)
- [Grundig](source/fileformats.md#grundig)
- [SatcoDX: ITT, Medion, Nabo, ok., PEAQ, Schaub-Lorenz, Silva-Schneider, Telefunken, ...](source/fileformats.md#satcodx)
- [DBM: Xoro, Strong, TechniSat, ...](source/fileformats.md#dbm)
- [Vision Edge 4K](source/fileformats.md#visionedge)
- [VDR](source/fileformats.md#vdr)
- [SAT>IP m3u](source/fileformats.md#m3u)
- [Enigma2](source/fileformats.md#enigma2)

Sistem Gereksinimleri
-------------------
**Windows**:  
- Windows 7 SP1, Windows 8.1, Windows 10 v1606 ve üzeri, Windows 11 (x86, x64 ya da ARM CPU)
- [Microsoft .NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework)
- .NET FW 4.8 şâyet ki, SP1 yüklü değilse Windows 7 üzerinde çalışmayacaktır, Windows 8 ya da Windows 10 v1606 ve üzeri

**Linux**:  
- wine (sudo apt-get install wine)
- winetricks (sudo apt-get install winetricks)
- winetricks'i başlatın, wineprefix'i seçin ya da oluşturun (32 bit ya da 64 bit), "Install Windows DLL or component"i seçin ve "dotnet48" paketini yükleyin, bu sırada çıkan düzinelerce uyarı mesajını görmezden gelin
- ChanSort.exe'ye sağ tıklayın ve "open with", "all applications", "A wine application" sırasınca seçin

**Mac**
- macOS doğrudan desteklenmez, ancak Mac'te Windows 10/11 ile bir VM kurmak için Parallels veya UTM kullanabilirsiniz
- m1/ARM CPU'lu Mac'ler için talimatlar: https://history-computer.com/how-to-run-windows-on-m1-macs/

**Donanım**:
- TV'niz ile PC'niz arasında kanal listesini aktarabilmek için USB bellek/SD-kart. En azından 32 GB bir bellek şiddetle tavsiye olunur. (Bazı TV'ler çöplerini NTFS dosya ssitemine yazar ve exFAT'ı desteklemezler bile.)

Kaynaktan derleme
-----------------
[build.md](source/build.md)'i gözden geçirin.

Lisans (GPLv3)
---------------
GNU Genel Kamu Lisansı, Versiyon 3: http://www.gnu.org/licenses/gpl.html  
Kaynak koduna https://github.com/PredatH0r/ChanSort adresinden erişilebilir

BU YAZILIM; PAZARLANABİLİRLİK, BELİRLİ BİR AMACA UYGUNLUK VE HAK İHLALİ OLMAMASINA DAİR GARANTİLERİN DE DAHİL OLDUĞU AMA BUNLARLA SINIRLI OLMAYAN DOĞRUDAN VEYA DOLAYLI HERHANGİBİR GARANTİ OLMAKSIZIN "OLDUĞU GİBİ" SAĞLANMIŞTIR.

YAZILIM SAHİBİ, SÖZLEŞME DAVASINDA, HAKSIZ MUAMELE İLE YA DA BAŞKA BİR ŞEKİLDE OLSUN YA DA OLMASIN YAZILIMIN KULLANIMI VEYA YAZILIM İLE İLİŞKİLİ DİĞER MESELELERDEN YA DA YAZILIM DIŞI VEYA YAZILIM İLE BAĞLANTILI OLMASINDAN KAYNAKLI HİÇBİR DURUMDA HERHANGİ BİR TALEP, ZARAR VEYA BAŞKA BİR YÜKÜMLÜLÜKTEN MESÛL OLMAYACAKTIR.
