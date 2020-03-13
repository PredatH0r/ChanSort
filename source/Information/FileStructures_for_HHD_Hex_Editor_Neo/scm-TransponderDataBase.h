#include "chansort.h"

// 49 = B Series
// 45 = C-J Series

struct SCM_TransponderDataBase_B_entry
{
    byte MagicByte;
    word TransponderIndex;
    word Unknown3;
    word SatelliteIndex;
    word Unknown7;
    dword Frequency;
    byte Unknown13[4];
    word SymbolRate;
    byte Unknown19[2];
    byte Polarity;
    byte Unknown22[27];
};

public struct SCM_TransponderDataBase_B
{
    dword Count;
    SCM_TransponderDataBase_B_entry Entries[*];
};


struct SCM_TransponderDataBase_CDEFHJ_entry
{
    byte MagicByte;
    word TransponderIndex;
    word Unknown3;
    word SatelliteIndex;
    word Unknown7;
    dword Frequency;
    word SymbolRate;
    byte Unknown15[2];
    byte Polarity;
    byte Unknown18[27];
};

public struct SCM_TransponderDataBase_CDEFHJ
{
    dword Count;
    SCM_TransponderDataBase_CDEFHJ_entry Entries[*];
};


