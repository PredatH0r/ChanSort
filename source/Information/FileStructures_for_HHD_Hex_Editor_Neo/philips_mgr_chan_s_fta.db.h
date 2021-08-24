#include <stddefs.h>

public struct Philips_mgr_chan_s_fta
{
	uint8 header[64];
	struct
	{
		uint32 curProgNr;
		uint32 u1;
		uint8 u2[8];
		uint32 favNr;
		union
		{
			char chName1[200];
//#pragma byte_order(BigEndian)
			big_endian wchar_t chName2[100];
			little_endian wchar_t chName3[100];
			struct 
			{
				uint8 zero;
				wchar_t chName3[99];
				uint8 zero2;
			} chName4;
//#pragma byte_order(Default)
		} chName;
		uint16 u3;
		uint8 u3b[208];
		uint8 u3c[2];
		uint16 u3d;	
		uint8 u4[10];
		uint16 freqInMhz1;
		uint16 u5;
		uint16 u6;
		uint16 symRate;
		uint32 curProgNr2;
		uint32 prevProgNr;
		uint16 tsid;
		uint16 u7;
		uint16 sid;
		uint16 onid;
		uint16 freqInMhz2;
		uint16 u9;
		uint32 u10;
	} channels[1231];
	struct
	{
		uint8 u1[8];
		uint16 wordsum;
		uint8 u2[2];
	} footer;
	
};