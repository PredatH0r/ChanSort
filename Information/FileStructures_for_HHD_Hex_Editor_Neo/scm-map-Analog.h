#include "chansort.h"

struct SCM_mapAnalog_B_entry
{
  byte InUse;
  byte u2[3];
  word ProgramNr;
  byte u3[6];
  uc16be Name[6];
  word u4;
  byte Favorites_Maybe;
  byte Checksum;
};

public struct SCM_mapAnalog_B
{
  SCM_mapAnalog_B_entry Entries[*];
};



struct SCM_mapAnalog_C_entry
{
  byte u1;
  byte InUse;
  byte Skip;
  byte u2[3];
  byte Lock;
  byte u3;
  byte Tuned;
  word ProgramNr;
  byte u3[5];
  word SlotNr;
  word NameLength;
  uc16be Name[6];
  float Frequency;
  word u4;
  byte Favorites;
  byte Checksum;
};

public struct SCM_mapAnalog_C
{
  SCM_mapAnalog_C_entry Entries[*];
};



struct SCM_mapAnalog_DE_entry
{
  byte u1;
  byte InUse;
  byte Skip;
  byte u2[3];
  byte Lock;
  byte u3;
  byte Tuned;
  word ProgramNr;
  byte u3[5];
  word SlotNr;
  word NameLength;
  uc16be Name[6];
  float Frequency;
  dword Fav1;
  dword Fav2;
  dword Fav3;
  dword Fav4;
  dword Fav5;
  byte u4[7];
  byte Checksum;
};

public struct SCM_mapAnalog_DE
{
  SCM_mapAnalog_DE_entry Entries[*];
};


