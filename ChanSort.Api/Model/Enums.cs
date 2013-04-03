using System;

namespace ChanSort.Api
{
  /// <summary>
  /// Bitmask for channel and list classification.
  /// An individual channel can only have one bit of each group set.
  /// A ChannelList can have multiple bits set to indicate which type of channels it can hold.
  /// </summary>
  [Flags]
  public enum SignalSource
  {
    // bit 1+2: analog/digital
    Analog = 0x0001,
    Digital = 0x0002,

    // bit 5+6+7: Antenna/Cable/Sat
    Antenna = 0x0010,
    Cable = 0x0020,
    Sat = 0x0040,

    // bit 9+10: TV/Radio
    Tv = 0x0100,
    Radio = 0x0200,
    
    // bit 13-16: Preset list selector (AstraHD+, Freesat, TivuSat, CanalDigitalSat, ... for Samsung)
    StandardSat = 0 << 24,
    HdPlus = 1 << 24,
    Freesat = 2 << 24,
    TivuSat = 3 << 24,
    CanalDigital = 4 << 24,

    AnalogC=Analog + Cable, 
    AnalogT=Analog + Antenna, 
    AnalogCT=Analog + Cable + Antenna, 
    DvbC = Digital + Cable, 
    DvbT= Digital + Antenna, 
    DvbCT= Digital + Cable + Antenna, 
    DvbS= Digital + Sat,
    HdPlusD = Digital + HdPlus
  }

  public enum SignalType { Tv = SignalSource.Tv, Radio = SignalSource.Radio, Mixed = SignalSource.Tv|SignalSource.Radio }

  [Flags]
  public enum Favorites : byte { A = 0x01, B = 0x02, C = 0x04, D = 0x08, E = 0x10 }

  public enum UnsortedChannelMode
  {
    AppendInOrder,
    AppendAlphabetically,
    Hide
  }
}
