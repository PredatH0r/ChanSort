#include "chansort.h"

// 144 = C Series
// 172 = D Series
// 168 = E,F Series

struct SCM_mapSate_C_entry
{
    word ProgramNr;
    word VideoPid;
    word PcrPid;
    byte Unknown6;
    byte Status_InUse;
    byte Unknown8[5];
    byte Lock;
    byte ServiceType;
    byte Unknown15;
    word ServiceId;
    word TransponderNr;
    word SatelliteNr;
    byte Unknown22[2];
    word TransportStreamId;
    byte Unknown26[2];
    word OriginalNetworkId;
    byte Unknown30[2];
    word HRes;
    word VRes;
    uc16be Name[50];
    byte Encrypted;
    byte Unknown137;
    word Bouquet;
    byte Unknown140[3];
    byte Checksum;
};

public struct SCM_mapSate_C
{
  SCM_mapSate_C_entry Entries[*];
};


struct SCM_mapSate_D_entry
{
    word ProgramNr;
    word VideoPid;
    word PcrPid;
    byte Unknown6;
    byte Status_InUse;
    byte Unknown8[5];
    byte Lock;
    byte ServiceType;
    byte Unknown15;
    word ServiceId;
    word TransponderNr;
    word SatelliteNr;
    byte Unknown22[2];
    word TransportStreamId;
    byte Unknown26[2];
    word OriginalNetworkId;
    byte Unknown30[2];
    word HRes;
    word VRes;
    uc16be Name[50];
    byte Encrypted;
    byte Unknown137;
    word Bouquet;
    dword Favorite1;
    dword Favorite2;
    dword Favorite3;
    dword Favorite4;
    dword Favorite5;
    byte Unknown160[11];
    byte Checksum;
};

public struct SCM_mapSate_D
{
  SCM_mapSate_D_entry Entries[*];
};

struct SCM_mapSate_EF_entry
{
    word ProgramNr;
    word VideoPid;
    word PcrPid;
    byte Unknown6;
    byte Status_InUse;
    byte Unknown8[5];
    byte Lock;
    byte ServiceType;
    byte Unknown15;
    word ServiceId;
    word TransponderNr;
    word SatelliteNr;
    byte Unknown22[2];
    word TransportStreamId;
    byte Unknown26[2];
    word OriginalNetworkId;
    byte Unknown30[2];
    word HRes;
    word VRes;
    uc16be Name[50];
    byte Encrypted;
    byte Unknown137;
    word Bouquet;
    dword Favorite1;
    dword Favorite2;
    dword Favorite3;
    dword Favorite4;
    dword Favorite5;
    byte Unknown160[7];
    byte Checksum;
};

public struct SCM_mapSate_EF
{
  SCM_mapSate_EF_entry Entries[*];
};


struct SCM_mapAstraHDPlus_entry
{
    word ProgramNr;
    word VideoPid;
    word PcrId;
    byte u6;
    byte InUse;
    byte u8[6];
    byte ServiceType;
    byte u15;
    word ServiceId;
    word TransponderChannel;
    word ProgramNr2;
    byte u22[10];
    word OriginalNetworkId;
    word NetworkId_Maybe;
    word TransportStreamId;
    byte u38[10];
    uc16be Name[50];
    byte u148[32];
    byte Encrypted;
    byte Unknown181[3];
    dword Favorite1;
    dword Favorite2;
    dword Favorite3;
    dword Favorite4;
    dword Favorite5;
    byte Unknown160[7];
    byte Checksum;
};

public struct SCM_mapAstraHDPlus
{
  SCM_mapAstraHDPlus_entry Entries[*];
};
