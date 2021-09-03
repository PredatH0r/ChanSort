#include "chansort.h"

#define channelSize 660
#define transponderSize 392
#define satelliteSize 68

enum ChannelType : byte
{
	Tv = 1,
	Radio = 2
};

struct DvbId
{
	word id : 13;
	word extra : 3;
};

struct Channel_5048
{
	var off0 = current_offset;
	word channelIndex;
	byte u1[13];
	ChannelType channelType;
	word serviceType;
	byte u2[2];
	word transponderIndex;
	word pmtPid;
	word u3;
	word PcrPid;
	word u4[2];
	word programNr;
	word serviceId;
	byte u5[22];
	word audioPid;
	byte u6[186];
	char name[66];
	char provider[270];
	byte unk[channelSize - (current_offset - off0)];
};

struct Transponder_5048
{
	var off0 = current_offset;
	word satelliteIndex;
	word u1[2];
	word tsid;
	word onid;
	word nid_maybe;
	word u2;
	word transpoderNumber;
	word freqInMhz;
	byte u3[10];
	word symrate;
	byte unk[transponderSize - (current_offset - off0)];
};

struct Satellite_5048
{
	var off0 = current_offset;
	byte u1[2];
	char name[32];
	byte u2[2];
	word lowFreq;
	word highFreq;
	byte unk[satelliteSize - (current_offset - off0)];
};

public struct dtv_cmdb_2_5048
{
	char magic[4];
	word u1;
	word u2;
	byte channelBitmap[752];
	Channel_5048 channels[6000];
		
	byte transponderBitmap[376];
	Transponder_5048 transponder[3000];
		
	byte unknownBitmap[32];
	struct {
		word u1;
		word onid;		
		byte unknownData[50];
	} unknown[254];
	
	byte satelliteBitmap[32];
	Satellite_5048 satellites[254];
	
	byte _0x30;
};


#undef channelSize
#define channelSize 256
#undef transponderSize
#define transponderSize 52
#undef satelliteSize
#define satelliteSize 68

struct Channel_1684
{
	var off0 = current_offset;
	word channelIndex;
	byte u1[13];
	ChannelType channelType;
	word serviceType;
	byte u2[2];
	word transponderIndex;
	DvbId pmtPid;
	word u3;
	DvbId pcrPid;
	DvbId videoPid_Maybe;
	word u4;
	word programNr;
	word serviceId;
	byte u5[22];
	DvbId audioPid;
	byte u6[90];
	char name[50];
	char provider[52];
	byte unk[channelSize - (current_offset - off0)];
};

struct Transponder_1684
{
	var off0 = current_offset;
	word satelliteIndex;
	word u1[2];
	word tsid;
	word onid;
	word nid_maybe;
	word u2;
	word transpoderNumber;
	word freqInMhz;
	byte u3[10];
	word symrate;
	byte unk[transponderSize - (current_offset - off0)];
};

struct Satellite_1684
{
	var off0 = current_offset;
	byte u1[2];
	char name[32];
	byte u2[2];
	word lowFreq;
	word highFreq;
	byte unk[satelliteSize - (current_offset - off0)];
};

public struct dtv_cmdb_2_1684
{
	char magic[4];
	word u1;
	word u2;
	byte channelBitmap[752];
	Channel_1684 channels[6000];
		
	byte transponderBitmap[376];
	Transponder_1684 transponder[3000];
		
	byte unknownBitmap[32];
	struct {
		word u1;
		word onid;		
		byte unknownData[50];
	} unknown[254];
	
	byte satelliteBitmap[32];
	Satellite_1684 satellites[254];
	
	byte _0x30;
};


#undef channelSize
#define channelSize 200
#undef transponderSize
#define transponderSize 40
#undef satelliteSize
#define satelliteSize 72

struct Channel_1322
{
	var off0 = current_offset;
	word channelIndex;
	byte u1[14];
	//ChannelType channelType;
    byte u1b;
	byte serviceType;
	word transponderIndex;
	DvbId pmtPid;
	byte u2[2];
	DvbId videoPid_maybe;
	DvbId PcrPid;
	word u4[2];
	word programNr;
	word serviceId;
	byte u5[16];
	DvbId audioPid;
	byte u6[90];
	char name[50];
	byte unk[channelSize - (current_offset - off0)];
};

struct Transponder_1322
{
	var off0 = current_offset;
	word satelliteIndex;
	word u1[2];
	word tsid;
	word onid;
	word nid_maybe;
	word u2;
	word transpoderNumber;
	word freqInMhz;
	byte u3[10];
	word symrate;
	byte unk[transponderSize - (current_offset - off0)];
};

struct Satellite_1322
{
	var off0 = current_offset;
	byte u1[2];
	char name[16];
	byte u2[40];
	word orbitalPos;
	byte unk[satelliteSize - (current_offset - off0)];
};

public struct dtv_cmdb_2_1322
{
	char magic_0005[4];
	word u1[4];
	byte channelBitmap[752];
	Channel_1322 channels[6000];
		
	byte transponderBitmap[376];
	Transponder_1322 transponder[3000];
		
	byte unknownBitmap[32];
	struct {
		word u1;
		word onid;		
		byte unknownData[50];
	} unknown[254];
	
	byte satelliteBitmap[32];
	Satellite_1322 satellites[254];
	
	byte _0x30;
};


#undef channelSize
#define channelSize 196
#undef transponderSize
#define transponderSize 40
#undef satelliteSize
#define satelliteSize 64

struct Channel_1296
{
	var off0 = current_offset;
	word channelIndex;
	byte u1[13];
	byte serviceType;	
	word transponderIndex;
	DvbId pmtPid;
	byte u2[2];
	DvbId pcrPid;
	DvbId videoPid_maybe;
	word u4;
	word programNr;
	word u4b;
	word serviceId;
	byte u5[14];
	DvbId audioPid;
	byte u6[90];
	char name[50];
	byte unk[channelSize - (current_offset - off0)];
};

struct Transponder_1296
{
	var off0 = current_offset;
	word satelliteIndex;
	word u1[2];
	word tsid;
	word onid;
	word nid_maybe;
	word u2;
	word transpoderNumber;
	word freqInMhz;
	byte u3[10];
	word symrate;
	byte unk[transponderSize - (current_offset - off0)];
};

struct Satellite_1296
{
	var off0 = current_offset;
	byte u1[2];
	char name[16];
	byte u2[40];
	word orbitalPos;
	byte unk[satelliteSize - (current_offset - off0)];
};

public struct dtv_cmdb_2_1296
{
	char magic_0005[4];
	word u1[2];
	byte channelBitmap[752];
	Channel_1296 channels[6000];
		
	byte transponderBitmap[376];
	Transponder_1296 transponder[3000];
		
	byte unknownBitmap[32];
	struct {
		word u1;
		word onid;		
		byte unknownData[48];
	} unknown[254];
	
	byte satelliteBitmap[32];
	Satellite_1296 satellites[254];
	
	byte _0x30;
};
