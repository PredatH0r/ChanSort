#include "chansort.h"

/***********************************************************
 * Panasonic LS/LX 2022 idtvChannel.bin
 ***********************************************************/

struct Flags
{
  dword SimpleChNum:1; // 0x0001
  dword Encrypted:1; // 0x0002
  dword HasAudio:1; // 0x0004
  dword HasVideo:1; // = 0x0008
  dword IsHidden:1; // = 0x0010
  dword EpgHidden:1; // = 0x0020
  dword OneSegment:1; // = 0x0040
  dword IsFavorite:1; // = 0x0080
  dword Deleted:1; // = 0x0100
  dword Locked:1; // 0x0200
  dword Skip:1; // = 0x0400
  dword Added:1; // = 0x0800
  dword Moved:1; // = 0x1000
  dword Fake:1; // = 0x2000
  dword x0000_4000:1;
  dword x0000_8000:1;
  dword IsRadio:1; // 0x0001 0000
  dword x0002_0000:1; // 0x0002 0000
  dword IsData:1; // 0x0004 0000
  dword x0008_0000:1; // 0x0008 0000
  dword x0010_0000:1; // 0x0010 0000
  dword x0020_0000:1; // 0x0020 0000
  dword x0040_0000:1;
  dword x0080_0000 : 1;
  dword xFF00_0000 : 8;
};

struct Pa_idtvChannel_bin_FileEntry
{
  word RecordType; // always 1 (for DVB?)
  word RecordLength; // some series specific fixed structure (with different sizes) + length of channel name
  var off0 = current_offset;
  byte U4; // always 6
  dword U5; // all 00
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
  byte U38[2]; // 01 02
  word Tsid;
  word Onid;
  word Sid;
  byte UserChannelNameLength;
  byte ChannelNameLength;
  word FrontendType;
  dword ProviderFlag2;
if (RecordLength - UserChannelNameLength - ChannelNameLength == 58)
  byte U56[6];
else if (RecordLength - UserChannelNameLength - ChannelNameLength == 60)
  byte U56[8];
else if (RecordLength - UserChannelNameLength - ChannelNameLength == 64)
  byte U56[6];
else if (RecordLength - UserChannelNameLength - ChannelNameLength == 66)
  byte U56[8];
else
  $assert(false, "Structure for this file size is not supported");

  byte UserChannelName[UserChannelNameLength];
  byte ChannelName[ChannelNameLength];
  var off1 = current_offset;
  byte Extra[RecordLength - (off1 - off0)];
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
