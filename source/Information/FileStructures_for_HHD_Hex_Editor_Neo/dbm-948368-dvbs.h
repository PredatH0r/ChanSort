#include "chansort.h"

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
  byte unk[76 - (off1 - off0)];
};

struct s_Transponder
{
  var off0 = current_offset;
  dword Freq;
  byte unk1[16];
  word SymRate;
  var off1 = current_offset;

  byte unk[36 - (off1 - off0)];
};

struct s_Channel
{
  var off0 = current_offset;
  byte Name[64];
  word progNrMinus1;
  word lcn;
  byte u3[2];
  word satelliteIndex;
  word transponderIndex;
  byte u4[26];
  word tsid;
  word onid;
  word sid;

  byte u5[2];
  word pcrPidMaybe;
  word vpidMaybe;

  var off1 = current_offset;
  byte unk[164 - (off1-off0)];
};


public struct DBM_948368_DvbS
{
  word BytesumPlus0x55;
  dword DataLengthForBytesum;

  byte SatelliteBitmap[32];
  s_Satellite SatelliteData[254];

  byte TransponderBitmap[376];
  s_Transponder TransponderData[3000];

  var off0 = current_offset;
  byte unk1[0x1f2fc - off0];

  byte ChannelBitmap[626];
  s_Channel ChannelData[5000];

  byte Extra[*];
};

