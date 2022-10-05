#include "chansort.h"

/***********************************************************
 * Panasonic LS/LX 2022 idtvChannel.bin
 ***********************************************************/

enum Flags : word
{
  Encrypted = 0x0002,
  IsFavorite = 0x0080,
  Deleted = 0x0100,
  Hidden = 0x0400,
  CustomProgNr = 0x1000
};

struct Pa_idtvChannel_bin_FileEntry
{
  word U0; // always 1
  word RecordLength; // 60 + length of channel name
  word U4; // always 6
  byte U6[3]; // all 00
  word U9; // 0 = Sat, 18 = Cable ?
  byte U11[5]; // all 00
  dword Freq; // Hz for DVB-C/T, kHz for DVB-S
  dword SymRate; // in Sym/s, like 22000000
  word U24; // always 100
  word U26; // always 0
  word U28; // always 0
  word ProgNr;
  word LcnMaybe;
  byte U32[2]; // e.g. 0a 01 00 00
  Flags Flags;
  byte U38[4]; // 12 07 01 02
  word Tsid;
  word Onid;
  word Sid;
  byte U48[4];
  dword ProviderFlag2;
  byte U56[8];
  byte ChannelName[RecordLength - 60];
};

public struct Pa_idtvChannel_bin
{
  word u0;
  word u2;
  word numRecords;
  word u6;
  byte md5[16];
  Pa_idtvChannel_bin_FileEntry fileEntries[numRecords];
};
