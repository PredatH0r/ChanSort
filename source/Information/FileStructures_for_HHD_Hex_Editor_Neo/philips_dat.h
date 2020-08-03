#include "chansort.h"

struct Ph_NextPrevTableEntry
{
  word next;
  word prev;
};

struct Ph_Channel
{
  word pcrPid;
  word unk1;
  word onid;
  word tsid;
  word sid;
  word unk3;
  word unk4;
  word unk5;
  word vpid;
  word unk6;
  word progNr;
  word unk7;
  word unk8;
  word unk9;
  char channelName[32];
  char providerName[32];
  char unk9[40];
};

public struct Ph_ServiceDat 
{
  word unk1;
  word chanCount;
  word chanSize;
  word unk2;
  word chanCount2;
  word unk3;
  Ph_NextPrevTableEntry NextPrevTable[chanCount];
  Ph_Channel Channels[chanCount];
  dword crc32;
};
