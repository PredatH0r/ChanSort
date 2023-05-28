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
    // 0x0000 // 0000000000000000
    Any = 0,

    // 0x0001 // 0000000000000001
    Analog = 1,
    // 0x0002 // 0000000000000010
    Digital = 2,
    // 0x0003 // 0000000000000011
    MaskAnalogDigital = Analog | Digital,                               // bit 0-1: analog/digital

    // 0x0008 // 0000000000001000
    AVInput = 8,
    AvInput = 8,
    // 0x0010 // 0000000000010000
    Antenna = 16,
    // 0x0020 // 0000000000100000
    Cable = 32,
    // 0x0040 // 0000000001000000
    Sat = 64,
    // 0x0078 // 0000000001111000
    MaskAntennaCableSat = AVInput | Antenna | Cable | Sat,              // bit 3-6: AvInput/Antenna/Cable/Sat

    // 0x0080 // 0000000010000000                                       // bit 7: ANTENNA>IP/CABLE>IP/SAT>IP
    IP = 128,

    // 0x0011 // 0000000000010001
    AnalogAntenna = Analog + Antenna,
    AnalogT = Analog + Antenna,
    // 0x0021 // 0000000000100001
    AnalogCable = Analog + Cable,
    AnalogC = Analog + Cable,
    // 0x0031 // 0000000000110001
    AnalogAntennaCable = Analog + Antenna + Cable,
    AnalogCT = Analog + Cable + Antenna,
    // 0x0041 // 0000000001000001
    AnalogSat = Analog + Sat,
    // 0x0012 // 0000000000010010
    DVBT = Digital + Antenna,
    DvbT = Digital + Antenna,
    // 0x0022 // 0000000000100010
    DVBC = Digital + Cable,
    DvbC = Digital + Cable,
    // 0x0032 // 0000000000110010
    DVBTC = Digital + Antenna + Cable,
    DvbCT = Digital + Cable + Antenna,
    // 0x0042 // 0000000001000010
    DVBS = Digital + Sat,
    DvbS = Digital + Sat,
    // 0x0072 // 0000000001110010
    DVBAll = Digital + Antenna + Cable + Sat,
    // 0x0092 // 0000000010010010
    DVBIPAntenna = Digital + Antenna + IP,
    // 0x00A2 // 0000000010100010
    DVBIPCable = Digital + Cable + IP,
    // 0x00C2 // 0000000011000010
    DVBIPSat = Digital + Sat + IP,
    SatIP = Digital + Sat + IP,

    // 0x00FB // 0000000011111011
    AllAnalogDigitalInput = MaskAnalogDigital | MaskAntennaCableSat,
    // 0x00FB // 0000000011111011
    MaskAdInput = MaskAnalogDigital | MaskAntennaCableSat,              // bit 0-7: Combination of above

    // 0x0100 // 0000000100000000
    TV = 256,
    Tv = 256,
    // 0x0200 // 0000001000000000
    Radio = 512,
    // 0x0400 // 0000010000000000
    Data = 1024,
    // 0x0300 // 0000001100000000
    TVAndRadio = TV | Radio,
    // 0x0500 // 0000010100000000
    TVAndData = TV | Data,
    TvAndData = TV | Data,
    // 0x0600 // 0000011000000000
    RadioAndData = Radio | Data,
    // 0x0700 // 0000011100000000
    TVAndRadioAndData = TV | Radio | Data,
    // 0x0700 // 0000011100000000
    MaskTVRadioData = TV | Radio | Data,                                // bit 8-10: TV/Radio/Data
    MaskTvRadioData = TV | Radio | Data,

    // 0x07FB // 0000011111111011
    All = MaskAnalogDigital | MaskAntennaCableSat | MaskTVRadioData,

    // bit 12-15: Preset list selector (AstraHD+, Freesat, TivuSat, CanalDigitalSat, ... for Samsung)
    MaskProvider = 0xF000,
    Provider0 = 0 << 12,
    Provider1 = 1 << 12,
    Provider2 = 2 << 12,
    Provider3 = 3 << 12,
    Provider4 = 4 << 12,

    StandardSat = 0 << 12,
    AstraHdPlus = 1 << 12,
    Freesat = 2 << 12,
    TivuSat = 3 << 12,
    CanalDigital = 4 << 12,
    DigitalPlus = 5 << 12,
    CyfraPlus = 6 << 12,
    
    StandardCable = 0 << 12,
    CablePrime = 1 << 12,

    CablePrimeD = Digital + Cable + CablePrime,
    HdPlusD = Digital + Sat + AstraHdPlus,
    FreesatD = Digital + Sat + Freesat,
    TivuSatD = Digital + Sat + TivuSat,
    CanalDigitalSatD = Digital + Sat + CanalDigital,
    DigitalPlusD = Digital + Sat + DigitalPlus,
    CyfraPlusD = Digital + Sat + CyfraPlus,
  }
  #endregion

  #region enum Favorites
  [Flags]
  public enum Favorites : byte { A = 0x01, B = 0x02, C = 0x04, D = 0x08, E = 0x10, F=0x20, G=0x40, H=0x80 }
  #endregion

  #region enum FavoritesMode
  public enum FavoritesMode
  {
    NotInitialized = 0,
    None = 1,
    Flags = 2,
    OrderedPerSource = 3,
    MixedSource = 4
  }
  #endregion

  #region enum UnsortedChannelMode
  public enum UnsortedChannelMode
  {
    AppendInOrder=0,
    AppendAlphabetically=1,
    Delete=2
  }
  #endregion

  #region ChannelNameEditMode
  [Flags]
  public enum ChannelNameEditMode
  {
    None = 0x00,
    Analog = 0x01,
    Digital = 0x02,
    All = Analog|Digital
  }
  #endregion
}
