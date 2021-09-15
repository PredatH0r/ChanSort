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

struct SChannel_pkg_476
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
    // some files have this additional size of 4 bytes
    //uint8 padding2[4];
};

public struct Philips_mgr_chan_s_pkg_476
{
    char filename[32];
    SHeader header;
    var recordCount = header.channelBlockSize / sizeof(SChannel_pkg_476);
    SChannel_pkg_476 channels[recordCount];
    SFooter footer;	
};

struct SChannel_pkg_480
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
    // some files have this additional size of 4 bytes
    uint8 padding2[4];
};

public struct Philips_mgr_chan_s_pkg_480
{
    char filename[32];
    SHeader header;
    var recordCount = header.channelBlockSize / sizeof(SChannel_pkg_480);
    SChannel_pkg_480 channels[recordCount];
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
    // optional:
    BYTE u5[5];
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
    // optional
    BYTE u6[4];
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


/////////////////////////////////////////////////////////////////

struct s_channelTransponder2
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

struct s_transponder2
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
    BYTE u5[5];
};

struct s_satTransponder2
{
    WORD satId_maybe;
    WORD transponderId;
};

struct s_unknown2
{
    WORD u1;
    BYTE u2[2];
    DWORD zero;
};

struct s_channel2
{
    WORD id;
    BYTE u1[4];
    BYTE u2[2];
    WORD sid;
    WORD pcrPid;
    WORD vpid;
    BYTE u3[10];
    ProgNr progNr;
    BYTE u3b[8];
    WORD apid1_maybe;
    char lang1[3];
    BYTE u4[45];
    WORD apid2_maybe;
    char lang2[3];
    BYTE u5[3];
    BYTE u6[1];
};

struct s_channelBlock2
{
    char ddtc[4];
    s_channel2 channels[703];
    BYTE filler[0x20000 - 0x1ff67];
};

#undef numChannelTransponderMap
#define numChannelTransponderMap 5000
#undef numSatTransponderMap
#define numSatTransponderMap 60
#undef numTransponder
#define numTransponder 1100
#undef numSatData
#define numSatData 3512
#undef numUnknown
#define numUnknown 2180
public struct Philips_FLASH_DTVINFO_S_FTA_for_pkg
{
    char ddtc[4];
    s_channelTransponder2 channelTranponderMap[numChannelTransponderMap];
    s_satTransponder2 satTransponderMap[numSatTransponderMap];
    s_transponder2 transponder[numTransponder];
    BYTE satData[numSatData];
    BYTE unknown[numUnknown];
    s_channelBlock2 channelBlocks[*];
};

