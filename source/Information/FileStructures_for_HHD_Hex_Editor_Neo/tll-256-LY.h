#include "tll-common.h"

#define MAX_SAT_COUNT 64
struct TLL48_Satellite;
typedef TLL48_Satellite TLL_Satellite;

#define MAX_TP_COUNT 2400
struct TLL56_Transponder;
typedef TLL56_Transponder TLL_Transponder;

#define MAX_DVBS_COUNT 7520
struct TLL92_SatChannel;
typedef TLL92_SatChannel TLL_SatChannel;

#define MAX_LNB_COUNT 40
struct TLL52_Lnb;
typedef TLL52_Lnb TLL_Lnb;

#define DVBS_CHANNELLIST_PREFIXSIZE 0

#include "tll-satellite.h"

struct LY256_Channel
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
  word Frequency1Div50__PcrPid1;
  word APID1;
  word xPID;
  byte t3[6];
  word ChannelNumber_Band__VPID1;
  byte t3b[6];
  char CH_Name1[40];
  word CH_NameLength1;
  word SID1;
  byte t5a[47];
  byte NitVersion;
  word ChannelTransponder2;
  word t5b;
  dword Frequency;
  byte t6[6];
  word ONID;
  word TSID;
  byte t7[23];
  word ChannelTransponder3;
  byte t8;
  word ProgramNr2;
  word LogicalProgramNr2;
  word ChannelTransponder4;
  byte t9[2];
  byte Favorites2;
  TLL_EditFlags EditFlags;
  byte t10[2];
  word SID2;
  byte ServiceType; 
  byte CH_NameLength2;
  char CH_Name2[40];
  word FrequencyDiv50__PcrPid2;
  word APID2;
  word u1;
  word u2;
  byte t11[12];
};

struct LY256_AnalogBlock
{
  dword BlockSize; 
  dword ChannelCount;
  LY256_Channel Channels[ChannelCount];
};

struct LY256_HotelSettings
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

struct LY256_FirmwareBlock
{
  dword BlockSize;
  byte Data[BlockSize];
};

struct LY256_HospitalityBlock
{
  dword BlockSize;
  byte Data[BlockSize];
};

struct LY256_DvbCTBlock
{
  dword BlockSize;
  dword ChannelCount;
  LY256_Channel Channels[ChannelCount];
};

struct TLL48_Satellite
{
  char Name[32]; 
  byte PosDeg; 
  byte PosCDeg; 
  byte LnbIndex;
  byte FactoryDefault;
  word TransponderHead;
  word TransponderTail;
  word TransponderCount;
  word Unknown4;
  word Unknown5;
  word Unknown6;
};

struct TLL56_Transponder
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
  byte u40[12];
};

struct TLL92_SatChannel
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
  byte t5[12];
};

struct TLL52_Lnb
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
  byte t5[22]; 
};


public struct LY256
{
  byte Header[4]; 
  
  LY256_AnalogBlock Analog;
  LY256_FirmwareBlock Firmware;
  LY256_HospitalityBlock Hospitality;
  LY256_DvbCTBlock DvbCT;
  TLL_DvbSBlock DvbS;
  TLL_SettingsBlock Settings;
};
