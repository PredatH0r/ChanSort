#include "chansort.h"

// CRCs are calculated MSB first (left-shift with initial mask 0x80000000), polynomial 0x04C11DB7, init-value 0xFFFFFFFF and exit-XOR 0x00000000


public struct LaSat
{
	int32 dataBlockLength;
	int32 u1;
	uint32 crcDataBlock;
	uint8 u2[12];
	char lasaMarker[4];
	union
	{
		uint8 raw[dataBlockLength];
		struct
		{
			uint8 unk3[52];
			uint32 crcHeader;
			uint32 crcChannels;
			uint32 crcTransponders;
			uint32 crcSatellites;
			uint8 header[28];
			struct
			{
				uint16 u1;
				uint16 zero1;
				uint16 zero2;
				uint16 sid;
				uint16 u2;
				uint16 pcrPid;
				uint16 vpid;
				uint16 apid2;
				uint16 vtPid;
				uint16 apid1;
				uint16 transponderIndex;
				char dvbName[20];
			} channels[4000];
			struct
			{
				uint16 symbolrate;
				uint16 u1;
				uint32 freqInHz;
				uint16 tsid;
				uint8 u2[3];
				uint8 satIndexMaybe;
				uint8 u3[2];
			} transponders[1000];
			struct
			{
				uint8 u1[4];
				uint16 lowFreq;
				uint16 highFreq;
				char name[20];
				uint8 u2[2];
			} satellites[30];
		} structured;
	} dataBlock;
	uint8 suffix_0A_0D[2]; // this may or may not be present, also additional data may follow, which should be kept as-is
};