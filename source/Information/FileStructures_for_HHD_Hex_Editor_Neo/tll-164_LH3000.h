#include "tll-common.h"


struct LH3000_AnalogChannel
{
  byte t1[2];
  LH_SignalSource SignalSource;
  byte t1b[6];
  byte ChannelTransponder1;
  word ProgramNr;
  word LogicalProgramNr1;
  word t2[2];
  byte Favorites1;
  byte t2d;
  word Freqency1Div50;
  word APID1;
  byte ChannelNumberInBand;
  byte ChannelBand;
  byte t3[2];
  char CH_Name1[40];
  byte CH_NameLength1;
  byte t4;
  word SID1;
  byte t5a[14];
  word ONID;
  word TSID;
  byte t5b[14];
  word ProgramNrTimes4;
  word LogicalProgramNr2;
  byte ChannelTransponder4;
  byte _Favorites2;
  byte LockSkipHide;
  byte CH_NameLength2;
  char CH_Name2[40];
  word Frequency2Div50;
  word APID2;
  word t11;
  word t12;
  ServiceType ServiceType;
  byte t13[3];
};


struct LH3000_DvbCtChannel
{
  byte t1[2];
  LH_SignalSource SignalSource;
  byte t1b[6];
  byte ChannelTransponder1;
  word ProgramNr;
  word LogicalProgramNr1;
  word t2[2];
  byte Favorites1;
  byte t2d;
  word PcrPid;
  word APID1;
  word VPID1;
  byte t3[2];
  char CH_Name1[40];
  byte CH_NameLength1;
  byte t4;
  word SID1;
  byte t5a[14];
  word ONID;
  word TSID;
  byte t5b[14];
  word ProgramNrTimes4;
  word LocicalProgramNr2;
  byte ChannelTransponder4;
  byte _Favorites2;
  byte LockSkipHide;
  byte Ch_NameLength2;
  char CH_Name2[40];
  word PcrPid2;
  word APID2;
  word t11;
  word SID2;
  ServiceType ServiceType;
  byte t13[3];
};



struct LH3000_AnalogBlock
{
  dword BlockSize; 
  dword ChannelCount;
  LH3000_AnalogChannel Channels[ChannelCount];
};

struct LH3000_FirmwareBlock
{
  dword BlockSize;
  byte Data[BlockSize];
};

struct LH3000_DvbCTBlock
{
  dword BlockSize;
  dword ChannelCount;
  LH3000_DvbCtChannel Channels[ChannelCount];
};


public struct LH3000
{
  LH3000_AnalogBlock Analog;
  LH3000_FirmwareBlock Firmware;
  LH3000_DvbCTBlock DvbCT;
};
