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
    uint16 u3[2];
    char provider[200];

	// < unsure > offset not 100% sure
    uint8 u4[4];
	struct
	{
		uint8 isFav : 1;
	} flags1;
	uint8 u5;
	struct
	{
		uint8 u1 : 3;
		uint8 isFav : 1;
	} flags2;
    uint8 u6[5];
	uint32 favNr2;
	// </ unsure >

    uint32 freqInHz;
    uint16 u7;
    uint16 not_symRate;
    uint32 oldProgNr;
    uint8 u8[4];
    uint32 channelIndex;
    uint16 tsid;
    uint16 symRate_maybe;
    uint16 sid;
    uint16 onid;
    uint32 u9;
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
    BYTE u4[8];
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
    BYTE u5[3];
};

struct s_channelBlock
{
  char ddtc[4];
  s_channel channels[734];
  BYTE filler[0x20000-0x1ff32];
};

#define numChannelTransponderMap 5000
#define numSatTransponderMap 1090
#define numTransponder 1100
#define numSatData 4592
#define numUnknown 310*8
public struct Philips_FLASH_DTVINFO_S_FTA
{
    char ddtc[4];
    s_channelTransponder channelTranponderMap[numChannelTransponderMap];
    s_satTransponder satTransponderMap[numSatTransponderMap];
    s_transponder transponder[numTransponder];
    BYTE satData[numSatData];
    s_unknown unkData[numUnknown];
    s_channelBlock channelBlocks[*];
};

#undef numChannelTransponderMap
#define numChannelTransponderMap 2000
#undef numSatTransponderMap
#define numSatTransponderMap 590
#undef numTransponder
#define numTransponder 550
#undef numSatData
#define numSatData 4592
#undef numUnknown
#define numUnknown 310
public struct Philips_FLASH_DTVINFO_S_PKG
{
    char ddtc[4];
    s_channelTransponder channelTranponderMap[numChannelTransponderMap];
    s_satTransponder satTransponderMap[numSatTransponderMap];
    s_transponder transponder[numTransponder];
    BYTE satData[numSatData];
    var off0 = current_offset;
    BYTE unk[0x10000 - off0];
    s_channelBlock channelBlocks[*];
};
