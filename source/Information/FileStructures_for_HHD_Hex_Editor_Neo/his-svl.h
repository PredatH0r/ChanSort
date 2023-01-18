#include "chansort.h"

// common ---------------------------

enum TableId : word
{
  Antenna = 1, Cable = 2, Sat = 3
};

struct Header
{
  word id;
  char name[34];
  dword size;
};

enum BroadcastType : byte
{
  Analog = 1,
  Dvb = 2,
};

// TSL ------------------------------

enum Medium : byte
{
  DigAnt = 1,
  DigCable = 2,
  DigSat = 3,
  AnaAnt = 4,
  AnaCable = 5,
  AnaSat = 6
};

enum HisServiceType : byte
{
  TV = 1, Radio = 2, App = 3
};

enum BandwidthType { Unknown, Mhz6, Mhz7, Mhz8 };

struct TslRecord
{
  var off0 = current_offset;
  word id;

  BroadcastType broadcastType;
  Medium medium;
  word nid;
  word onid;
  word tsid;
  word nwlRecId;
  byte nwlVal;
  byte u1[3];

  dword freq;
  union
  {
    dword symRate;
    BandwidthType bwType;
  } bandwidth;
  byte u2[192];
  byte name[32];
  var off1 = current_offset;
  byte padding[tslRecordSize - (off1 - off0) - 12];
  dword mask;
  word nwlTblId;
  word nwlRecId;
  word satTblId;
  word satRecId;
};

public struct HIS_TSL
{
  var tslRecordSize = 328;
  Header tsl1;
  Header tsl2;
  Header tsl3;

  var count = tsl1.size / tslRecordSize;
  TslRecord antenna[count];

  var count = tsl2.size / tslRecordSize;
  TslRecord cable[count];

  var count = tsl3.size / tslRecordSize;
  TslRecord sat[count];
};




// SVL ------------------------------

// bit values defined in https://patents.google.com/patent/CN102523534B/en

enum NwMask : dword
{
  Skip = 1 << 3,
  Fav1 = 1 << 4,
  Fav2 = 1 << 5,
  Fav3 = 1 << 6,
  Fav4 = 1 << 7,
  Lock = 1 << 8
};

enum OptionMask : dword
{
  Rename = 1 << 3,
  Moved = 1 << 10
};

enum Option2Mask : dword
{
  Unknown = 0
};

enum Hashcode : dword
{
  Name = 0x0001,
  ServiceId = 0x0002,
  BroadcastType = 0x0004,
  TlsRecId = 0x0008,
  PrgNum = 0x0010,
  ShortName = 0x0020,
  Radio = 0x0400, // guess
  Encrypted = 0x0800, // guess
  Tv = 0x2000 // guess
};

enum DvbServiceRunningStatus : dword
{
  Unknown,
  Undefined,
  NotRunning,
  StartInSeconds,
  Pausing,
  Running,
  OffAir
};

enum LinkageMask : dword
{
  Nid = 0x01,
  Onid = 0x02,
  Tsid = 0x04
};

struct DvbId
{
  word onid;
  word tsid;
  word sid;
};

enum DvbReplacementMask: dword
{
  CaReplacement = 0x01,
  ServiceReplacement = 0x02,
  CmpltEitSch = 0x04,
  EpgPid = 0x08
};

struct SvlDvb
{
  dword reserved;
  byte shortName[18];
  word caSystemId;
  DvbServiceRunningStatus runningStatus;
  DvbReplacementMask replacementMask;
  DvbId caReplacement;
  DvbId svcReplacement;
  DvbId cmplEitSchOn;
  DvbId hdSimulcastOn;
  DvbId orig;
  word epgPid;
  bool freeCaMode;
  byte ciHostShunningMode;

  dword ciPlusPrevChannelId;
  byte ciPlusCiTuneService;
  byte nonStandard[10];

  byte downmixMode;
  byte subtitlePref1[4];
  byte subtitlePref2[4];
  qword svcTimeStarted;
  qword svcTimeDuration;
  word lastViewSvlTableId;
  word lastViewSvlRecordId;
  byte mfType;
  bool eitPfFlag;
  word curLcn;
  word origLcn;
  word lcnIndex;
  word dvbcTsid;
  word dvbcOnid;
  byte audioIndex;
  bool dvbsDpIdFlag;
  word dvbsDpTsid;
  word dvbsDpSid;
  bool hbbtvOff;
  byte sdtServiceType;
  byte doNotScramble;
  byte dadInfo[33];
};

[display(format("{0} - {1}", header.channelId.progNr, trim(header.name)))]
struct SvlRecord
{
  // different sizes possible: 304 (observed), 308, and lots of smaller ones
  var off0 = current_offset;
  word recordId;
  struct
  {
    dword nonStandard;
    struct
    {
      word unk : 2;
      word progNr : 14;
    } channelId;
    union
    {
      // !! file uses a different field order than documentation/definition !!
      // Hisense seems to have accidentally put the hashcode and nwMask to the same location
      // and there's indication that Skip is actually reversed (1=noskip)
      Hashcode hashcode;
      NwMask nwMask;
    } hashcodeAndNwMask;
    OptionMask optionMask;
    Option2Mask option2Mask;
    dword unknown;
    word serviceId;
    word tslTableId;
    word tslRecordId;
    word nwlTableId;
    word nwlRecordId;
    word satTableId;
    word satRecordId;
    BroadcastType broadcastType;
    HisServiceType serviceType;
    byte name[65];
    byte nonStandard2[3];
    byte customData[8];
    byte privateData[20];
  } header;

  union
  {
    SvlDvb dvb;
    byte padding[168];
  } broadcastSystemData;
  var off1 = current_offset;
  byte padding[svlRecordSize - (off1 - off0)];
};

public struct HIS_SVL
{
  var svlRecordSize = 304;

  Header svl1;
  Header svl2;
  Header svl3;

  var count = svl1.size / svlRecordSize;
  SvlRecord antenna[count];

  var count = svl2.size / svlRecordSize;
  SvlRecord cable[count];

  var count = svl3.size / svlRecordSize;
  SvlRecord sat[count];
};



// HIS_FAV -----------------------------------

[display(format("{0} - {1}", trim(progNumDisplayInFavList), trim(name)))]
struct FavRecord
{
  word svlTableId;
  word svlRecordId;
  char progNumDisplayInFavList[11];
  char name[65];
};

public struct HIS_FAV
{
  dword fav1size;
  dword fav2size;
  dword fav3size;
  dword fav4size;

  FavRecord fav1[fav1size / sizeof(FavRecord)];
  FavRecord fav2[fav2size / sizeof(FavRecord)];
  FavRecord fav3[fav3size / sizeof(FavRecord)];
  FavRecord fav4[fav4size / sizeof(FavRecord)];
};