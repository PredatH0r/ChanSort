typedef unsigned char byte;
typedef unsigned short word;
typedef unsigned int dword;

#define ACT_CHANNEL_PADDING 4
#define SAT_CHANNEL_PADDING 2
#define MAX_SAT_COUNT 64
#define MAX_LNB_COUNT 40
#define MAX_DVBS_COUNT 7520
#define MAX_TP_COUNT 2400

public struct PT180_AnalogChannel
{
	word t1[5];
  byte ChannelTransponder1;
  byte t1b;
	word ProgramNr;
	word t2[3];
  byte Favorites1;
  byte t2b;
  word Frequency1Div50;
	word APID1;
  byte ChannelNumberInBand;
  byte ChannelBand;
	byte t3[2];
	char CH_Name1[40];
	byte CH_NameLength1;
	byte t4[1];
	word SID1;
	byte t5a[16];
  byte ChannelTransponder2;
  byte t5b;
	dword Frequency;
	byte t6[2];
	word ONID;
	word TSID;
	byte t7[18];
  word ProgramNr2;
  byte t8[2];
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


public struct PT180_DvbCtChannel
{
	word t1[5];
	byte ChannelTransponder1;
  byte t1f;
	word ProgramNr;
	word t2[3];
	byte Favorites1;
	byte t2d;
  word PcrPid1;
	word APID1;
  word VPID1;
	byte t3[2];
	char CH_Name1[40];
	byte CH_NameLength1;
	byte t4;
	word SID1;
	byte t5a[16];
  byte ChannelTransponder2;
  byte t5b;
	dword Frequency;
	byte t6[2];
	word ONID;
	word TSID;
	byte t7[18];
  word ProgramNr2;
  byte t8[2];
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

public struct PT180_SatChannel
{
	byte t1[2];
	byte t2[4];
	word TP_Number; 
	word CH_Number; 
	byte t3[5];
	byte EditFlag;   
	word SID;       
	byte ServiceType;
	byte CH_NameLength; 
	char CH_Name[40];
	word VID; 
	word AID; 
	word t4;  
	byte t5[SAT_CHANNEL_PADDING];
};

public struct PT180_Satellite
{
	char Name[32]; 
	byte PosDeg; 
	byte PosCDeg; 
	byte t1[10];
};

public struct PT180_Transponder
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
	byte SatIndex; 
	byte t5[3]; 
};

public struct PT180_Lnb
{
	byte t1[12];
	byte SettingsID; 
	byte t2[3];
	byte SatelliteID;
	byte t3[3];
	char FrequenceName[12]; 
	word LOF1; 
	byte t4[2]; 
	word LOF2; 
	byte t5[6]; 
};

struct PT180_AnalogBlock
{
  dword BlockSize; 
	dword ChannelCount;
	PT180_AnalogChannel Channels[ChannelCount];
};

struct PT180_FirmwareBlock
{
  dword BlockSize;
  byte Data[BlockSize];
};

struct PT180_DvbCTBlock
{
  dword BlockSize;
	dword ChannelCount;
	PT180_DvbCtChannel Channels[ChannelCount];
};

struct PT180_DvbSBlock
{
  dword BlockSize;
  byte Temp03[0x20];
	byte Temp04[0x44]; 	
  PT180_Satellite Satellites[MAX_SAT_COUNT];
	byte Temp05[0x397C];
  PT180_Transponder Transponder[MAX_TP_COUNT];	
  dword Checksum; 
	byte Temp06[6];
	word DVBS_MaxIndex1;
	word DVBS_MaxIndex2;
	word DVBS_ChannelCount;
  byte Temp07[0xEEAC];
  PT180_SatChannel Channels[MAX_DVBS_COUNT];
  PT180_Lnb Lnb[MAX_LNB_COUNT];
  byte Temp08[12];
};

struct PT180_SettingsBlock
{
  dword BlockSize;
	byte Data[BlockSize]; 
};

public struct PT180
{
	byte Header[4]; 
	
  PT180_AnalogBlock Analog;
  PT180_FirmwareBlock Firmware;
	PT180_DvbCTBlock DvbCT;
  PT180_DvbSBlock DvbS;
  PT180_SettingsBlock Settings;
};
