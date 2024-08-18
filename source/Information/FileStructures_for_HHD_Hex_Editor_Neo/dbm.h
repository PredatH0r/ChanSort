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
  byte Name[34];
  word LowFreq;
  word HighFreq;
  var off1 = current_offset;
  byte u1[74 - (off1 - off0)];
  word OrbitalPos;

  var off1 = current_offset;
  byte unk[satRecordLength - (off1 - off0)];
};

struct s_Transponder
{
  var off0 = current_offset;
  dword Freq;
  byte unk1[16];
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
  Hide=0x04,
  Skip=0x08,
  Lock=0x10
};

enum e_ServiceType : byte
{
  TV=1,
  Radio=2
};

struct s_Channel
{
  var off0 = current_offset;
  byte Name[64];
  word progNrMinus1;
  word lcn;
  byte u3[2];
  union
  {
    struct
    {
      word satelliteIndex;
      word transponderIndex;
    } dvbs;
    struct
    {
      byte transponderIndex;
      byte unknown;
      word dummy;
    } dvbc;
  } transponder;
  byte u4[bytesBetweenTransponderIndexAndServiceType];
  e_ServiceType serviceType;
  e_Flags flags;
  byte u5;
  e_Favorites favorites;
  byte u6[16];
  word tsid;
  word onid;
  word sid;

  byte u7[2];
  word pcrPidMaybe;
  word vpidMaybe;
  word pmt;

  var off1 = current_offset;
  byte unk[channelRecordLength - (off1-off0)];
};


public struct DBM
{
  var hasHeader = 1;
  var satBitmapLength = 0;
  var satRecordCount = 0;
  var satRecordLength = 0;
  var transponderBitmapLength = 0;
  var transponderRecordCount = 0;
  var transponderRecordLength = 0;
  var unknownDataAfterTransponderData = 0;
  var channelBitmapLength = 0;
  var unknownDataAfterChannelBitmap = 0;
  var channelRecordCount = 0;
  var channelRecordLength = 0;
  var bytesBetweenTransponderIndexAndServiceType = 6;

  switch (GetDocumentSize())
  {
  case 74303:
    // Renkforce 1510 C HD, Telestar digiHD TC 7
    satBitmapLength = 0;
    satRecordCount = 0;
    satRecordLength = 0;
    transponderBitmapLength = 16;
    transponderRecordCount = 100;
    transponderRecordLength = 36;
    unknownDataAfterTransponderData = 22;
    channelBitmapLength = 50;
    channelRecordCount = 400;
    channelRecordLength = 176;
    bytesBetweenTransponderIndexAndServiceType = 2;
	  break;
  case 100120:
    // XORO 7660
    hasHeader = 0;
    satBitmapLength = 0;
    satRecordCount = 0;
    satRecordLength = 0;
    transponderBitmapLength = 16;
    transponderRecordCount = 100;
    transponderRecordLength = 40;
    unknownDataAfterTransponderData = 22;
    channelBitmapLength = 78;
    channelRecordCount = 600;
    channelRecordLength = 160;
    bytesBetweenTransponderIndexAndServiceType = 2;
    break;
  case 109720:
    // XORO DVB-C tuner
    hasHeader = 0;
    satBitmapLength = 0;
    satRecordCount = 0;
    satRecordLength = 0;
    transponderBitmapLength = 16;
    transponderRecordCount = 100;
    transponderRecordLength = 40;
    unknownDataAfterTransponderData = 22;
    channelBitmapLength = 78;
    unknownDataAfterChannelBitmap = 0;
    channelRecordCount = 600;
    channelRecordLength = 176;
    bytesBetweenTransponderIndexAndServiceType = 2;
    break;
  case 163772:
    // TechniSat DVB-C TS_Programmliste_06_01.DBM
    satBitmapLength = 0;
    satRecordCount = 0;
    satRecordLength = 84;
    transponderBitmapLength = 16;
    transponderRecordCount = 100;
    transponderRecordLength = 36;
    unknownDataAfterTransponderData = 22;
    channelBitmapLength = 126;
    channelRecordCount = 1000;
    channelRecordLength = 160;
    bytesBetweenTransponderIndexAndServiceType = 2;
    break;
  case 781736:
    // Strong HB_DATABASE_1_18.DBM, Xoro HB_DATABASE_6_29.DBM
    satBitmapLength = 28;
    satRecordCount = 200;
    satRecordLength = 84;
    transponderBitmapLength = 376;
    transponderRecordCount = 3000;
    transponderRecordLength = 36;
    unknownDataAfterTransponderData = 22;
    channelBitmapLength = 502;
    channelRecordCount = 4000;
    channelRecordLength = 164;
    break;
  case 785256:
    // Strong HB_DATABASE_5_4.DBM
    satBitmapLength = 32;
    satRecordCount = 254;
    satRecordLength = 80;
    transponderBitmapLength = 376;
    transponderRecordCount = 3000;
    transponderRecordLength = 36;
    unknownDataAfterTransponderData = 20;
    channelBitmapLength = 500;
    channelRecordCount = 4000;
    channelRecordLength = 164;
    break;
  case 793736:
    // Xoro HB_DATABASE_8_19.DBM
    satBitmapLength = 28;
    satRecordCount = 200;
    satRecordLength = 84;
    transponderBitmapLength = 376;
    transponderRecordCount = 3000;
    transponderRecordLength = 40;
    unknownDataAfterTransponderData = 22;
    channelBitmapLength = 502;
    channelRecordCount = 4000;
    channelRecordLength = 164;
    break;
  case 862272:
    // Orbitech IR440 HB_DATABASE_5_3.DBM
    satBitmapLength = 32;
    satRecordCount = 254;
    satRecordLength = 84;
    transponderBitmapLength = 376;
    transponderRecordCount = 3000;
    transponderRecordLength = 40;
    unknownDataAfterTransponderData = 20;
    channelBitmapLength = 500;
    channelRecordCount = 4000;
    channelRecordLength = 180;
    break;
  case 948368:
    // Comag SL40HD_V1_17_02, Xoro HRS 8520, ...
    satBitmapLength = 32;
    satRecordCount = 254;
    satRecordLength = 76;
    transponderBitmapLength = 376;
    transponderRecordCount = 3000;
    transponderRecordLength = 36;
    unknownDataAfterTransponderData = 22;
    channelBitmapLength = 626;
    channelRecordCount = 5000;
    channelRecordLength = 164;
    break;
  default:
    $assert(false, "Structure for this file size is not supported");
    break;
  }

  if (hasHeader != 0) {
    word BytesumPlus0x55;
    dword DataLengthForBytesum;
  }

  byte SatelliteBitmap[satBitmapLength];
  s_Satellite SatelliteData[satRecordCount];

  byte TransponderBitmap[transponderBitmapLength];
  s_Transponder TransponderData[transponderRecordCount];

  byte unknown[unknownDataAfterTransponderData];

  byte ChannelBitmap[channelBitmapLength];

  byte unknown2[unknownDataAfterChannelBitmap];

  s_Channel ChannelData[channelRecordCount];

  byte Extra[*];
};

