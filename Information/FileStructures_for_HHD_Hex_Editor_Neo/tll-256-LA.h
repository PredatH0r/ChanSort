#include "tll-common.h"

#define MAX_SAT_COUNT 64
#define MAX_LNB_COUNT 40
#define MAX_DVBS_COUNT 7520
#define MAX_TP_COUNT 2400

struct LA256_AnalogChannel
{
  byte t1[8];
  TLL_SignalSource SignalSource;
  byte t1b;
  word ChannelTransponder1;
  word ProgramNr;
  word LogicalProgramNr;
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
  byte LockSkipHide;
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

struct LA256_AnalogBlock
{
  dword BlockSize; 
  dword ChannelCount;
  LA256_AnalogChannel Channels[ChannelCount];
};

struct LA256_HotelSettings
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

struct LA256_FirmwareBlock
{
  dword BlockSize;
  byte u[38251];
  LA256_HotelSettings HotelSettings;  
  byte Data[BlockSize - 38251 - sizeof(LA256_HotelSettings)];
};

struct LA256_DvbCtChannel
{
  byte t1[8];
  TLL_SignalSource SignalSource;
  byte t1b;
  word ChannelTransponder1;
  word ProgramNr;
  word LogicalChannelNumber;
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
  byte t5a[38];
  word ChannelTransponder2;
  dword Frequency;
  byte t6[6];
  word ONID;
  word TSID;
  byte t7[32];
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
  byte t10[12];
  word PcrPid2;
  word APID2;
  word u1;
  word u2;
  byte t11[12];
};

struct LA256_DvbCTBlock
{
  dword BlockSize;
  dword ChannelCount;
  LA256_DvbCtChannel Channels[ChannelCount];
};

struct LA256_DvbsHeaderSubblock
{
  dword Crc32;
  byte DVBS_S2_Tag[8];
  word Temp03[2];
};

struct LA256_Satellite
{
  char Name[32]; 
  byte PosDeg; 
  byte PosCDeg; 
  byte LnbIndex;
  byte FactoryDefault;
  word TransponderStartIndex;
  word TransponderEndIndex;
  word TransponderCount;
  word Unknown4;
  word Unknown5;
  word Unknown6;
};

struct LA256_DvbsSatelliteSubblock
{
  dword Crc32;
  word MagicNo;
  byte SatAllocationBitmap[MAX_SAT_COUNT/8];
  word Reserved;
  word SatCount;
  byte SatOrder[MAX_SAT_COUNT];
  word Unknown3;
  LA256_Satellite Satellites[MAX_SAT_COUNT];
};

struct LA256_Transponder
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

struct LA256_DvbsTransponderSubblock
{
  dword Crc32;
  word Unknown1;
  word Unknown2;
  word Unknown3;
  word Unknown4;
  word TransponderCount;
  byte AllocationBitmap[MAX_TP_COUNT/8];
  struct LA256_DvbsTransponderTable1
  {
    word Prev;
    word Next;
    word Current;
  } TransponderTable1[MAX_TP_COUNT];
  word Unknown5;
  LA256_Transponder Transponder[MAX_TP_COUNT];  
};

struct LA256_SatChannel
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
  byte LockSkipHide;   
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

struct LA256_DvbsChannelSubblock
{
  dword Crc32; 
  word Unknown[2];
  word LinkedListStartIndex;
  word LinkedListEndIndex1;
  word LinkedListEndIndex2;
  word ChannelCount;
  byte AllocationBitmap[MAX_DVBS_COUNT/8];
  struct LA256_LinkedChannelList
  {
    word Prev;
    word Next;
    word Current;
    word Zero;
  } LinkedList[MAX_DVBS_COUNT];
  LA256_SatChannel Channels[MAX_DVBS_COUNT];
};

struct LA256_Lnb
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

struct LA256_DvbsLnbSubblock
{
  dword Crc32;
  word Unknown1;
  byte AllocationBitmap[5];
  byte Unknown2;
  LA256_Lnb Lnb[MAX_LNB_COUNT];
};

struct LA256_DvbSBlock
{
  dword BlockSize;
  LA256_DvbsHeaderSubblock HeaderBlock;
  LA256_DvbsSatelliteSubblock SatelliteBlock;
  LA256_DvbsTransponderSubblock TransponderBlock;
  LA256_DvbsChannelSubblock ChannelBlock;
  LA256_DvbsLnbSubblock LnbBlock;
};

struct LA256_SettingsBlock
{
  dword BlockSize;
  byte Data[BlockSize]; 
};

public struct LA256
{
  byte Header[4]; 
  
  LA256_AnalogBlock Analog;
  LA256_FirmwareBlock Firmware;
  LA256_DvbCTBlock DvbCT;
  LA256_DvbSBlock DvbS;
  LA256_SettingsBlock Settings;
};
