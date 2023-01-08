#include "chansort.h"

struct StringChar
{
	char c;
	if (c == 0x0a)
		$break_array(true);
};

struct DvbData
{
	uint8 unknown[2848];
	struct 
	{
		uint8 satData[88];
	} satellites[85];
	int8 unknown[63343];
	struct 
	{
		int8 data[146];
	} channels[3045]; // 615
};

public struct cvt_database_dat
{
	StringChar model[*];
	StringChar systemDatabaseKeyword[*];
	struct 
	{
		StringChar blockName[*];
		big_endian long length;
    
		if (blockName[0].c == 'D')
		{
			DvbData dvbData;
			int8 filler[length - sizeof(dvbData)];
		}
		else
		  uint8 data[length];

		uint8 extra[3];
	} blocklist[*];

};