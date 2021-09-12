#include <stddefs.h>

struct SHeader
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

struct SChannel_fta
{
	uint32 curProgNr;
	uint32 u1;
	uint8 u2[8];
	uint32 favNr;
    char chName1[200];
	uint16 u3;
	uint8 u3b[208];
	uint8 u3c[2];
	uint16 u3d;	
	uint8 u4[10];
	uint32 freqInMhz1;
	//uint16 u5;
	uint16 u6;
	uint16 symRate;
	uint32 oldProgNr;
	uint32 channelIndex;
	uint16 tsid;
	uint16 u7;
	uint16 sid;
	uint16 onid;
	uint16 freqInMhz2;
	uint8 padding[6];
};

struct SFooter
{
	uint32 numDataBlocks;
	uint32 numDataBlockBytes;
	uint16 bytesumFrom0;
	uint16 u_zero;
};

#pragma script("get_doc_size.js")

public struct Philips_mgr_chan_s_fta
{
    var docSize = GetDocumentSize();

    char filename[32];
	
	SHeader header;

	var recordCount = header.channelBlockSize / sizeof(SChannel_fta);
    SChannel_fta channels[recordCount];
	
	SFooter footer;	
};

//#########################################################

struct SChannel_pkg
{
	uint32 curProgNr;
	uint32 u1;
	uint8 u2[8];
	uint32 favNr;
    char chName1[200];
	uint16 u3;
	uint8 u3b[208];
	uint8 u3c[2];
	uint16 u3d;	
	uint8 u4[10];
	uint32 freqInMhz1;
	//uint16 u5;
	uint16 u6;
	uint16 symRate;
	uint32 oldProgNr;
	uint32 channelIndex;
	uint16 tsid;
	uint16 u7;
	uint16 sid;
	uint16 onid;
	uint16 freqInMhz2;
	uint8 padding[10];
};

public struct Philips_mgr_chan_s_pkg
{
    var docSize = GetDocumentSize();

    char filename[32];
	
	SHeader header;

	var recordCount = header.channelBlockSize / sizeof(SChannel_pkg);
    SChannel_pkg channels[recordCount];
	
	SFooter footer;	
};

//#########################################################

struct CChannel
{
	uint32 curProgNr;
	uint32 u1;
	uint8 u2[8];
	uint32 favNr;
	union
	{
		char chName1[200];
#pragma byte_order (BigEndian)
		big_endian wchar_t chName2[100];
#pragma byte_order ()
		struct 
		{
			uint8 zero;
			wchar_t chName3[99];
			uint8 zero2;
		} chName4;
	} chName;
	uint16 u3;
	uint16 u3b;
	char provider[200];
	uint8 u4[16];
	uint32 freqInHz;
	uint16 u6;
	uint16 not_symRate;
	uint32 oldProgNr;
	uint8 u7[4];
	uint32 channelIndex;
	uint16 tsid;
	uint16 symRate_maybe;
	uint16 sid;
	uint16 onid;
	//uint16 freqInMhz2;
	//uint16 u9;
	uint32 u10;
};


public struct Philips_mgr_chan_dvbt
{
    var docSize = GetDocumentSize();

    char filename[32];
	
	SHeader header;

	var recordCount = header.channelBlockSize / sizeof(CChannel);
    CChannel channels[recordCount];
	
	SFooter footer;	
};

#pragma byte_order(LittleEndian)

struct ProgNr
{
	WORD nr : 14;
	WORD flags : 2;
};

public struct Philips_FLASH_DTVINFO_S_FTA
{
	char ddtc[4];
	var off0 = current_offset;
	struct 
	{
		struct
		{
			WORD u1 : 3;
			WORD isRadio: 1;
			WORD u2 : 1;
			WORD transponderIndex : 11;
		} info;
		WORD chanIdx;
	} chanIndexInfo[(0x5f2c - off0)/4];
	
	off0 = current_offset;
	struct s_transponder
	{
		var off1 = current_offset;
		WORD id;
		WORD u1;
		WORD freqInMhz;
		WORD symbolRate;
		BYTE u2[2];
		WORD onid;
		WORD tsid;
		WORD nid;	
		BYTE u3[5];
		WORD freqInMhz2;
		BYTE u4[8];
	} transponder[(0x10000 - off0)/sizeof(s_transponder)];
	
	BYTE filler[4];
	char ddtc2[4];
	
	struct s_channel
	{
		WORD idx1;
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
	} channel[*];
};