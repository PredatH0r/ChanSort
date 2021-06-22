#include "chansort.h"

/***********************************************************
 * Philips ChannelMap_45 format
 ***********************************************************/

public struct Ph_ChannelMap45_CableDbBin
{
  dword majorVersion;
  dword minorVersion;
  dword recordCount;
  struct
  {
    var off0 = current_offset;
    dword id;
    dword freq;
    dword number;
    dword isDigital;
    dword onid;
    dword tsid;
    dword sid;
    dword symRate;
    dword logoNr;
    dword isEncrypted;
    dword locked;
    dword modulation;
    dword unk1;
    dword serviceType;
    dword systemHideMaybe;
    dword isUserModifiedLogo;
    dword serviceEdit;
    dword streamPriorityMaybe;
    dword unk2;
    word unk3TransponderRelated;
    word unk4TransponderRelated;
	byte unk3[8];
    wchar_t name[32+1];

    byte unk[156 - (current_offset - off0)];
  } Channels[recordCount];
};
 

public struct Ph_ChannelMap45_SatelliteDbBin
{
  dword majorVersion;
  dword minorVersion;
  dword recordCount;
  struct
  {
    var off0 = current_offset;
    dword id;
    dword freq;
    dword number;
    dword isDigital;
    dword onid;
    dword tsid;
    dword sid;
    dword symRate;
    dword logoNr;
    dword isEncrypted;
    dword locked;
    dword modulation;
    dword unk1;
    dword serviceType;
    dword systemHideMaybe;
    dword isUserModifiedLogo;
    dword serviceEdit;
    dword streamPriorityMaybe;
    dword polarity;
    word unk3TransponderRelated;
    word unk4TransponderRelated;
    wchar_t name[32+1];
    wchar_t satName[32+1];

    byte unk[212 - (current_offset - off0)];
  } Channels[recordCount];
};
