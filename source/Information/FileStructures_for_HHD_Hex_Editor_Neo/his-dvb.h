#include "chansort.h"

[display(format("{0}", trim(lang)))]
struct LanguageInfo
{
  byte lang[4];
  byte u2[2];
  word audioPid;
  byte u2[6];  
};

enum SubList : byte
{
  TV=1, Radio=2, Data=3
};

[display(format("{0} - {1}", progNr, trim(name)))]
struct DvbChannel
{
  byte u1[21];
  byte skip;
  byte lock;
  byte u2[6];
  SubList list;
  byte isFav;
  byte u3[4];
  ServiceType serviceType;
  word pcrPid;
  word videoPid;
  word progNr;
  byte u4[4];
  word pmtPid;
  word serviceId;
  byte u5[12];
  LanguageInfo languages[3];
  byte u6[182];
  char name[50];
  char provider[50];
  byte u7[10];
  word u8;
  word tsid;
  word onid1;
  word onid2;
  byte u9[4];
  word freq;
  byte u10[10];
  dword symbolRate;
  byte u11[74];
};

[display(format("{0}.{1} - {2}", dvb.list, dvb.progNr, trim(dvb.name)))]
struct SatChannel
{
  DvbChannel dvb;
  word u1;
  char satName[32];
  byte u11a[2];
  word lowFreq;
  word highFreq;
  byte u11b[10];
  byte orbitalPos;
  byte u12[13];
};
  
public struct HIS_DVB_BIN
{
  dword versionMaybe;
  dword numAntennaChannels;
  dword numCableChannels;
  dword numSatChannels;

  DvbChannel antennaChannels[numAntennaChannels];
  DvbChannel cableChannels[numCableChannels];
  SatChannel satChannels[numSatChannels];
};
