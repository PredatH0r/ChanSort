typedef unsigned char byte;
typedef unsigned short word;
typedef unsigned int dword;

struct SCM_SatDataBase_entry
{
  byte Magic0x55;
  dword SatNumber;
  dword TransponderCount;
  word Name[64];
  dword IsEast;
  dword LongitudeTimes10;
};

public struct SCM_SatDataBase
{
  dword Version;
  SCM_SatDataBase_entry Satellites[*];
};


struct SCM_TransponderDataBase_entry
{
  byte Magic0x55;
  dword TransponderNr;
  dword SatelliteNr;
  dword Frequency;
  dword SymbolRate;
  dword IsVerticalPolarity;
  dword Modulation;
  dword CodeRate;
  dword Unknown1;
  dword Unknown2;
  dword Unknown3;
  dword Unknown4;
};

public struct SCM_TransponderDatabase
{
  dword Version;
  SCM_TransponderDataBase_entry Entries[*];
};


struct SCM_PtcCable_entry
{
  dword Unknown0;
  float Freqency;
  word ChannelNr;
  byte Unknown11;
  byte Unknown12;
};

public struct SCM_PtcCable_file
{
  SCM_PtcCable_entry Entries[*];
};