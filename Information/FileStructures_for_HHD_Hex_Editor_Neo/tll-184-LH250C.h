#include "tll-common.h"

#define MAX_SAT_COUNT 64
#define MAX_LNB_COUNT 40
#define MAX_DVBS_COUNT 7520
#define MAX_TP_COUNT 2400

struct LH184_AnalogChannel
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
  byte t3[4];
  char CH_Name1[40];
  byte CH_NameLength1;
  byte t4[1];
  word SID1;
  byte t5a[14];
  word ChannelTransponder2;
  dword Frequency;
  byte t6[2];
  word ONID;
  word TSID;
  byte t7[22];
  word ProgramNr2Times4;
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


struct LH184_DvbCtChannel
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
  byte t3[4];
  char CH_Name1[40];
  byte CH_NameLength1;
  byte t4;
  word SID1;
  byte t5a[14];
  word ChannelTransponder2;
  dword Frequency;
  byte t6[2];
  word ONID;
  word TSID;
  byte t7[22];
  word ProgramNr2Times4;
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

struct LH184_AnalogBlock
{
  dword BlockSize; 
  dword ChannelCount;
  LH184_AnalogChannel Channels[ChannelCount];
};

struct LH184_FirmwareBlock
{
  dword BlockSize;
  byte Data[BlockSize];
};

struct LH184_DvbCTBlock
{
  dword BlockSize;
  dword ChannelCount;
  LH184_DvbCtChannel Channels[ChannelCount];
};

struct LH184_DvbsHeaderSubblock
{
  dword Crc32;
  byte DVBS_S2_Tag[8];
  word Temp03[2];
};

struct LH184_Satellite
{
  char Name[32]; 
  byte PosDeg; 
  byte PosCDeg; 
  word Unknown1;
  word Unknown2;
  word Unknown3;
  word TransponderCount;
  word Unknown4;
};

struct LH184_DvbsSatelliteSubblock
{
  dword Crc32;
  word Unknown1;
  byte SatAllocationBitmap[MAX_SAT_COUNT/8];
  word Unknown2;
  word SatCount;
  byte SatOrder[MAX_SAT_COUNT];
  word Unknown3;
  LH184_Satellite Satellites[MAX_SAT_COUNT];
};

struct LH184_Transponder
{
  byte t1[10];
  word TP_Number;
  word TP_Freq;
  byte t2[4]; 
  word NID; 
  word TID; 
  byte t3[3];
  word SRate;
  byte t4[9]; 
  byte SatIndexTimes2; 
  byte t5[3]; 
};

struct LH184_DvbsTransponderSubblock
{
  dword Crc32;
  word Unknown1;
  word Unknown2;
  word Unknown3;
  word Unknown4;
  word TransponderCount;
  byte AllocationBitmap[MAX_TP_COUNT/8];
  struct LH184_DvbsTransponderTable1
  {
    word Prev;
    word Next;
    word Current;
  } TransponderTable1[MAX_TP_COUNT];
  word Unknown5;
  LH184_Transponder Transponder[MAX_TP_COUNT];  
};

struct LH184_SatChannel
{
  word LnbIndex;  
  byte t2[2];
  TLL_SignalSource SignalSource;
  word TP_Number; 
  byte t2b;
  word CH_Number; 
  word CH_NumberFixed;
  word TP_Number2;
  byte t3b;
  byte EditFlag;   
  word SID;       
  byte ServiceType;
  byte CH_NameLength; 
  char CH_Name[40];
  word VID; 
  word AID; 
  word t4;  
  byte t5[2];
};

struct LH184_DvbsChannelSubblock
{
  dword Crc32; 
  word Unknown[2];
  word LinkedListStartIndex;
  word LinkedListEndIndex1;
  word LinkedListEndIndex2;
  word ChannelCount;
  byte AllocationBitmap[MAX_DVBS_COUNT/8];
  struct LH184_LinkedChannelList
  {
    word Prev;
    word Next;
    word Current;
    word Zero;
  } LinkedList[MAX_DVBS_COUNT];
  LH184_SatChannel Channels[MAX_DVBS_COUNT];
};

struct LH184_Lnb
{
  byte SettingsID; 
  byte t2[3];
  byte SatelliteID;
  byte t3[3];
  char FrequenceName[12]; 
  word LOF1; 
  byte t4[2]; 
  word LOF2; 
  byte t5[18]; 
};

struct LH184_DvbsLnbSubblock
{
  dword Crc32;
  word Unknown1;
  byte AllocationBitmap[5];
  byte Unknown2;
  LH184_Lnb Lnb[MAX_LNB_COUNT];
};

struct LH184_DvbSBlock
{
  dword BlockSize;
  LH184_DvbsHeaderSubblock HeaderBlock;
  LH184_DvbsSatelliteSubblock SatelliteBlock;
  LH184_DvbsTransponderSubblock TransponderBlock;
  LH184_DvbsChannelSubblock ChannelBlock;
  LH184_DvbsLnbSubblock LnbBlock;
};

struct LH184_SettingsBlock
{
  dword BlockSize;
  byte Data[BlockSize]; 
};

public struct LH184
{
  byte Header[4];  
  LH184_AnalogBlock Analog;
  LH184_FirmwareBlock Firmware;
  LH184_DvbCTBlock DvbCT;
  LH184_SettingsBlock Settings;
};
