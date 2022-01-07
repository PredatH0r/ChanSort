#include "chansort.h"

/***********************************************************
 * s2channellib / DVB-S files
 ***********************************************************/
 
struct Ph_NextPrevTableEntry
{
  word next;
  word prev;
};

public struct Ph_DbFileInfoDat
{
  dword dataSize;
  var off0 = current_offset;
  byte unk1[6];
  struct
  {
    word unk : 2;
    word seqNr : 14;
  } seq;
  byte unk[dataSize - (current_offset - off0)];
  dword crc32;
};

public struct Ph_SatelliteDat
{
  word unk1;
  word unk2;
  dword recordSize;
  dword recordCount;
  Ph_NextPrevTableEntry NextPrevTable[recordCount];
  struct
  {
    var off0 = current_offset;
    dword oneShiftLeftSatIndex;
    word unk1;
    word unk2;
    byte orbitalPos;
    byte unk3[7];
    char name[16];   
    byte unk[recordSize - (current_offset - off0)];
  } Satellites[recordCount];
  dword crc32;
};

public struct Ph_TuneinfoDat_1_0
{
  word unk1;
  word unk2;
  dword recordSize;
  dword recordCount;
  Ph_NextPrevTableEntry NextPrevTable[recordCount];
  struct
  {
    var off0 = current_offset;
    word symbolRate;
    word freqInMhz;
    word unk1;
    struct
    {
      byte unk : 4;
      byte satIndex: 4;
    } u1a;
    byte unk2[9];
    word tsid;
    word onid;
	byte unk3[2];
    char networkName[32];
    word unk4;
    byte unk[recordSize - (current_offset - off0)];
  } Transponders[recordCount];
  dword crc32;
};

public struct Ph_TuneinfoDat_1_2
{
  word unk1;
  word unk2;
  dword recordSize;
  dword recordCount;
  Ph_NextPrevTableEntry NextPrevTable[recordCount];
  struct
  {
    var off0 = current_offset;
    word symbolRate;
    word freqInMhz;
    word unk1;
    struct
    {
      byte unk : 4;
      byte satIndex: 4;
    } u1a;
    byte unk2[9];
    word tsid;
    word onid;
	byte unk3[6];
    char networkName[32];
    word unk4;
    byte unk[recordSize - (current_offset - off0)];
  } Transponders[recordCount];
  dword crc32;
};

public struct Ph_ServiceDat 
{
  word unk1;
  word chanCount2;
  dword chanSize;
  dword chanCount;
  Ph_NextPrevTableEntry NextPrevTable[chanCount];
  struct 
  {
    var off0 = current_offset;
    struct 
    {
      word pid : 13;
      word unk : 3;
    } pcrPid;
    byte unk1;
    struct
    {
      byte unk : 4;
      byte locked : 1;
      byte crypt : 1;
      byte isFav2 : 1;
      byte unk3 : 1;
    } flags;
    word onid;
    word tsid;
    word sid;
    word transponderIndex;
    word unk2;
    word unk3;
    struct
    {
      word vpid : 13;
      word unk : 2;
      word isFav : 1;
    } vpid;
    byte unk4;
    byte unk5;
    word progNr;
    byte unk6;
    byte unk7;
    word unk8;
    word unk9;
    char channelName[32];
    char providerName[32];
    byte unk9[chanSize - (current_offset - off0)];   
  } Channels[chanCount];
  [description("CRC32 MSBit first, init=0xFFFFFFFF, poly=0xEDB88320, post=0xFFFFFFFF, as int32 little-endian")]
  dword crc32;
};

public struct Ph_FavoriteDat
{
  dword dataSize;
  short firstIndex;
  short count;
  struct
  {
    short prev;
    short next;
  } Table[dataSize/4-1];
  dword crc32;
};

public struct Ph_SatelliteDigSrvTable_11
{
  dword version;
  dword unk2;
  dword chRecordSize;
  dword channelCount;
  dword versionCode;
  struct Ph_CableChannel
  {
    var off0 = current_offset;
    dword checksum;
    byte unk1[16];
    dword symbolRate;
    byte unk2[16];
    dword freq;
    word onid;
    word sid;
    word tsid;
    byte unk3b[58];
    word progNrMostly;
    byte unk4[6];
    word progNr;
    byte unk5[22];
    byte locked;
    byte isFav;
    byte unk6[68];
    wchar_t channelName[32];
    byte unk4[chRecordSize - (current_offset - off0)];
  } Channels[channelCount];
};

/***********************************************************
 * channellib / antenna and cable files
 ***********************************************************/

public struct Ph_CableDigSrvTable
{
  dword unk1;
  dword unk2;
  dword chRecordSize;
  dword channelCount;
  dword versionCode;
  struct Ph_CableChannel
  {
    var off0 = current_offset;
    dword checksum;
    byte unk1[20];
    dword symbolRate;
    byte unk2[20];
    struct 
    {
      word subFreq : 4;
      word mhz : 12;
    } freqTimes16;
    word onid;
    word sid;
    word tsid;
    byte unk3b[58];
    word progNrMostly;
    byte unk4[6];
    word progNr;
    byte unk5[22];
    byte locked;
    byte isFav;
    byte unk6[68];
    wchar_t channelName[32];
    byte unk4[chRecordSize - (current_offset - off0)];
  } Channels[channelCount];
};

public struct Ph_CableDigTSTable
{
  dword unk1;
  dword unk2;
  dword recordSize;
  dword recordCount;
  dword versionCode;
  struct Ph_CableChannel
  {
    var off0 = current_offset;
    dword checksum;
    dword unk1;
    dword unk2;
    struct
    {
      dword subFreq :4;
      dword mhz : 28;
    } freqInMhzTimes16a;
    dword unk3;
    dword unk4;
    dword unk5;
    dword symbolRate;
    dword unk6;
    dword unk7;
    struct
    {
      dword subFreq :4;
      dword mhz : 28;
    } freqInMhzTimes16b;
    word onid;
    word nid;
    word tsid;
    word unk10;
    byte unk4[recordSize - (current_offset - off0)];
  } Transponders[recordCount];
};

public struct Ph_CableFrqMapTable
{
  dword unk1;
  dword unk2;
  dword recordSize;
  dword recordCount;
  dword versionCode;
  struct 
  {
    var off0 = current_offset;
    dword checksum;
    struct
    {
      dword subFreq :4;
      dword mhz : 28;
    } freqInMhzTimes16;
    dword symbolRate;
    dword unk2;
    word onid;
    word tsid;
    word unk5;
    word unk6;
    dword unk7;
    byte unk[recordSize - (current_offset - off0)];
  } Transponders[recordCount];
};

public struct Ph_CablePresetTable
{
  dword unk1;
  dword unk2;
  dword recordSize;
  dword recordCount;
  dword versionCode;
  struct
  {
    var off0 = current_offset;
    dword checksum;
    struct
    {
      dword subFreq :4;
      dword mhz : 28;
    } freqInMhzTimes16;
    word progNr;
    word unk3;
    word unk4;
    word onid;
    word tsid;
    word sid;
    byte unk[recordSize - (current_offset - off0)];
  } ChanPreset[recordCount];
};
