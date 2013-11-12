#include "tll-common.h"

#define MAX_SAT_COUNT 64
#define MAX_LNB_COUNT 40
#define MAX_DVBS_COUNT 6000
#define MAX_TP_COUNT 2400

struct LN224_AnalogChannel
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
  word Frequency2Div50;
  word APID2;
  byte t11[8];
};

struct LN224_AnalogBlock
{
  dword BlockSize; 
  dword ChannelCount;
  LN224_AnalogChannel Channels[ChannelCount];
};

struct LN224_HotelSettings
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

struct LN224_FirmwareBlock
{
  dword BlockSize;
  byte u1[13623];
  LN224_HotelSettings HotelSettings;  
  byte Data[BlockSize - sizeof(HotelSettings) - 13623];
};

struct LN224_DvbCtChannel
{
  byte t1[8];
  TLL_SignalSource SignalSource;
  byte t1b;
  word ChannelTransponder1;
  word ProgramNr;
  word LogicalProgramNr1;
  word t2[2];
  byte Favorites1;
  byte t2b[3];
  word PcrPid;
  word APID1;
  byte t2c[8];
  word VPID1;
  byte t3[6];
  char CH_Name1[40];
  byte CH_NameLength1;
  byte t4;
  word SID1;
  byte t5a[30];
  word ChannelTransponder2;
  dword Frequency;
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
  byte LockSkipHide;
  word SID2;
  byte ServiceType; 
  byte CH_NameLength2;
  char CH_Name2[40];
  word PcrPid2;
  word APID2;
  word XPID;
  word YPID;
  byte t11[8];
};

struct LN224_DvbCTBlock
{
  dword BlockSize;
  dword ChannelCount;
  LN224_DvbCtChannel Channels[ChannelCount];
};

struct LN224_DvbsHeaderSubblock
{
  dword Crc32;
  byte DVBS_S2_Tag[8];
  word Temp03[2];
};

struct LN224_Satellite
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

struct LN224_DvbsSatelliteSubblock
{
  dword Crc32;
  word Unknown1;
  byte SatAllocationBitmap[MAX_SAT_COUNT/8];
  word Unknown2;
  word SatCount;
  byte SatOrder[MAX_SAT_COUNT];
  word Unknown3;
  LN224_Satellite Satellites[MAX_SAT_COUNT];
};

struct LN224_Transponder
{
  word FirstChannelIndex;
  word LastChannelIndex;
  word ChannelCount;
  byte t1[4];
  word TP_Number;
  word TP_Freq;
  byte t2[8]; 
  word NID; 
  word TID; 
  byte t3[3];
  word SRate;
  byte t4[9]; 
  byte SatIndexTimes2; 
  byte t5[3]; 
};

struct LN224_DvbsTransponderSubblock
{
  dword Crc32;
  word StartIndex;
  word HeadIndex;
  word TailIndex1;
  word TailIndex2;
  word TransponderCount;
  byte AllocationBitmap[MAX_TP_COUNT/8];
  struct LN224_DvbsTransponderTable1
  {
    word Prev;
    word Next;
    word Current;
  } TransponderTable1[MAX_TP_COUNT];
  word Unknown5;
  LN224_Transponder Transponder[MAX_TP_COUNT];  
};

struct LN224_SatChannel
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

struct LN224_DvbsChannelSubblock
{
  dword Crc32; 
  word Unknown[2];
  word LinkedListStartIndex;
  word LinkedListEndIndex1;
  word LinkedListEndIndex2;
  word ChannelCount;
  byte AllocationBitmap[MAX_DVBS_COUNT/8];
  struct LN224_LinkedChannelList
  {
    word Prev;
    word Next;
    word Current;
    word Zero;
  } LinkedList[MAX_DVBS_COUNT];
  word Unknown2;
  LN224_SatChannel Channels[MAX_DVBS_COUNT];
};

struct LN224_Lnb
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

struct LN224_DvbsLnbSubblock
{
  dword Crc32;
  word Unknown1;
  byte AllocationBitmap[5];
  byte Unknown2;
  LN224_Lnb Lnb[MAX_LNB_COUNT];
};

struct LN224_DvbSBlock
{
  dword BlockSize;
  LN224_DvbsHeaderSubblock HeaderBlock;
  LN224_DvbsSatelliteSubblock SatelliteBlock;
  LN224_DvbsTransponderSubblock TransponderBlock;
  LN224_DvbsChannelSubblock ChannelBlock;
  LN224_DvbsLnbSubblock LnbBlock;
};

struct LN224_SettingsBlock
{
  dword BlockSize;
  byte Data[BlockSize]; 
};

public struct LN224
{
  byte Header[4]; 
  
  LN224_AnalogBlock Analog;
  LN224_FirmwareBlock Firmware;
  LN224_DvbCTBlock DvbCT;
  LN224_DvbSBlock DvbS;
  LN224_SettingsBlock Settings;
};
