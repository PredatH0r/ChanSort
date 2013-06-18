#include "tll-common.h"

// all LM models except 340S and 611S

#define ACT_CHANNEL_PADDING 8
#define SAT_CHANNEL_PADDING 6
#define MAX_SAT_COUNT 64
#define MAX_LNB_COUNT 40
#define MAX_DVBS_COUNT 7520
#define MAX_TP_COUNT 2400

public struct LM192_AnalogChannel
{
	word t1[5];
	byte ChannelTransponder1;
  byte t1f;
	word ProgramNr;
	word t2[3];
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
  byte ChannelTransponder2;
  byte t5b;
	dword Frequency;
	byte t6[2];
	word ONID;
	word TSID;
	byte t7[20];
  byte ChannelTransponder3;
  byte t8;
  word ProgramNr2;
  byte t8b[2];
	byte ChannelTransponder4;
  byte t9;
	byte Favorites2;
	byte LockSkipHide;
	word SID2;
	byte ServiceType; 
	byte CH_NameLength2;
	char CH_Name2[40];
	word Frequency2Div50;
	word APID2;
	byte t11[ACT_CHANNEL_PADDING];
};

struct LM192_AnalogBlock
{
  dword BlockSize; 
	dword ChannelCount;
	LM192_AnalogChannel Channels[ChannelCount];
};

struct LM192_FirmwareBlock
{
  dword BlockSize;
  byte u1[167];
  byte SystemLock;
  byte u2;
  byte TvPassword[4];
  byte u3[34919];
  byte HbbTvEnable;
  byte u4[538];
  TLL_HotelSettings HotelSettings;  
  byte u7[42];
  byte HotelMenuPin[4];
  byte u8[834];
  byte SettingsAutoChannelUpdate; // 0x8EBC
  byte Data[BlockSize - 0x8ebc - 1];
};

public struct LM192_DvbCtChannel
{
	word t1[5];
	byte ChannelTransponder1;
  byte t1f;
	word ProgramNr;
	word t2[3];
	byte Favorites1;
	byte t2d;
  word PcrPid;
	word APID1;
  word VPID1;
	byte t3[2];
	char CH_Name1[40];
	byte CH_NameLength1;
	byte t4;
	word SID1;
	byte t5a[20];
  byte ChannelTransponder2;
  byte t5b;
	dword Frequency;
	byte t6[2];
	word ONID;
	word TSID;
	byte t7[20];
  byte ChannelTransponder3;
  byte t8a;
  word ProgramNr2;
  byte t8b[2];
	byte ChannelTransponder4;
  byte t9;
	byte Favorites2;
	byte LockSkipHide;
	word SID2;
	byte ServiceType; 
	byte CH_NameLength2;
	char CH_Name2[40];
	word PcrPid2;
	word APID2;
	byte t11[ACT_CHANNEL_PADDING];
};

struct LM192_DvbCTBlock
{
  dword BlockSize;
	dword ChannelCount;
	LM192_DvbCtChannel Channels[ChannelCount];
};

struct LM192_DvbsHeaderSubblock
{
  dword Crc32;
  byte DVBS_S2_Tag[8];
  word Temp03[2];
};

public struct LM192_Satellite
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

public struct LM192_DvbsSatelliteSubblock
{
  dword Crc32;
  word Unknown1;
  byte SatAllocationBitmap[MAX_SAT_COUNT/8];
  word Unknown2;
  word SatCount;
  byte SatOrder[MAX_SAT_COUNT];
  word Unknown3;
  LM192_Satellite Satellites[MAX_SAT_COUNT];
};

public struct LM192_Transponder
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

public struct LM192_DvbsTransponderSubblock
{
  dword Crc32;
  word Unknown1;
  word Unknown2;
  word Unknown3;
  word Unknown4;
  word TransponderCount;
  byte AllocationBitmap[MAX_TP_COUNT/8];
  struct LM192_DvbsTransponderTable1
  {
    word Prev;
    word Next;
    word Current;
  } TransponderTable1[MAX_TP_COUNT];
	word Unknown5;
  LM192_Transponder Transponder[MAX_TP_COUNT];	
};

public struct LM192_SatChannel
{
	byte t1[2];
	byte t2[4];
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
	word VID; 
	word AID; 
	word AID_Times8;  
	byte t6[SAT_CHANNEL_PADDING];
};

struct LM192_DvbsChannelSubblock
{
  dword Crc32; 
	word Unknown[2];
  word LinkedListStartIndex;
	word LinkedListEndIndex1;
	word LinkedListEndIndex2;
	word ChannelCount;
  byte AllocationBitmap[MAX_DVBS_COUNT/8];
  struct LM192_LinkedChannelList
  {
    word Prev;
    word Next;
    word Current;
    word Zero;
  } LinkedList[MAX_DVBS_COUNT];
  LM192_SatChannel Channels[MAX_DVBS_COUNT];
};

public struct LM192_Lnb
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

struct LM192_DvbsLnbSubblock
{
  dword Crc32;
  word Unknown1;
  byte AllocationBitmap[5];
  byte Unknown2;
  LM192_Lnb Lnb[MAX_LNB_COUNT];
};

struct LM192_DvbSBlock
{
  dword BlockSize;
  LM192_DvbsHeaderSubblock HeaderBlock;
  LM192_DvbsSatelliteSubblock SatelliteBlock;
  LM192_DvbsTransponderSubblock TransponderBlock;
  LM192_DvbsChannelSubblock ChannelBlock;
  LM192_DvbsLnbSubblock LnbBlock;
};

struct LM192_SettingsBlock
{
  dword BlockSize;
	byte Data[BlockSize]; 
};

public struct LM192
{
	byte Header[4]; 
	
  LM192_AnalogBlock Analog;
  LM192_FirmwareBlock Firmware;
	LM192_DvbCTBlock DvbCT;
  LM192_DvbSBlock DvbS;
  LM192_SettingsBlock Settings;
};
