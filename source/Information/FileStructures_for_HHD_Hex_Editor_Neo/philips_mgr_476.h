#include "chansort.h"

#pragma byte_order(LittleEndian)

struct Header
{
  uint32 blockId;
  uint32 blockSize;
  uint16 u1;
  uint16 u2;
  uint32 numTvChannels;
  uint32 numRadioChannels;
  uint32 u3;
  uint32 channelBlockId;
  uint32 channelBlockSize;
};

struct Channel
{
    uint32 curProgNr;
    uint32 u1;
    uint8 u2[8];
    uint32 favNr;
    char name[200];
    uint8 u3[208];
	struct
	{
		uint8 isFav : 1;
	} flags1;
	uint8 u4;
	struct
	{
		uint8 u1 : 3;
		uint8 isFav : 1;
	} flags2;
    uint8 u5[5];
	uint32 favNr2;
	uint8 u6[4];
    uint32 freqInMhz1;
    uint16 u7;
    uint16 symRate;
    uint32 oldProgNr;
    uint32 channelIndex;
    uint16 tsid;
    uint16 u8;
    uint16 sid;
    uint16 onid;
    uint16 freqInMhz2;
    uint8 padding[6];
};

struct Footer
{
    uint32 numDataBlocks;
    uint32 numDataBlockBytes;
    uint16 bytesumFrom0;
    uint16 u_zero;
};

public struct Philips_mgr_chan
{
    char filename[32];  
    Header header;
    var recordCount = header.channelBlockSize / sizeof(Channel);
    Channel channels[recordCount];   
    Footer footer;	
};

//*****************************************************************************************
// FLASH files
//*****************************************************************************************

struct ProgNr
{
    WORD nr : 14;
    WORD flags : 2;
};

struct s_channelTransponder
{
    struct
    {
        WORD u1 : 3;
        WORD isRadio: 1;
        WORD u2 : 1;
        WORD transponderId : 11;
    } info;
    WORD channelId;
};

struct s_transponder
{
	var off0 = current_offset;
    WORD id;
    BYTE source_maybe;
    BYTE satId_maybe;
    WORD freqInMhz;
    WORD symbolRate;
    BYTE u2[2];
    WORD onid;
    WORD tsid;
    WORD nid;	
    BYTE u3[5];
    WORD freqInMhz2;
    BYTE u4[31 - (current_offset - off0)];
};

struct s_satTransponder
{
    WORD satId_maybe;
    WORD transponderId;
};

struct s_unknown
{
    WORD u1;
    BYTE u2[2];
    DWORD zero;
};

struct s_channel
{
	var off0 = current_offset;
    WORD id;
    BYTE u1[4];
    BYTE u2[2];
    WORD sid;
    WORD pcrPid;
    WORD vpid;
    BYTE u3[8];
    ProgNr progNr;
    BYTE u3b[7];
    WORD apid1_maybe;
    char lang1[3];
    BYTE u4[45];
    WORD apid2_maybe;
    char lang2[3];
    BYTE u5[89 - (current_offset - off0)]; // 3
};

struct s_channelBlock
{
  char ddtc[4];
  s_channel channels[734];
  BYTE filler[0x20000-0x1ff32];
};

struct s_satellite
{
	BYTE data[42];
};

#define numChannelTransponderMap 5000
#define numSatTransponderMap 1000
#define numTransponder 1100
#define numSatellites 100
#define numUnknown 250 //(0x10000-0xF650)/8
public struct Philips_FLASH_DTVINFO_S_FTA
{
    char ddtc[4];
    s_channelTransponder channelTranponderMap[numChannelTransponderMap];
    s_satTransponder satTransponderMap[numSatTransponderMap];
	BYTE unk[360];
    s_transponder transponder[numTransponder];
    s_satellite satellites[numSatellites];
	BYTE unk[0xF650-current_offset];
    s_unknown unknownTable[numUnknown];
	BYTE filler[0x10000-current_offset];
    s_channelBlock channelBlocks[*];
};

#undef numChannelTransponderMap
#define numChannelTransponderMap 2000
#undef numSatTransponderMap
#define numSatTransponderMap 500
#undef numTransponder
#define numTransponder 550
#undef numSatellites
#define numSatellites 100
#undef numUnknown
#define numUnknown 0
public struct Philips_FLASH_DTVINFO_S_PKG
{
    char ddtc[4];
    s_channelTransponder channelTranponderMap[numChannelTransponderMap];
    s_satTransponder satTransponderMap[numSatTransponderMap];
	BYTE unk[360];
    s_transponder transponder[numTransponder];
    s_satellite satellites[numSatellites];
    //s_unknown unknownTable[numUnknown];
	BYTE filler[0x10000-current_offset];
    s_channelBlock channelBlocks[*];
};

#pragma byte_order(BigEndian)		

public struct Philips_FLASH_SDTSECTS_S_PKG
{
	BYTE u1[4];
	BYTE allocationBitmap[64];
	struct
	{
		var off0 = current_offset;
		DWORD ffff;
		BYTE u1[3];
		WORD tsid;
		BYTE u2[6];
		struct Channel
		{
			WORD serviceId;
			BYTE u1[6];
			BYTE lenProviderName;
			char provider[lenProviderName];
			BYTE lenChannelName;
			char channelName[lenChannelName];		
		};		
		Channel chan0;
		BYTE u3[51];		
		Channel chan1;
		BYTE u4[60];
		Channel chan2;
		BYTE u5[18];
		Channel chans[16];
		BYTE filler[0x44C - current_offset];
	} transponder[*];
};

#pragma byte_order(LittleEndian)