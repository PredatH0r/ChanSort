#include "chansort.h"

struct s_Transponder
{
  var off0 = current_offset;
  dword Freq;
  byte unk1[4];
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
  byte transponderIndex;
  byte u4[25];
  word tsid;
  word onid;
  word sid;

  byte u5[2];
  word pcrPidMaybe;
  word vpidMaybe;

  var off1 = current_offset;
  byte unk[160 - (off1-off0)];
};


public struct DBM_163772_DvbC
{
  word BytesumPlus0x55;
  dword DataLengthForBytesum;

  byte TransponderBitmap[16];
  s_Transponder TransponderData[100];

  var off0 = current_offset;
  byte unk1[0x0E3C - off0];

  byte ChannelBitmap[126];
  s_Channel ChannelData[1000];

  byte Extra[*];
};

