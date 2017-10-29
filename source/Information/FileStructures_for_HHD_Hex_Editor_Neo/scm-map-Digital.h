#include "chansort.h"

struct SCM_mapDigital_B_entry
{
   word ProgramNr;
   word VideoPid;
   word PcrPid;
   byte Fav1;
   byte Qam;
   byte Status;
   byte ServiceType;
   word ServiceId;
   word Onid;
   word Nid;
   byte u16[7];
   byte Encrypted;
   byte u24[2];
   byte ChannelTransponder;
   byte u27;
   word LogicalProgramNr;
   byte u30[2];
   word SymbolRate;
   byte u34[2];
   word TransportStreamId;
   byte u38[6];
   uc16be Name[50];
   byte u144[101];
   byte Lock;
   byte Favorites;
   byte Checksum;
};

public struct SCM_mapDigital_B
{
  SCM_mapDigital_B_entry Entries[*];
};


struct SCM_mapDigital_C_entry
{
   word ProgramNr;
   word VideoPid;
   word PcrPid;
   word ServiceId;
   byte Unknown8[4];
   byte Qam;
   byte Skip;
   byte Bandwidth;
   byte ServiceType;
   byte Codec;
   byte Unknown16[3];
   word HRes;
   word VRes;
   byte Encrypted;
   byte Hidden;
   byte Unknown26[2];
   word SymbolRate;
   byte Unknown30;
   byte Lock;
   word OriginalNetworkId;
   word NetworkId;
   byte Unknown36[4];
   word ServiceProviderId;
   word ChannelTransponder;
   word LogicalProgramNr;
   byte Unknown46[2];
   word TransportStreamId;
   byte Unknown50[14];
   uc16be Name[100];
   uc16be ShortName[9];
   byte VideoFormat;
   byte Unknown283[6];
   byte Unknown289;
   byte Favorites;
   byte Checksum;
};

public struct SCM_mapDigital_C
{
  SCM_mapDigital_C_entry Entries[*];
};



struct SCM_mapDigital_DEF_entry
{
   word ProgramNr;
   word VideoPid;
   word PcrPid;
   word ServiceId;
   byte Skip_Deleted;
   byte InUse;
   byte SignalSource;
   byte SignalType;
   byte Qam;
   byte Skip;
   byte BandwidthIs8MHz;
   byte ServiceType;
   byte Codec;
   byte Unknown16[3];
   word HRes;
   word VRes;
   byte Encrypted;
   byte Hidden;
   byte Unknown26[2];
   word SymbolRate;
   byte Unknown30;
   byte Lock;
   word OriginalNetworkId;
   word NetworkId;
   byte Unknown36[4];
   word ServiceProviderId;
   word ChannelTransponder;
   word LogicalProgramNr;
   byte Unknown46[2];
   word TransportStreamId;
   byte Unknown50[14];
   uc16be Name[100];
   uc16be ShortName[9];
   byte VideoFormat;
   byte Unknown283[7];
   word Zero;
   dword Fav1;
   dword Fav2;
   dword Fav3;
   dword Fav4;
   dword Fav5;
   byte Unknown312[7];
   byte Checksum2;
};

public struct SCM_mapDigital_DEF
{
  SCM_mapDigital_DEF_entry Entries[*];
};

