#include "tll-common.h"

#define MAX_SAT_COUNT 64
struct TLL44_Satellite;
typedef TLL44_Satellite TLL_Satellite;

#define MAX_TP_COUNT 2400
struct TLL44_Transponder;
typedef TLL44_Transponder TLL_Transponder;

#define DVBS_CHANNELLIST_PREFIXSIZE 0
#define MAX_DVBS_COUNT 7520
struct TLL76_SatChannel;
typedef TLL76_SatChannel TLL_SatChannel;

#define MAX_LNB_COUNT 40
struct TLL48_Lnb;
typedef TLL48_Lnb TLL_Lnb;

#include "tll-satellite.h"

struct LP84_AnalogChannel
{
	byte unknown[84];
	/*
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
  */
};

struct LP84_AnalogBlock
{
  dword BlockSize; 
  dword ChannelCount;
  LP84_AnalogChannel Channels[ChannelCount];
};

struct LP84_FirmwareBlock
{
  dword BlockSize;
  byte Data1[12635];
  TLL_HotelSettings HotelSettings;  
  byte Data2[BlockSize - 12635 - sizeof(TLL_HotelSettings)];
};

struct LP84_DvbCtChannel
{
};

struct LP84_DvbCTBlock
{
  dword BlockSize;
  dword ChannelCount;
  LP84_DvbCtChannel Channels[ChannelCount];
};

struct TLL44_Transponder
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

  byte t4[10]; 
  byte SatIndexTimes1; 
  byte t5[5]; 
};

struct TLL76_SatChannel
{
  word LnbIndex;
  word t1;
  TLL_SignalSource SignalSource;
  word TP_Number; 
  byte t2;
  word ChannelNumber; 
  word LogicalChannelNumber;
  word TP_Number2;
  byte t3[2];
  byte FavDelCrypt;
  byte LockSkipHide;   
  byte t4[2];
  word SID;       
  byte ServiceType;
  byte CH_NameLength; 
  char CH_Name[40];
  word VID; 
  word AID; 
  word AID_Times8;  
  byte t6[6];
};

struct TLL48_Lnb
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


public struct LP84
{
  byte Header[4]; 
  
  LP84_AnalogBlock Analog;
  LP84_FirmwareBlock Firmware;
  TLL_SettingsBlock HotelData;
  LP84_DvbCTBlock DvbCT;
  TLL_DvbSBlock DvbS;
  TLL_SettingsBlock Settings;
};
