#include "tll-common.h"

#define MAX_SAT_COUNT 64
struct TLL44_Satellite;
typedef TLL44_Satellite TLL_Satellite;

#define MAX_TP_COUNT 2400
struct TLL48_Transponder;
typedef TLL48_Transponder TLL_Transponder;

#define MAX_DVBS_COUNT 6000
struct TLL76_SatChannel;
typedef TLL76_SatChannel TLL_SatChannel;

#define MAX_LNB_COUNT 40
struct TLL48_Lnb;
typedef TLL48_Lnb TLL_Lnb;

#define DVBS_CHANNELLIST_PREFIXSIZE 2

#include "tll-satellite.h"

struct LB244_AnalogChannel
{
  byte t1[8];
  TLL_SignalSource SignalSource;
  byte t1b;
  word ChannelTransponder1;
  word ProgramNr;
  word LogicalProgramNr1;
  byte t2[4];
  byte Favorites1;
  byte t2b[3];
  word Frequency1Div50;
  word APID1;
  byte ChannelNumberInBand;
  byte ChannelBand;
  byte t3[18];
  char CH_Name1[40];
  byte CH_NameLength1;
  byte t4;
  word SID1;
  byte t5a[45];
  byte na_NitVersion;
  word t5b;
  word ChannelTransponder2;
  word t5c;
  dword FrequencyDiv50;
  byte t6[6];
  word ONID;
  word TSID;
  byte t7[19];
  word ChannelTransponder3;
  byte t7b;
  word ProgramNr2;
  word LogicalProgramNr2;
  word ChannelTransponder4;
  byte Favorites2;
  TLL_EditFlags EditFlags;
  word SID2;
  byte ServiceType; 
  byte CH_NameLength2;
  char CH_Name2[40];
  byte t10[0];
  word Frequency2Div50;
  word APID2;
  word u1;
  word u2;
  byte t11[8];
};

struct LB244_AnalogBlock
{
  dword BlockSize; 
  dword ChannelCount;
  LB244_AnalogChannel Channels[ChannelCount];
};

struct LB244_HotelSettings
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
  byte Unknown1;
  byte DtvChannelUpdate;
  byte PowerOnDefault;
  byte InputSource;
  word Programme;
  byte Unknown2;
  byte Volume;
  byte AvSettings;
  byte RadioVideoBlank;
  byte unknown3;
  byte StartProgNr;
  byte unknown4;
  byte NumberOfPrograms;
  byte RadioNameDisplay;
  byte unknown5[2];
  byte AccessCode[4];
};

struct LB244_FirmwareBlock
{
  dword BlockSize;
  byte u[17808];
  // LB244_HotelSettings HotelSettings;  
  // byte Data[BlockSize - 38251 - sizeof(LB244_HotelSettings)];
};

struct LB244_DvbCtChannel
{
  byte t1[8];
  TLL_SignalSource SignalSource;
  byte t1b;
  word ChannelTransponder1;
  word ProgramNr;
  word LogicalProgramNr1;
  byte t2a[4];
  byte Fav1;
  byte t2b[3];
  word PcrPid1;
  word APID1;
  byte t2c[8];
  word VPID1;
  byte t3[6];
  char CH_Name1[40];
  byte CH_NameLength1;
  byte t4;
  word SID1;
  byte t5a[45];
  byte NitVersion;
  word t5b;
  word ChannelTransponder2;
  word t5c;
  dword Frequency;
  byte t6[6];
  word ONID;
  word TSID;
  word NID;
  dword SpecialData;
  byte t7[13];
  word ChannelTransponder3;
  byte t7b;
  word ProgramNr2;
  word LogicalProgramNr2;
  word ChannelTransponder4;
  byte Favorites2;
  TLL_EditFlags EditFlags;
  word SID2;
  byte ServiceType; 
  byte CH_NameLength2;
  char CH_Name2[40];
  byte t10[0];
  word PcrPid2;
  word APID2;
  word u1;
  word u2;
  byte t11[8];
};

struct LB244_DvbCTBlock
{
  dword BlockSize;
  dword ChannelCount;
  LB244_DvbCtChannel Channels[ChannelCount];
};

struct TLL48_Transponder
{
  word FirstChannelIndex;
  word LastChannelIndex;
  word ChannelCount;
  byte t1[6];
  word TP_Number;
  byte t1b[2];
  word TP_Freq;
  byte t2[8]; 
  word NID; 
  word TID; 
  byte t3[3];
  word SRateTimes2;
  byte t4[9]; 
  byte SatIndexTimes2; 
  byte t5[3];
  byte u40[0];
};

struct TLL76_SatChannel
{
  word LnbConfigIndex;
  word u2;
  byte SourceType;
  word TP_Number;
  byte u3;
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


public struct LB550U
{
  byte Header[4]; 
  
  LB244_AnalogBlock Analog;
  LB244_FirmwareBlock Firmware;
  LB244_DvbCTBlock DvbCT;
  TLL_DvbSBlock DvbS;
  TLL_SettingsBlock Settings;
};
