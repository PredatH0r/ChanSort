typedef unsigned char byte;
typedef unsigned short word;
typedef unsigned int dword;

#define ACT_CHANNEL_PADDING 4
#define SAT_CHANNEL_PADDING 2
#define MAX_SAT_COUNT 64
#define MAX_LNB_COUNT 40
#define MAX_DVBS_COUNT 7520
#define MAX_TP_COUNT 2400

public struct LD176_AnalogChannel
{
	word t1[5];
	byte ChannelTransponder1;
  byte t1f;
	word ProgramNr;
	word t2[3];
	byte Favorites1;
	byte t2d;
	word Freqency1Div50;
	word APID1;
  byte ChannelNumberInBand;
  byte ChannelBand;
  byte t3[2];
	char CH_Name1[40];
	byte CH_NameLength1;
	byte t4;
	word SID1;
	byte t5[10]; // !
	word t5b;
	byte ChannelTransponder2;
  byte t5c;
	dword Frequency;
	byte t6[2];
  word ONID;
  word TSID;
	byte t7[19];
  word ProgramNr2;
	byte t8;
	byte ChannelTransponder4;
  byte t9;
	byte Favorites2;
  byte LockSkipHide;
  word SID2;
  byte ServiceType;
	byte CH_NameLength2;
	char CH_Name2[40];
  word Frequency2Div50;
	word t10[3];
};


public struct LD176_DvbCtChannel
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
	byte t5a[12]; // !
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
  word t11;
	word t12;
};

public struct LD176_SatChannel
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

public struct LD176_Satellite
{
	char Name[32]; 
	byte PosDeg; 
	byte PosCDeg; 
	byte t1[10];
};

public struct LD176_Transponder
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

public struct LD176_Lnb
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

struct LD176_AnalogBlock
{
  dword BlockSize; 
	dword ChannelCount;
	LD176_AnalogChannel Channels[ChannelCount];
};

struct LD176_FirmwareBlock
{
  dword BlockSize;
  byte Data[BlockSize];
};

struct LD176_DvbCTBlock
{
  dword BlockSize;
	dword ChannelCount;
	LD176_DvbCtChannel Channels[ChannelCount];
};

struct LD176_DvbSBlock
{
  dword BlockSize;
  byte Temp03[0x20];
	byte Temp04[0x44]; 	
  LD176_Satellite Satellites[MAX_SAT_COUNT];
	byte Temp05[0x397C];
  LD176_Transponder Transponder[MAX_TP_COUNT];	
  dword Checksum; 
	byte Temp06[6];
	word DVBS_MaxIndex1;
	word DVBS_MaxIndex2;
	word DVBS_ChannelCount;
  byte Temp07[0xEEAC];
  LD176_SatChannel Channels[MAX_DVBS_COUNT];
  LD176_Lnb Lnb[MAX_LNB_COUNT];
  byte Temp08[12];
};

struct LD176_SettingsBlock
{
  dword BlockSize;
	byte Data[BlockSize]; 
};

public struct LD176
{
	byte Header[4]; 
	
  LD176_AnalogBlock Analog;
  LD176_FirmwareBlock Firmware;
	LD176_DvbCTBlock DvbCT;
  LD176_DvbSBlock DvbS;
  LD176_SettingsBlock Settings;
};
