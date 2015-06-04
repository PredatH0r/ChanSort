#include "tll-common.h"

// LM340S and LM611S

#define MAX_SAT_COUNT 64
#define MAX_LNB_COUNT 40
#define MAX_DVBS_COUNT 7520
#define MAX_TP_COUNT 2400

struct LM188_AnalogChannel
{
  byte t1[8];
  TLL_SignalSource SignalSource;
  byte t1b;
  word ChannelTransponder1;  
  word ProgramNr;
  word LogicalProgramNr1;
  word t2[2];
  byte t2b;
  byte Favorites1;
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
  word NID;
  byte t7[17];
  word ChannelTransponder3;
  byte t8;
  word ProgramNr2;
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

struct LM188_DvbCtChannel
{
  byte t1[8];
  TLL_SignalSource SignalSource;
  byte t1b;
  word ChannelTransponder1;
  word ProgramNr;
  word LogicalProgramNr1;
  word t2[2];
  byte t2b;
  byte Favorites1;
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
  word NID;
  dword SpecialData;
  byte t7[13];
  word ChannelTransponder3;
  byte t8;
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
  byte t11[4];
};


struct LM188_AnalogBlock
{
  dword BlockSize; 
  dword ChannelCount;
  LM188_AnalogChannel Channels[ChannelCount];
};

struct LM188_FirmwareBlock
{
  dword BlockSize;
  byte Data[BlockSize];
};

struct LM188_DvbCTBlock
{
  dword BlockSize;
  dword ChannelCount;
  LM188_DvbCtChannel Channels[ChannelCount];
};


struct LM188_DvbsHeaderSubblock
{
  dword Crc32;
  byte DVBS_S2_Tag[8];
  word Temp03[2];
};

struct LM188_Satellite
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

struct LM188_DvbsSatelliteSubblock
{
  dword Crc32;
  word Unknown1;
  byte SatAllocationBitmap[MAX_SAT_COUNT/8];
  word Unknown2;
  word SatCount;
  byte SatOrder[MAX_SAT_COUNT];
  word Unknown3;
  LM188_Satellite Satellites[MAX_SAT_COUNT];
};

struct LM188_Transponder
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

struct LM188_DvbsTransponderSubblock
{
  dword Crc32;
  word Unknown1;
  word Unknown2;
  word Unknown3;
  word Unknown4;
  word TransponderCount;
  byte AllocationBitmap[MAX_TP_COUNT/8];
  struct LM188_DvbsTransponderTable1
  {
    word Prev;
    word Next;
    word Current;
  } TransponderTable1[MAX_TP_COUNT];
  word Unknown5;
  LM188_Transponder Transponder[MAX_TP_COUNT];  
};


struct LM188_SatChannel
{
  word LnbIndex;
  word t2;
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

struct LM188_DvbsChannelSubblock
{
  dword Crc32; 
  word Unknown[2];
  word LinkedListStartIndex;
  word LinkedListEndIndex1;
  word LinkedListEndIndex2;
  word ChannelCount;
  byte AllocationBitmap[MAX_DVBS_COUNT/8];
  struct LM188_LinkedChannelList
  {
    word Prev;
    word Next;
    word Current;
    word Zero;
  } LinkedList[MAX_DVBS_COUNT];
  LM188_SatChannel Channels[MAX_DVBS_COUNT];
};

struct LM188_Lnb
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

struct LM188_DvbsLnbSubblock
{
  dword Crc32;
  word Unknown1;
  byte AllocationBitmap[5];
  byte Unknown2;
  LM188_Lnb Lnb[MAX_LNB_COUNT];
};

struct LM188_DvbSBlock
{
  dword BlockSize;
  LM188_DvbsHeaderSubblock HeaderBlock;
  LM188_DvbsSatelliteSubblock SatelliteBlock;
  LM188_DvbsTransponderSubblock TransponderBlock;
  LM188_DvbsChannelSubblock ChannelBlock;
  LM188_DvbsLnbSubblock LnbBlock;
};


struct LM188_SettingsBlock
{
  dword BlockSize;
  byte Data[BlockSize]; 
};

public struct LM188
{
  byte Header[4]; 
  
  LM188_AnalogBlock Analog;
  LM188_FirmwareBlock Firmware;
  LM188_DvbCTBlock DvbCT;
  LM188_DvbSBlock DvbS;
  LM188_SettingsBlock Settings;
};
