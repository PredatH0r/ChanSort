#include "tll-common.h"

struct LT212_AnalogChannel
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
  word Frequency1Div50;
  word APID1;
  byte ChannelNumberInBand;
  byte ChannelBand;
  byte t3[4];
  char CH_Name1[40];
  byte CH_NameLength1;
  byte t4;
  word SID1;
  byte t5a[30];
  word ChannelTransponder2;
  dword Frequency;
  byte t6[2];
  word ONID;
  word TSID;
  byte t7[23];
  word ChannelTransponder3;
  byte t7b;
  word ProgramNr2;
  word LogicalProgramNr2;
  word ChannelTransponder4;
  byte t8b[4];
  byte Favorites2;
  byte LockSkipHide;
  word SID2;
  byte ServiceType; 
  byte CH_NameLength2;
  char CH_Name2[40];
  word Frequency2Div50;
  word APID2;
  byte t11[8];
};

struct LT212_AnalogBlock
{
  dword BlockSize; 
  dword ChannelCount;
  LT212_AnalogChannel Channels[ChannelCount];
};

struct LT212_HotelSettings
{
  byte HotelModeActive;
  byte PowerOnStatus;
  byte SetupMenuDisplay;
  byte ProgramChange;
  byte InputSourceChange;
  byte MenuDisplay;
  byte OsdDisplay;
  byte LgIrOperation;
  byte LocalKeyOp;
  byte MaxVolume;
  byte DtvChannelUpdate;
  byte PowerOnDefault;
  byte InputSource;
  word Programme;
  byte Volume;
  byte AvSettings;
  byte RadioVideoBlank;
  byte StartProgNr;
  byte NumberOfPrograms;
  byte RadioNameDisplay;
  byte unknown3[2];
  byte AccessCode[4];
};

struct LT212_FirmwareBlock
{
  dword BlockSize;
  byte u1[12635];
  LT212_HotelSettings HotelSettings;  
  byte Data[BlockSize - sizeof(HotelSettings) - 12635];
};

struct LT212_UnknownBlock
{
  dword BlockSize;
  byte Data[BlockSize];
};

struct LT212_DvbCtChannel
{
  byte t1[8];
  TLL_SignalSource SignalSource;
  byte t1b;
  word ChannelTransponder1;
  word ProgramNr;
  word LogicalChannelNr1;
  word t2[2];
  byte Favorites1;
  byte t2d;
  word Frequency1Div50;
  word APID1;
  byte ChannelNumberInBand;
  byte ChannelBand;
  byte t3[4];
  char CH_Name1[40];
  byte CH_NameLength1;
  byte t4;
  word SID1;
  byte t5a[30];
  word ChannelTransponder2;
  dword Frequency;
  byte t6[2];
  word ONID;
  word TSID;
  byte t7[23];
  word ChannelTransponder3;
  byte t7b;
  word ProgramNr2;
  word LogicalProgramNr2;
  word ChannelTransponder4;
  byte t8b[4];
  byte Favorites2;
  byte LockSkipHide;
  word SID2;
  byte ServiceType; 
  byte CH_NameLength2;
  char CH_Name2[40];
  word Frequency2Div50;
  word APID2;
  byte t11[8];
};

struct LT212_DvbCTBlock
{
  dword BlockSize;
  dword ChannelCount;
  LT212_DvbCtChannel Channels[ChannelCount];
};

struct LT212_SettingsBlock
{
  dword BlockSize;
  byte Data[BlockSize]; 
};

public struct LT212
{
  byte Header[4]; 
  
  LT212_AnalogBlock Analog;
  LT212_FirmwareBlock Firmware;
  LT212_UnknownBlock Unknown;
  LT212_DvbCTBlock DvbCT;
  LT212_SettingsBlock Settings;
};
