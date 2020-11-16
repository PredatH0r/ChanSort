#include "chansort.h"

/***********************************************************
 * Philips Repair/ChannelLst/chanLst.bin
 ***********************************************************/
 
struct Ph_chanLst_bin_FileEntry
{
  dword fileNameLength;
  char fileName[fileNameLength];
  if (fileName[0] != '/')
    word crc16modbus;
};

public struct Ph_chanLst_bin
{
  word versionMinor;
  word versionMajor;
  byte unknown4[14];
  dword modelNameLen;
  char modelName[modelNameLen];
  Ph_chanLst_bin_FileEntry fileEntries[*];
};
