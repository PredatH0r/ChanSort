#include "tll-common.h"

#define MAX_SAT_COUNT 64
struct TLL44_Satellite;
typedef TLL44_Satellite TLL_Satellite;

#define MAX_TP_COUNT 2400
struct TLL48_Transponder;
typedef TLL48_Transponder TLL_Transponder;

#define MAX_DVBS_COUNT 6000
struct TLL84_SatChannel;
typedef TLL84_SatChannel TLL_SatChannel;

#define MAX_LNB_COUNT 40
struct TLL48_Lnb;
typedef TLL48_Lnb TLL_Lnb;

#define DVBS_CHANNELLIST_PREFIXSIZE 2

#include "tll-satellite.h"

struct LF252_AnalogChannel
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

struct LF252_AnalogBlock
{
  dword BlockSize; 
  dword ChannelCount;
  LF252_AnalogChannel Channels[ChannelCount];
};

struct LF252_FirmwareBlock
{
  dword BlockSize;
  byte u[BlockSize];
};

struct LF252_DvbCtChannel
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
  TLL_DvbID PcrPid1;
  TLL_DvbID APID1;
  byte t2c[8];
  TLL_DvbID VPID1;
  byte t3[6];
  char CH_Name1[40];
  byte CH_NameLength1;
  byte t4;
  word SID1;
  byte t5a[47];
  byte NitVersion;
  word ChannelTransponder2;
  byte t5b[2];
  dword Frequency;
  byte t6[4];
  word ONID;
  word TSID;
  word NID;
  dword SpecialData;
  byte t7[15];
  word ChannelTransponder3;
  byte t7b;
  word ProgramNr2;
  word LogicalProgramNr2;
  word ChannelTransponder4;
  byte Favorites2;
  TLL_EditFlags EditFlags;
  byte t7c[2];
  word SID2;
  byte ServiceType; 
  byte CH_NameLength2;
  char CH_Name2[40];
  //byte t10[12];
  TLL_DvbID PcrPid2;
  TLL_DvbID APID2;
  word u1;
  word u2;
  byte t11[14];
};

struct LF252_DvbCTBlock
{
  dword BlockSize;
  dword ChannelCount;
  LF252_DvbCtChannel Channels[ChannelCount];
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

struct TLL84_SatChannel
{
  word LnbIndex;
  word t1;
  TLL_SignalSource SignalSource;
  //byte t2;
  word TP_Number; 
  byte t2;
  TLL_DvbID CH_Number; 
  word CH_NumberFixed;
  word TP_Number2;
  byte FavCrypt;
  TLL_EditFlags EditFlags;   
  byte t3[2];
  word SID;       
  byte ServiceType;
  byte CH_NameLength; 
  char CH_Name[40];
  TLL_DvbID VPID;
  TLL_DvbID APID;
  byte t3[18];
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


public struct LF252
{
  byte Header[4]; 
  
  LF252_AnalogBlock Analog;
  LF252_FirmwareBlock Firmware;
  LF252_DvbCTBlock DvbCT;
  TLL_DvbSBlock DvbS;
  TLL_SettingsBlock Settings;
};
