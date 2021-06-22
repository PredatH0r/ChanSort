//#include "chansort.h"

struct TLL_DvbsHeaderSubblock
{
  dword Crc_32;
  byte DVBS_S2_Tag[8];
  word Temp03[2];
};

struct TLL44_Satellite
{
  char Name[32]; 
  byte PosDeg; 
  byte PosCDeg; 
  byte Unknown1[2];
  word TransponderHead;
  word TransponderTail;
  word TransponderCount;
  word InUse;
};

struct TLL40_Transponder
{
  word FirstChannelIndex;
  word LastChannelIndex;
  word ChannelCount;
  word t1a;
  word t1b;
  word TP_Number;
  word TP_Freq;
  byte t2[4]; 
  word NID; 
  word TID; 
  word t3;
  struct TP_Flags1
  {
    byte Unknown : 6;
	byte IsHorizontal : 1;
  } Flags1;
  word SRate;
  struct TP_Flags2
  {
    enum E_FEC : byte
	{
	  FEC2_3 = 2,
	  FEC3_4 = 3,
	  FEC5_6 = 4,
	  FEC7_8 = 5,
	  FEC9_10 = 9
	} FEC : 4;
	byte S2 : 1;
  } Flags2;

  byte t4[8]; 
  byte SatIndexTimes2; 
  byte t5[3]; 
};

struct TLL_DvbsSatelliteSubblock
{
  dword Crc32;
  word Unknown1;
  byte SatAllocationBitmap[MAX_SAT_COUNT/8];
  word Unknown2;
  word SatCount;
  byte SatOrder[MAX_SAT_COUNT];
  word Unknown3;
  TLL_Satellite Satellites[MAX_SAT_COUNT];
};


struct TLL_DvbsTransponderSubblock
{
  dword Crc32;
  word Unknown1;
  word LinkedListHead;
  word LinkedListTail1;
  word LinkedListTail2;
  word TransponderCount;
  byte AllocationBitmap[MAX_TP_COUNT/8];
  struct TLL_DvbsTransponderLinkedListEntry
  {
    word Prev;
    word Next;
    word Current;
  } LinkedList[MAX_TP_COUNT];
  word Unknown5;
  TLL_Transponder Transponder[MAX_TP_COUNT];  
};


struct TLL72_SatChannel
{
  word LnbIndex;
  word t1;
  TLL_SignalSource SignalSource;
  byte t2;
  word TP_Number; 
  word ChannelNumber; 
  word LogicalChannelNumber;
  word TP_Number2;
  byte FavDelCrypt;
  byte LockSkipHide;   
  word SID;       
  byte ServiceType;
  byte CH_NameLength; 
  char CH_Name[40];
  word VID; 
  word AID; 
  word AID_Times16;  
  byte t6[6];
};

struct TLL_DvbsChannelSubblock
{
  dword Crc32; 
  word Unknown[2];
  word LinkedListHead;
  word LinkedListTail1;
  word LinkedListTail2;
  word ChannelCount;
  byte AllocationBitmap[MAX_DVBS_COUNT/8];
  struct TLL_LinkedChannelListEntry
  {
    word Prev;
    word Next;
    word Current;
    word Zero;
  } LinkedList[MAX_DVBS_COUNT];
  byte Unknown2[DVBS_CHANNELLIST_PREFIXSIZE];
  TLL_SatChannel Channels[MAX_DVBS_COUNT];
};

struct TLL44_Lnb
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

struct TLL_DvbsLnbSubblock
{
  dword Crc32;
  word Unknown1;
  byte AllocationBitmap[5];
  byte Unknown2;
  TLL_Lnb Lnb[MAX_LNB_COUNT];
};

struct TLL_DvbSBlock
{
  dword BlockSize;
  TLL_DvbsHeaderSubblock HeaderBlock;
  TLL_DvbsSatelliteSubblock SatelliteBlock;
  TLL_DvbsTransponderSubblock TransponderBlock;
  TLL_DvbsChannelSubblock ChannelBlock;
  TLL_DvbsLnbSubblock LnbBlock;
};
