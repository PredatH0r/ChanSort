#include "chansort.h"

struct TLL_HotelSettings
{
  byte HotelModeActive;
  byte PowerOnStatus;
  byte SetupMenuDisplay;
  byte ProgramChange;
  byte InputSourceChange;
  byte MenuDisplay;
  byte OsdDisplay;
  byte LgIrOperation;
  byte LocalKeyOp;
  byte MaxVolume;
  byte DtvChannelUpdate;
  byte PowerOnDefault;
  byte InputSource;
  word Programme;
  byte Volume;
  byte AvSettings;
  byte RadioVideoBlank;
  byte unknown1;
  byte StartProgNr;
  byte unknown2;
  byte NumberOfPrograms;
  byte RadioNameDisplay;
  byte unknown3[2];
  byte AccessCode[4];
};

enum TLL_SignalSource : byte
{
  Analog_T = 0,
  DVB_T = 1,
  Analog_C = 2,
  DVB_C = 3,
  DVB_S = 7
};

enum LH_SignalSource : byte
{
  //Antenna = 2,
  //Cable = 3
  Analog_T = 0,
  DVB_T = 1,
  Analog_C = 2,
  DVB_C = 3,
  DVB_S = 7
};

enum TLL_EditFlags : byte
{
  Lock = 1,
  Skip = 2,
  Hide = 3,
  CustChNr = 64
};

struct TLL_SettingsBlock
{
  dword BlockSize;
  byte Data[BlockSize]; 
};
