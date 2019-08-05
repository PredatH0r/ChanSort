using System;

namespace ChanSort.Api
{
  #region enum SignalSource
  /// <summary>
  /// Bitmask for channel and list classification.
  /// An individual channel can only have one bit of each group set.
  /// A ChannelList can have multiple bits set to indicate which type of channels it can hold.
  /// </summary>
  [Flags]
  public enum SignalSource
  {
    // bit 1+2: analog/digital
    MaskAnalogDigital = 0x0003,
    Analog = 0x0001,
    Digital = 0x0002,

    // bit 4+5+6+7+8: AvInput/Antenna/Cable/Sat/IP
    MaskAntennaCableSat = 0x00F8,
    AvInput = 0x0008,
    Antenna = 0x0010,
    Cable = 0x0020,
    Sat = 0x0040,
    IP = 0x0080,

    MaskAdInput = MaskAnalogDigital | MaskAntennaCableSat,

    // bit 9+10: TV/Radio
    MaskTvRadio = 0x0300,
    Tv = 0x0100,
    Radio = 0x0200,
    TvAndRadio = Tv | Radio,

    // bit 13-16: Preset list selector (AstraHD+, Freesat, TivuSat, CanalDigitalSat, ... for Samsung)
    MaskProvider = 0xFC00,
    StandardSat = 0 << 12,
    AstraHdPlus = 1 << 12,
    Freesat = 2 << 12,
    TivuSat = 3 << 12,
    CanalDigital = 4 << 12,
    DigitalPlus = 5 << 12,
    CyfraPlus = 6 << 12,

    StandardCable = 0 << 12,
    CablePrime = 1 << 12,

    AnalogC = Analog + Cable,
    AnalogT = Analog + Antenna,
    AnalogCT = Analog + Cable + Antenna,
    DvbC = Digital + Cable,
    DvbT = Digital + Antenna,
    DvbCT = Digital + Cable + Antenna,
    DvbS = Digital + Sat,
    SatIP = Digital + Sat + IP,

    CablePrimeD = Digital + Cable + CablePrime,
    HdPlusD = Digital + Sat + AstraHdPlus,
    FreesatD = Digital + Sat + Freesat,
    TivuSatD = Digital + Sat + TivuSat,
    CanalDigitalSatD = Digital + Sat + CanalDigital,
    DigitalPlusD = Digital + Sat + DigitalPlus,
    CyfraPlusD = Digital + Sat + CyfraPlus,

    All = MaskAnalogDigital | MaskAntennaCableSat | MaskTvRadio
  }
  #endregion

  [Flags]
  public enum Favorites : byte { A = 0x01, B = 0x02, C = 0x04, D = 0x08, E = 0x10 }

  public enum UnsortedChannelMode
  {
    AppendInOrder=0,
    AppendAlphabetically=1,
    MarkDeleted=2
  }

  [Flags]
  public enum ChannelNameEditMode
  {
    None = 0x00,
    Analog = 0x01,
    Digital = 0x02,
    All = Analog|Digital
  }
}
