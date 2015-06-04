#include "tll-common.h"

struct LD176_AnalogChannel
{
  byte t1[8];
  TLL_SignalSource SignalSource;
  byte t1b;
  word ChannelTransponder1;
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
  byte t5[10]; // !
  word t5b;
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
  word t10[3];
};


struct LD176_DvbCtChannel
{
  byte t1[8];
  TLL_SignalSource SignalSource;
  byte t1b;
  word ChannelTransponder1;
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
  byte t5a[12]; // !
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
  word t11;
  word t12;
};

struct LD176_AnalogBlock
{
  dword BlockSize; 
  dword ChannelCount;
  LD176_AnalogChannel Channels[ChannelCount];
};

struct LD176_FirmwareBlock
{
  dword BlockSize;
  byte Data[BlockSize];
};

struct LD176_DvbCTBlock
{
  dword BlockSize;
  dword ChannelCount;
  LD176_DvbCtChannel Channels[ChannelCount];
};

struct LD176_SettingsBlock
{
  dword BlockSize;
  byte Data[BlockSize]; 
};

public struct LD176
{
  byte Header[4]; 
  
  LD176_AnalogBlock Analog;
  LD176_FirmwareBlock Firmware;
  LD176_DvbCTBlock DvbCT;
  LD176_SettingsBlock Settings;
};
