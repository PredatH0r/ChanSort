#include "tll-common.h"

struct LH164_AnalogChannel
{
  byte t1[2];
  LH_SignalSource SignalSource;
  byte t1b[6];
  byte ChannelTransponder1;
  word ProgramNr;
  word LogicalProgramNr1;
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
  byte t5a[14]; // !
  word ONID;
  word TSID;
  byte t5b[6];
  dword Frequency;
  byte t6[8];
  word ProgramNrTimes4;
  word LogicalProgramNr2;
  byte ChannelTransponder4;
  byte _Favorites2;
  byte LockSkipHide;
  byte ServiceType;
  char CH_Name2[40];
  word Frequency2Div50;
  word APID2;
  word t11;
  word t12;
};


struct LH164_DvbCtChannel
{
  byte t1[2];
  LH_SignalSource SignalSource;
  byte t1b[6];
  byte ChannelTransponder1;
  word ProgramNr;
  word LogicalProgramNr2;
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
  byte t5a[14]; // !
  word ONID;
  word TSID;
  byte t5b[6];
  dword Frequency;
  byte t6[8];
  word ProgramNrTimes4;
  word LogicalProgramNr2;
  byte ChannelTransponder4;
  byte _Favorites2;
  byte LockSkipHide;
  byte ServiceType;
  char CH_Name2[40];
  word PcrPid2;
  word APID2;
  word t11;
  word SID2;
};


struct LH164_AnalogBlock
{
  dword BlockSize; 
  dword ChannelCount;
  LH164_AnalogChannel Channels[ChannelCount];
};

struct LH164_FirmwareBlock
{
  dword BlockSize;
  byte Data[BlockSize];
};

struct LH164_DvbCTBlock
{
  dword BlockSize;
  dword ChannelCount;
  LH164_DvbCtChannel Channels[ChannelCount];
};


public struct LH164
{
  LH164_AnalogBlock Analog;
  LH164_FirmwareBlock Firmware;
  LH164_DvbCTBlock DvbCT;
};
