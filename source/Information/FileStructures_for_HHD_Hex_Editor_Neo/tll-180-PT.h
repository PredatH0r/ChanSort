#include "tll-common.h"

struct PT180_AnalogChannel
{
  byte t1[8];
  TLL_SignalSource SignalSource;
  byte t1b;
  word ChannelTransponder1;
  word ProgramNr;
  word LogicalProgramNr1;
  word t2[2];
  byte Favorites1;
  byte t2b;
  word Frequency1Div50;
  word APID1;
  byte ChannelNumberInBand;
  byte ChannelBand;
  byte t3[2];
  char CH_Name1[40];
  byte CH_NameLength1;
  byte t4[1];
  word SID1;
  byte t5a[16];
  word ChannelTransponder2;
  dword Frequency;
  byte t6[2];
  word ONID;
  word TSID;
  byte t7[18];
  word ProgramNrTimes4;
  word LogicalProgramNr2;
  word ChannelTransponder4;
  byte Favorites2;
  byte LockSkipHide;
  word SID2;
  byte ServiceType; 
  byte CH_NameLength2;
  char CH_Name2[40];
  word Frequency2Div50;
  word APID2;
  byte t11[4];
};


struct PT180_DvbCtChannel
{
  byte t1[8];
  TLL_SignalSource SignalSource;
  byte t1b;
  word ChannelTransponder1;
  word ProgramNr;
  word LogicalProgramNr2;
  word t2[2];
  byte Favorites1;
  byte t2d;
  word PcrPid1;
  word APID1;
  word VPID1;
  byte t3[2];
  char CH_Name1[40];
  byte CH_NameLength1;
  byte t4;
  word SID1;
  byte t5a[16];
  word ChannelTransponder2;
  dword Frequency;
  byte t6[2];
  word ONID;
  word TSID;
  byte t7[18];
  word ProgramNrTimes4;
  word LogicalProgramNr2;
  word ChannelTransponder4;
  byte Favorites2;
  byte LockSkipHide;
  word SID2;
  byte ServiceType; 
  byte CH_NameLength2;
  char CH_Name2[40];
  word PcrPid2;
  word APID2;
  byte t11[4];
};

struct PT180_AnalogBlock
{
  dword BlockSize; 
  dword ChannelCount;
  PT180_AnalogChannel Channels[ChannelCount];
};

struct PT180_FirmwareBlock
{
  dword BlockSize;
  byte Data[BlockSize];
};

struct PT180_DvbCTBlock
{
  dword BlockSize;
  dword ChannelCount;
  PT180_DvbCtChannel Channels[ChannelCount];
};


struct PT180_SettingsBlock
{
  dword BlockSize;
  byte Data[BlockSize]; 
};

public struct PT180
{
  byte Header[4];  
  PT180_AnalogBlock Analog;
  PT180_FirmwareBlock Firmware;
  PT180_DvbCTBlock DvbCT;
  PT180_SettingsBlock Settings;
};
