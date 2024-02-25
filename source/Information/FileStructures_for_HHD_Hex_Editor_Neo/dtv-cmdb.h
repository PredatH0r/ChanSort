#include "chansort.h"
#pragma script("get_doc_size.js")

/*
structure definition for various variants of MStar (aka MorningStar) DVB-C and DVB-S receiver channel list formats.
Also used by various models from brands like AEG, Akiwa, Auvisio, Boca, Botech, Comag, Dyon, LogiSat, Kjaerulff, Micro, Megasat, Schwaiger, SeaSat, Strong, TechniSat, TeleSystem, Trekstor, Xoro, Zehnder, ...
Typical file names include: vodafone.DBM, HB_DATABASE_6_29.DBM, MAS_HRS8520_23_08_2011.DBM, ...
*/ 

struct s_Satellite
{
  var off0 = current_offset;
  word u;
  byte Name[34];
  word LowFreq;
  word HighFreq;
  var off1 = current_offset;
  byte u1[50 - (off1 - off0)];
  word OrbitalPos;

  var off1 = current_offset;
  byte unk[satRecordLength - (off1 - off0)];
};

struct s_Transponder
{
  var off0 = current_offset;
  byte SatIndex;
  byte unk1[5];
  word Tsid;
  word Onid;
  word Nid;
  byte u[2];
  word transponderIndex;
  word FreqInMhz;
  byte unk2[10];
  word SymRate;
  var off1 = current_offset;
  byte unk[transponderRecordLength - (off1 - off0)];
};

enum e_Favorites : byte
{
  A=0x01,
  B=0x04,
  C=0x08,
  D=0x10
};

enum e_Flags : byte
{
  Encrypted=0x10,
  Skip=0x20,
  Lock=0x40
};

enum e_ServiceType : byte
{
  TV=1,
  Radio=2
};

struct s_Channel
{
  var off0 = current_offset;
  word Index;
  byte u0[13];
  //e_Flags Flags;
  //byte u1;
  
  byte ChannelType;
  byte ServiceType;

  byte u1[3];
  word TransponderIndex;
  word PmtPid;
  word u2;
  word PcrPid;
  word VideoPid;
  word u3;
  word ProgNr;
  word ServiceId;
  byte u4[14];
  char AudioLang1[4];
  word AudioPid1;
  char AudioLang2[4];
  word AudioPid2;
  byte u90[84];
  char Name[50];
  char Provider[224];

  var off1 = current_offset;
  byte unk[channelRecordLength - (off1-off0)];
};


public struct dtv_cmdb_2_unified
{
  var headerLength = 0;
  var channelBitmapLength = 0;
  var channelRecordCount = 0;
  var channelRecordLength = 0;
  var channelBlockUnknownLength = 0;
  var transponderBitmapLength = 0;
  var transponderRecordCount = 0;
  var transponderRecordLength = 0;
  var transponderBlockUnknownLength = 0;
  var satBitmapLength = 0;
  var satRecordCount = 0;
  var satRecordLength = 0;

  switch (GetDocumentSize())
  {
  case 2731173:
    // Dijitsu LDM538
    headerLength = 8;
    channelBitmapLength = 750;
    channelRecordCount = 6000;
    channelRecordLength = 424;
    channelBlockUnknownLength = 2;
    transponderBitmapLength = 376;
    transponderRecordCount = 3200;
    transponderBlockUnknownLength = 3348;
    transponderRecordLength = 52;
    satBitmapLength = 32;
    satRecordCount = 254;
    satRecordLength = 64;
 	  break;
 
  default:
    $assert(false, "Structure for this file size is not supported");
    break;
  }
	
  byte Header[headerLength];

  byte ChannelBitmap[channelBitmapLength];
  byte unk[2];
  s_Channel ChannelData[channelRecordCount];
  //byte ChannelBlockUnknown[channelBlockUnknownLength];

  byte TransponderBitmap[transponderBitmapLength];
  s_Transponder TransponderData[transponderRecordCount];
  byte TransponderBlockUnknown[transponderBlockUnknownLength];

  byte SatelliteBitmap[satBitmapLength];
  s_Satellite SatelliteData[satRecordCount];

  byte Extra[*];
};

