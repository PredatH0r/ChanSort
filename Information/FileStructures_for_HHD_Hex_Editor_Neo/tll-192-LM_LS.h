// all LM models except 340S and 611S

#include "tll-common.h"

#define MAX_SAT_COUNT 64
struct TLL44_Satellite;
typedef TLL44_Satellite TLL_Satellite;

#define MAX_TP_COUNT 2400
struct TLL40_Transponder;
typedef TLL40_Transponder TLL_Transponder;

#define MAX_DVBS_COUNT 7520
struct TLL72_SatChannel;
typedef TLL72_SatChannel TLL_SatChannel;

#define MAX_LNB_COUNT 40
struct TLL44_Lnb;
typedef TLL44_Lnb TLL_Lnb;

#define DVBS_CHANNELLIST_PREFIXSIZE 0

#include "tll-satellite.h"


struct LM192_AnalogChannel
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
  byte t3[2];
  char CH_Name1[40];
  byte CH_NameLength1;
  byte t4;
  word SID1;
  byte t5a[20];
  word ChannelTransponder2;
  dword Frequency;
  byte t6[2];
  word ONID;
  word TSID;
  byte t7[20];
  word ChannelTransponder3;
  word ProgramNr2;
  byte t8b[2];
  word ChannelTransponder4;
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

struct LM192_AnalogBlock
{
  dword BlockSize; 
  dword ChannelCount;
  LM192_AnalogChannel Channels[ChannelCount];
};

struct LM192_FirmwareBlock
{
  dword BlockSize;
  byte u1[167];
  byte SystemLock;
  byte u2;
  byte TvPassword[4];
  byte u3[34919];
  byte HbbTvEnable;
  byte u4[538];
  TLL_HotelSettings HotelSettings;  
  byte u7[42];
  byte HotelMenuPin[4];
  byte u8[834];
  byte SettingsAutoChannelUpdate; // 0x8EBC
  byte Data[BlockSize - 0x8ebc - 1];
};

struct LM192_DvbCtChannel
{
  byte t1[8];
  TLL_SignalSource SignalSource;
  byte t1b;
  word ChannelTransponder1;
  word ProgramNr;
  word LogicalChannelNumber;
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
  byte t5a[20];
  word ChannelTransponder2;
  dword Frequency;
  byte t6[2];
  word ONID;
  word TSID;
  byte t7[20];
  word ChannelTransponder3;
  word ProgramNr2;
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
  byte t11[8];
};

struct LM192_DvbCTBlock
{
  dword BlockSize;
  dword ChannelCount;
  LM192_DvbCtChannel Channels[ChannelCount];
};



struct LM192_SettingsBlock
{
  dword BlockSize;
  byte Data[BlockSize]; 
};

public struct LM192
{
  byte Header[4]; 
  
  LM192_AnalogBlock Analog;
  LM192_FirmwareBlock Firmware;
  LM192_DvbCTBlock DvbCT;
  TLL_DvbSBlock DvbS;
  TLL_SettingsBlock Settings;
};
