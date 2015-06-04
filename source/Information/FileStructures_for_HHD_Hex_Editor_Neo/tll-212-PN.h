#include "tll-common.h"

#define MAX_SAT_COUNT 64
struct TLL44_Satellite;
typedef TLL44_Satellite TLL_Satellite;

#define MAX_TP_COUNT 2400
struct TLL40_Transponder;
typedef TLL40_Transponder TLL_Transponder;

#define DVBS_CHANNELLIST_PREFIXSIZE 2

#define MAX_DVBS_COUNT 6000
struct TLL76_SatChannel;
typedef TLL76_SatChannel TLL_SatChannel;

#define MAX_LNB_COUNT 40
struct TLL48_Lnb;
typedef TLL48_Lnb TLL_Lnb;

#include "tll-satellite.h"

struct PN212_AnalogChannel
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
  byte u1[13869];
  PN212_HotelSettings HotelSettings;  
  byte Data[BlockSize - sizeof(HotelSettings) - 13869];
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
  word LogicalProgramNr1;
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
  word LogicalProgramNr2;
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

struct TLL76_SatChannel
{
  word LnbConfigIndex;
  word u2;
  byte SourceType;
  byte u3;
  word TP_Number; 
  word CH_Number; 
  word CH_NumberFixed;
  word TP_Number2;
  byte FavCrypt;
  byte LockSkipHide;   
  word SID;       
  byte ServiceType;
  byte CH_NameLength; 
  char CH_Name[40];
  word VPID; 
  word APID; 
  word APID2;  
  word XPID;
  byte t6[8];
};

struct TLL48_Lnb
{
  byte SettingsID; 
  byte t2[3];
  byte SatelliteID;
  byte t3[3];
  char FrequenceName[12]; 
  word LOF1; 
  byte t4[2]; 
  word LOF2; 
  byte t5[22]; 
};

public struct PN212
{
  byte Header[4]; 
  
  PN212_AnalogBlock Analog;
  PN212_FirmwareBlock Firmware;
  PN212_DvbCTBlock DvbCT;
  TLL_DvbSBlock DvbS;
  TLL_SettingsBlock Settings;
};
