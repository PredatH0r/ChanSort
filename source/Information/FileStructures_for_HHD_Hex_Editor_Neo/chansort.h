typedef unsigned char byte;
typedef unsigned short word;
typedef unsigned int dword;
typedef unsigned __int64 qword;

typedef unsigned char BYTE;
typedef unsigned short WORD;
typedef unsigned int DWORD;
typedef unsigned __int64 QWORD;

typedef big_endian unsigned short uc16be;

#ifndef stddef

typedef char int8;
typedef short int16;
typedef long int32;
typedef __int64 int64;

typedef unsigned int uint;
typedef unsigned char uint8;
typedef unsigned short uint16;
typedef unsigned long uint32;
typedef unsigned __int64 uint64;

#endif

enum ServiceType : byte
{
	SDTV = 1,
	Radio = 2,
	Data = 12,
	SDTV_MPEG4 = 22,
	HDTV = 25,
	Option = 211
};


function trim(str)
{
	var i = find(str, "\0");
	return i < 0 ? str : substring(str, 0, i);
}
