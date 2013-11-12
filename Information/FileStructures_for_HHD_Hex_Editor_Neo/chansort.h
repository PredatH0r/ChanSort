typedef unsigned char byte;
typedef unsigned short word;
typedef unsigned int dword;
typedef big_endian unsigned short uc16be;

enum ServiceType : byte
{
	SDTV = 1,
	Radio = 2,
	Data = 12,
	SDTV_MPEG4 = 22,
	HDTV = 25,
	Option = 211
};