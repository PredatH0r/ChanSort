using System;

namespace ChanSort.Api
{
  [Flags]
  public enum SignalSource
  {
    Analog = 0x00,
    Digital = 0x10,

    Cable = 0x01,
    Antenna = 0x02,
    Sat = 0x04,
    HdPlus = 0x08,

    AnalogC=Analog + Cable, 
    AnalogT=Analog + Antenna, 
    AnalogCT=Analog + Cable + Antenna, 
    DvbC = Digital + Cable, 
    DvbT= Digital + Antenna, 
    DvbCT= Digital + Cable + Antenna, 
    DvbS= Digital + Sat,
    HdPlusD = Digital + HdPlus
  }
  public enum SignalType { Tv, Radio, Mixed }

  [Flags]
  public enum Favorites : byte { A = 0x01, B = 0x02, C = 0x04, D = 0x08, E = 0x10 }

  public enum UnsortedChannelMode
  {
    AppendInOrder,
    AppendAlphabetically,
    Hide
  }
}
