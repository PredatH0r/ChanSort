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
    Any = 0,

    // bit 0-1, 7: analog/dvb/ip
    Analog = 0x0001,
    Dvb = 0x0002,
    Ip = 0x0080,
    MaskBcastSystem = Analog | Dvb | Ip,

    // bit 3-6: AvInput/Antenna/Cable/Sat
    AvInput = 0x0008,
    Antenna = 0x0010,
    Cable = 0x0020,
    Sat = 0x0040,
    MaskBcastMedium = AvInput | Antenna | Cable | Sat,


    MaskBcast = MaskBcastSystem | MaskBcastMedium,


    // bit 8-10: TV/Radio/Data
    Tv = 0x0100,
    Radio = 0x0200,
    Data = 0x0400,
    MaskTvRadioData = Tv | Radio | Data,
    TvAndData = Tv | Data,

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

    // some predefined combinations

    AnalogC = Analog | Cable,
    AnalogT = Analog | Antenna,
    AnalogCT = Analog | Cable | Antenna,
    DvbC = Dvb | Cable,
    DvbT = Dvb | Antenna,
    DvbCT = Dvb | Cable | Antenna,
    DvbS = Dvb | Sat,

    IpAntenna = Ip | Antenna,
    IpCable = Ip | Cable,
    IpSat = Ip | Sat,
    IpAll = Ip | Antenna | Cable | Sat,



    CablePrimeD = Dvb | Cable | CablePrime,
    HdPlusD = Dvb | Sat | AstraHdPlus,
    FreesatD = Dvb | Sat | Freesat,
    TivuSatD = Dvb | Sat | TivuSat,
    CanalDigitalSatD = Dvb | Sat | CanalDigital,
    DigitalPlusD = Dvb | Sat | DigitalPlus,
    CyfraPlusD = Dvb | Sat | CyfraPlus,

    All = MaskBcastSystem | MaskBcastMedium | MaskTvRadioData
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
