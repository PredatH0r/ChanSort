typedef unsigned char byte;
typedef unsigned short word;
typedef unsigned int dword;

public struct LH164_AnalogChannel
{
	word t1[4];
  byte t1f;
	byte ChannelTransponder1;
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
	byte t5a[14]; // !
  word ONID;
  word TSID;
  byte t5b[6];
	dword Frequency;
	byte t6[8];
  word ProgramNr2;
  byte t7[2];
  byte ChannelTransponder4;
  byte _Favorites2;
  byte LockSkipHide;
  byte ServiceType;
	char CH_Name2[40];
	word Frequency2Div50;
	word APID2;
  word t11;
	word t12;
};


public struct LH164_DvbCtChannel
{
	word t1[4];
  byte t1a;
	byte ChannelTransponder1;
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
	byte t5a[14]; // !
  word ONID;
  word TSID;
  byte t5b[6];
	dword Frequency;
	byte t6[8];
  word ProgramNr2;
  byte t7[2];
  byte ChannelTransponder4;
  byte _Favorites2;
  byte LockSkipHide;
  byte ServiceType;
	char CH_Name2[40];
	word PcrPid2;
	word APID2;
  word t11;
	word SID2;
};


struct LH164_AnalogBlock
{
  dword BlockSize; 
	dword ChannelCount;
	LH164_AnalogChannel Channels[ChannelCount];
};

struct LH164_FirmwareBlock
{
  dword BlockSize;
  byte Data[BlockSize];
};

struct LH164_DvbCTBlock
{
  dword BlockSize;
	dword ChannelCount;
	LH164_DvbCtChannel Channels[ChannelCount];
};


public struct LH164
{
  LH164_AnalogBlock Analog;
  LH164_FirmwareBlock Firmware;
	LH164_DvbCTBlock DvbCT;
};
