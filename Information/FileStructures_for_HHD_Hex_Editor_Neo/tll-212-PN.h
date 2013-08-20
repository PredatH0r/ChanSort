#include "tll-common.h"

struct PN212_AnalogChannel
{
  byte t1[8];
  TLL_SignalSource SignalSource;
  byte t1b;
  word ChannelTransponder1;
  word ProgramNr;
  word t2[3];
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
  byte t8[2];
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

struct PN212_AnalogBlock
{
  dword BlockSize; 
  dword ChannelCount;
  PN212_AnalogChannel Channels[ChannelCount];
};

struct PN212_HotelSettings
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
//  byte unknown1;
  byte StartProgNr;
//  byte unknown2;
  byte NumberOfPrograms;
  byte RadioNameDisplay;
  byte unknown3[2];
  byte AccessCode[4];
};

struct PN212_FirmwareBlock
{
  dword BlockSize;
  byte u1[13623];
  PN212_HotelSettings HotelSettings;  
  byte Data[BlockSize - sizeof(HotelSettings) - 13623];
};

struct PN212_UnknownBlock
{
  dword BlockSize;
  byte Data[BlockSize];
};

struct PN212_DvbCtChannel
{
  byte t1[8];
  TLL_SignalSource SignalSource;
  byte t1b;
  word ChannelTransponder1;
  word ProgramNr;
  word LogicalChannelNr;
  byte t2[6];
  byte Favorites1;
  byte t2d;
  word PcrPID : 14;
  word Unk : 2;
  word APID1;
  byte ChannelNumberInBand;
  byte ChannelBand;
  byte t3[14];
  char CH_Name1[40];
  byte CH_NameLength1;
  byte t4;
  word SID1;
  byte t5a[21];
  byte NIT_Version;
  word ChannelTransponder2;
  dword Frequency;
  byte t6[2];
  word ONID;
  word TSID;
  word NID;
  byte t7[17];
  word ChannelTransponder3;
  byte t7b;
  word ProgramNr2;
  word LogicalChannelNr2;
  word ChannelTransponder4;
  byte t8b[0];
  byte Favorites2;
  byte LockSkipHide;
  word SID2;
  byte ServiceType; 
  byte CH_NameLength2;
  char CH_Name2[40];
  word PcrPID2 : 14;
  word Unk2 : 2;
  word APID2;
  byte t11[12];
};

struct PN212_DvbCTBlock
{
  dword BlockSize;
  dword ChannelCount;
  PN212_DvbCtChannel Channels[ChannelCount];
};

struct PN212_SettingsBlock
{
  dword BlockSize;
  byte Data[BlockSize]; 
};

public struct PN212
{
  byte Header[4]; 
  
  PN212_AnalogBlock Analog;
  PN212_FirmwareBlock Firmware;
  PN212_DvbCTBlock DvbCT;
  PN212_SettingsBlock Settings;
};
