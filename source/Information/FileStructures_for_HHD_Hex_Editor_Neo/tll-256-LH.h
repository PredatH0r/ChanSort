#include "tll-common.h"

#define MAX_SAT_COUNT 64
struct TLL44_Satellite;
typedef TLL44_Satellite TLL_Satellite;

#define MAX_TP_COUNT 2400
struct TLL48_Transponder;
typedef TLL48_Transponder TLL_Transponder;

#define MAX_DVBS_COUNT 6000
struct TLL80_SatChannel;
typedef TLL80_SatChannel TLL_SatChannel;

#define MAX_LNB_COUNT 40
struct TLL48_Lnb;
typedef TLL48_Lnb TLL_Lnb;

#define DVBS_CHANNELLIST_PREFIXSIZE 2

#include "tll-satellite.h"

struct LH256_AnalogChannel
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
  byte t3[10];
  char CH_Name1[40];
  byte CH_NameLength1;
  byte t4;
  word SID1;
  byte t5a[38];
  word ChannelTransponder2;
  dword FrequencyDiv50;
  byte t6[6];
  word ONID;
  word TSID;
  byte t7[32];
  word ChannelTransponder3;
  word ProgramNr2;
  word LogicalProgramNr2;
  word ChannelTransponder4;
  byte Favorites2;
  TLL_EditFlags EditFlags;
  word SID2;
  byte ServiceType; 
  byte CH_NameLength2;
  char CH_Name2[40];
  byte t10[12];
  word Frequency2Div50;
  word APID2;
  word u1;
  word u2;
  byte t11[12];
};

struct LH256_AnalogBlock
{
  dword BlockSize; 
  dword ChannelCount;
  LH256_AnalogChannel Channels[ChannelCount];
};

struct LH256_FirmwareBlock
{
  dword BlockSize;
  byte u[BlockSize];
};

struct LH256_DvbCtChannel
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
  byte t3[2];
  char CH_Name1[40];
  byte CH_NameLength1;
  byte t4;
  word SID1;
  byte t5a[37];
  byte NitVersion;
  word ChannelTransponder2;
  dword Frequency;
  byte t6[6];
  word ONID;
  word TSID;
  word NID;
  dword SpecialData;
  byte t7[26];
  word ChannelTransponder3;
  word ProgramNr2;
  word LogicalProgramNr2;
  word ChannelTransponder4;
  byte Favorites2;
  TLL_EditFlags EditFlags;
  word SID2;
  byte ServiceType; 
  byte CH_NameLength2;
  char CH_Name2[40];
  byte t10[12];
  word PcrPid2;
  word APID2;
  word u1;
  word u2;
  byte t11[12];
};

struct LH256_DvbCTBlock
{
  dword BlockSize;
  dword ChannelCount;
  LH256_DvbCtChannel Channels[ChannelCount];
};

struct TLL48_Transponder
{
  byte t1[10];
  word TP_Number;
  word TP_Freq;
  byte t2[8]; 
  word NID; 
  word TID; 
  byte t3[3];
  word SRateTimes2;
  byte t4[9]; 
  byte SatIndexTimes2; 
  byte t5[3];
  byte u40[4];
};

struct TLL80_SatChannel
{
  word LnbIndex;
  word t1;
  TLL_SignalSource SignalSource;
  byte t2;
  word TP_Number; 
  word CH_Number; 
  word CH_NumberFixed;
  word TP_Number2;
  byte FavCrypt;
  TLL_EditFlags EditFlags;   
  word SID;       
  byte ServiceType;
  byte CH_NameLength; 
  char CH_Name[52];
  word VPID; 
  word APID; 
  word t3;
  word t4;
};

struct TLL48_Lnb
{
  byte SettingsID; 
  byte t2[3];
  byte SatelliteID;
  byte ScanSearchType;
  byte NetworkSearch;
  byte BlindSearch;
  byte t3[4];
  char FrequencyName[12]; 
  word LOF1; 
  byte t4[2]; 
  word LOF2; 
  byte t5[18]; 
};


public struct LH256
{
  byte Header[4]; 
  
  LH256_AnalogBlock Analog;
  LH256_FirmwareBlock Firmware;
  LH256_DvbCTBlock DvbCT;
  TLL_DvbSBlock DvbS;
  TLL_SettingsBlock Settings;
};
